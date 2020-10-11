// dllmain.cpp : Define el punto de entrada de la aplicaci√≥n DLL.
#include "pch.h"
#include <stdio.h>
#include <WinSock2.h>
#include "../../CryptoPP/cryptlib.h"
#include "../../CryptoPP/modes.h"
#include "../../CryptoPP/aes.h"
#pragma comment(lib,"../../CryptoPP/Win32/Output/Release/cryptlib.lib")

LPBYTE _GetStartupInfoA = (LPBYTE)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetStartupInfoA");

template<typename T>
class Hook
{
    void* targetAddress;
    BYTE copy[5];
public:
    static DWORD VPOld;
    Hook()
    { }
    Hook(void* oldFunc, void* newFunc)
    {
        this->Set(oldFunc, newFunc);
    }

    T Set(void * oldFunc, void * newFunc)
    {
        memcpy(copy, oldFunc, sizeof(copy));
        Jump(oldFunc, newFunc);

        targetAddress = oldFunc;

        return (T)(copy + 1);
    }
    void Release()
    {
        VirtualProtect(targetAddress, sizeof(copy), PAGE_EXECUTE_READWRITE, &VPOld);
        memcpy(targetAddress, copy, sizeof(copy));
        VirtualProtect(targetAddress, sizeof(copy), VPOld, &VPOld);
    }

    static void Jump(void* offset, void* target)
    {
        BYTE newJump[5] = { 0xE9, 0, 0, 0, 0 };
        VirtualProtect(offset, sizeof(newJump), PAGE_EXECUTE_READWRITE, &VPOld);
        *((DWORD*)&newJump[1]) = ((DWORD)target) - ((DWORD)offset) - 5;
        memcpy(offset, newJump, sizeof(newJump));
        VirtualProtect(offset, sizeof(newJump), VPOld, &VPOld);
    }

    static void Call(void* offset, void* target)
    {
        BYTE newJump[5] = { 0xE8, 0, 0, 0, 0 };
        VirtualProtect(offset, sizeof(newJump), PAGE_EXECUTE_READWRITE, &VPOld);
        *((DWORD*)&newJump[1]) = ((DWORD)target) - ((DWORD)offset) - 5;
        memcpy(offset, newJump, sizeof(newJump));
        VirtualProtect(offset, sizeof(newJump), VPOld, &VPOld);
    }

    static void Write(void* offset, BYTE array[], int size)
    {
        VirtualProtect(offset, size, PAGE_EXECUTE_READWRITE, &VPOld);
        memcpy(offset, array, size);
        VirtualProtect(offset, size, VPOld, &VPOld);
    }

    static void WriteByte(void* offset, BYTE bt)
    {
        VirtualProtect(offset, 1, PAGE_EXECUTE_READWRITE, &VPOld);
        *((LPBYTE)offset) = bt;
        VirtualProtect(offset, 1, VPOld, &VPOld);
    }
};
template<typename T>
DWORD Hook<T>::VPOld = 0;

typedef void(_stdcall* T_GetStartupInfoA)(LPSTARTUPINFOA lpStartupinfo);
typedef void(*T_WZSend)(LPBYTE, DWORD, int, int);
typedef void(*T_WZRecv)(LPBYTE,int,int);
typedef void(*T_ProcCoreA)(int, int, BYTE*, int, int);
typedef void(*T_ProcCoreB)(DWORD, BYTE*, DWORD, DWORD);

Hook<T_GetStartupInfoA> kernel32_GetStartupInfoA;
Hook<T_WZSend> main_Send;
Hook<T_WZRecv> main_Recv;
T_GetStartupInfoA old_GetStartupInfoA;

// MU Online Season 12 V1.18.70 Offsets
auto old_Send = (T_WZSend)0x00BAEBDD;
auto old_Recv = (T_WZRecv)0x00C19CF5;
auto ProtocolCoreA = (T_ProcCoreA)0xBE50E1;
auto ProtocolCoreB = (T_ProcCoreB)0xC150A9;
auto PARSE_PACKET_STREAM = (FARPROC)0xBAFACC;
//auto MU_SENDER_CLASS = (DWORD*)0x159F3E4; // S12
auto MU_SEND_PACKET = (DWORD)0x00BAF008; // S12

auto szIP = (char*)0x01596520;
auto iPort = (int*)0x01595A54;
auto szClientVersion = (char*)0x0159F3C8;
auto szClientSerial = szClientVersion + 8;
const char* cfgFile = ".\\config.ini";
auto SerialOut = (LPBYTE)0xA17F99C;

FILE* fp;
bool SendHooked = false;
CryptoPP::ECB_Mode<CryptoPP::AES>::Encryption m_Encryption;
CryptoPP::ECB_Mode<CryptoPP::AES>::Decryption m_Decryption;

inline void PacketPrint(LPBYTE buff, DWORD size)
{
    static BYTE DecBuff[7024];
    for (unsigned int i = 0; i < size; i++)
    {
        char hex[] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', '∑' };
        auto a = (buff[i] >> 4) & 0x0f;
        DecBuff[i * 3 + 0] = hex[a];
        a = buff[i] & 0x0f;
        DecBuff[i * 3 + 1] = hex[a];
        DecBuff[i * 3 + 2] = ' ';
        DecBuff[i * 3 + 3] = 0;
    }
    fprintf(fp, "     PACKET:%s\n", DecBuff);
}

inline void DataSend(BYTE* buff, DWORD len)
{
    __asm
    {
        PUSH len;
        PUSH buff;
        MOV ECX, DWORD PTR DS : [0x159F3E4] ;//MU SENDER CLASS
        CALL MU_SEND_PACKET;
    }
}

void SendPacket(BYTE* lpMsg, DWORD len, int enc, int unk1)
{
    static BYTE send[8192];
    auto buff = send;
    DWORD size;

    fprintf(fp, "SendPacket(0x%08X, 0x%08X, %d, %d)\n", (DWORD)lpMsg, len, enc, unk1);
    PacketPrint(lpMsg, len);

    if (enc)
    {
        BYTE offset = lpMsg[0] == 0xC1 ? 2 : 3;
        size = len;
        size -= offset;
        int newSize = (1 + (size / 16)) * 16;
        m_Encryption.ProcessData(send + offset, lpMsg+offset, newSize);
        send[newSize + offset] = (BYTE)(newSize - size);
        size = newSize + offset + 1;

        if (lpMsg[0] == 0xC1)
        {
            send[0] = 0xC3;
            send[1] = (BYTE)size;
        }else
        {
            send[0] = 0xC4;
            send[1] = (BYTE)(size >> 8) & 0xff;
            send[2] = (BYTE)(size) & 0xff;
        }
        printf("     RawSize: %d, DataSize %d, EncSize %d, PaddingSize %d", len, newSize, size, send[newSize + offset]);
        PacketPrint(send, size);
    }
    else
    {
        memcpy(send, lpMsg, len);
        size = len;
    }

    fflush(fp);
    DataSend(send, size);
}

void ParsePacket(void* PackStream, int unk1, int unk2)
{
    static BYTE DecBuff[7024];
    BYTE* buff;
    unsigned int DecSize;

    while (true)
    {
        __asm {
            MOV ECX, PackStream;
            MOV EDX, PARSE_PACKET_STREAM;
            CALL EDX;
            MOV buff, EAX;
        }
        if (!buff)
            break;


        int proto;
        int size;
        int enc;

        switch (buff[0])
        {
        case 0xC1:
            proto = buff[2];
            size = buff[1];
            enc = 0;
            break;
        case 0xC2:
            proto = buff[3];
            size = buff[1]*16+ buff[2];
            enc = 0;
            break;
        case 0xC3:
            enc = 1;
            size = buff[1];
            DecSize = size - buff[size-1];
            m_Decryption.ProcessData(&DecBuff[1], &buff[2], size - 3);
            DecBuff[0] = 0xC1;
            DecBuff[1] = DecSize + 2;
            size = DecSize + 2;
            buff = DecBuff;
            proto = DecBuff[2];
            break;
        case 0xC4:
            enc = 1;
            size = MAKEWORD(buff[2], buff[1]);
            DecSize = size - buff[size - 1];
            m_Decryption.ProcessData(&DecBuff[2], &buff[3], size - 4);
            DecBuff[0] = 0xC2;
            DecBuff[2] = LOBYTE(DecSize + 3);
            DecBuff[1] = HIBYTE(DecSize + 3);
            size = DecSize + 3;
            buff = DecBuff;
            proto = buff[3];
            break;
        }
        fprintf(fp, "proto:0x%02X, buff:0x%08X, size:%d, enc:%d\n", proto, (DWORD)buff, size, enc);
        PacketPrint(buff, size);
        fflush(fp);
        if (unk1 == 1)
            ProtocolCoreA(unk2, proto, buff, size, enc);
        else
            ProtocolCoreB(proto, buff, size, enc);
    }
}

void ProcLoading()
{
    fp = fopen("ParsePacket.log", "a+");
    BYTE key[] = { 0x44, 0x9D, 0x0F, 0xD0, 0x37, 0x22, 0x8F, 0xCB, 0xED, 0x0D, 0x37, 0x04, 0xDE, 0x78, 0x00, 0xE4, 0x33, 0x86, 0x20, 0xC2, 0x79, 0x35, 0x92, 0x26, 0xD4, 0x37, 0x37, 0x30, 0x98, 0xEF, 0xA4, 0xDE };

    m_Encryption.SetKey(key, sizeof(key));
    m_Decryption.SetKey(key, sizeof(key));

    old_Send = main_Send.Set(old_Send, SendPacket);
    old_Recv = main_Recv.Set(old_Recv, ParsePacket);

    Hook<void>::Jump((void*)0x00BAEEC5, (void*)0x0A3A86EF);
    Hook<void>::Jump((void*)0x0A327E33, (void*)0x00BEAA7F);
    Hook<void>::Call((void*)0x0051087E, (void*)0x0A317ED0);
    Hook<void>::Call((void*)0x00511238, (void*)0x0A317ED0);
    Hook<void>::Call((void*)0x0051196A, (void*)0x0A317ED0);
    Hook<void>::Call((void*)0x00511DB6, (void*)0x0A317ED0);

    BYTE a[] = { 0xEB, 0x4B };
    Hook<void>::Write((void*)0x005069DC, a, sizeof(a));

    BYTE b[] = { 0xE9,0xBA,0x00,0x00,0x00,0x90 };
    Hook<void>::Write((void*)0x00506E1E, b, sizeof(b));

    // Disable Log encoder
    BYTE c[32];
    memset(c, 0x90, 32);
    Hook<void>::Write((void*)0x00D42114, c, sizeof(c));

    BYTE GG_JMP[] = { 0xE9,0x88,0x00,0x00,0x00,0x90 };
    Hook<void>::Write((void*)0x00507524, GG_JMP, sizeof(GG_JMP));//1.18.70
    BYTE GG_JMP1[] = { 0xEB,0x19 };
    Hook<void>::Write((void*)0x005074E1, GG_JMP1, sizeof(GG_JMP1));//1.18.70

    //Remove GameGuard
    Hook<void>::WriteByte((void*)0x0050CFD2, 0xEB);//1.18.70
    Hook<void>::WriteByte((void*)0x00CC296F, 0xEB);//1.18.70

    Hook<void>::WriteByte((void*)0x00CC2AA8, 0xEB);//1.18.70
    //??
    Hook<void>::WriteByte((void*)0x015964E0, 0x86);//1.18.70
    //??????
    Hook<void>::Write((void*)0x00460DE2, c, 13);//1.18.70

    Hook<void>::WriteByte((void*)0x004BC12C, 0xEB);
    Hook<void>::WriteByte((void*)0x00AD5F93, 0xEB);
    Hook<void>::WriteByte((void*)0x00AD5F94, 0x43);
    Hook<void>::WriteByte((void*)0x00B100D2, 0xEB);
    Hook<void>::Write((void*)0x00C7B11C, c, sizeof(2));//1.18.70

    //?ItemtooltipBmd
    Hook<void>::WriteByte((void*)0x0085216E, 0xEB);//1.18.70
    //?itemsetoptiontext
    Hook<void>::WriteByte((void*)0x00529b6c, 0xEB);//1.18.70
    //masterskillTooltip
    Hook<void>::WriteByte((void*)0x00b02eb5, 0xEB);//1.18.70
    //SkillToolTipText
    BYTE SKILL_JMP[] = { 0xE9,0xAD,0x00,0x00,0x00,0x90 };
    Hook<void>::Write((void*)0x00CCA2F8, SKILL_JMP, sizeof(SKILL_JMP));//1.18.70

    GetPrivateProfileStringA("MU", "URL", "127.0.0.1", szIP, 256, cfgFile);
    GetPrivateProfileStringA("MU", "Serial", "fughy683dfu7teqg", szClientSerial, 18, cfgFile);
    *iPort = GetPrivateProfileIntA("MU", "Port", 44405, cfgFile);

    kernel32_GetStartupInfoA.Release();
}

void _stdcall new_GetStartupInfoA(LPSTARTUPINFOA lpStartupinfo)
{
    ProcLoading();

    GetStartupInfoA(lpStartupinfo);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        old_GetStartupInfoA = kernel32_GetStartupInfoA.Set(_GetStartupInfoA, new_GetStartupInfoA);
        break;
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        fclose(fp);
        break;
    }
    return TRUE;
}

extern "C" _declspec(dllexport) void Init()
{
}