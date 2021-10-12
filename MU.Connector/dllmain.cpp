// dllmain.cpp : Define el punto de entrada de la aplicaci√≥n DLL.
#include "pch.h"
#include <stdio.h>
#include <WinSock2.h>
#include <windows.h>
#include <DBGHELP.H>
#include "../../CryptoPP/cryptlib.h"
#include "../../CryptoPP/modes.h"
#include "../../CryptoPP/aes.h"
#include "Offsets.h"
#include "Protocol.h"
#pragma comment(lib,"../../CryptoPP/Win32/Output/Release/cryptlib.lib")

LPBYTE _GetStartupInfoA = (LPBYTE)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetStartupInfoA");
LPBYTE _OutputDebugStringA = (LPBYTE)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "OutputDebugStringA");

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

    static void ChangeAddress(DWORD Addr, DWORD AddrNew)
    {
        DWORD OldProtect;
        VirtualProtect((LPVOID)Addr, 4, PAGE_EXECUTE_READWRITE, &OldProtect);
        __asm
        {
            MOV EAX, Addr;
            MOV EDX, AddrNew;
            MOV DWORD PTR DS : [EAX] , EDX;
        }
        VirtualProtect((LPVOID)Addr, 4, OldProtect, &OldProtect);
    }
};
template<typename T>
DWORD Hook<T>::VPOld = 0;

Hook<T_GetStartupInfoA> kernel32_GetStartupInfoA;
Hook<T_OutputDebugStringA> kernel32_OutputDebugStringA;
Hook<T_WZSend> main_Send;
Hook<T_WZRecv> main_Recv;
Hook<T_sprintf> main_sprintf;
T_GetStartupInfoA old_GetStartupInfoA;
T_OutputDebugStringA old_OutputDebugStringA;

// Crypto
CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Encryption m_Encryption;
CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Decryption m_Decryption;

// Functions
auto old_Send = (T_WZSend)SEND_PACKET_HOOK;// 0x00BAEBDD;
auto old_Recv = (T_WZRecv)PARSE_PACKET_HOOK;// 0x00C19CF5;
auto ProtocolCoreA = (T_ProcCoreA)PROTOCOL_CORE1;// 0xBE50E1;
auto ProtocolCoreB = (T_ProcCoreB)PROTOCOL_CORE2;// 0xC150A9;
auto old_sprintf = (T_sprintf)0x00D0A18A;

const char* cfgFile = ".\\config.ini";

FILE* fp;
bool SendHooked = false;

void BuxConvert(char* buf, int size)
{
    static unsigned char bBuxCode[3] = { 0xFC, 0xCF, 0xAB };
    for (int n = 0; n < size; n++)
    {
        buf[n] ^= bBuxCode[n % 3];		// Nice trick from WebZen
    }
}

inline void DataSend(BYTE* buff, DWORD len)
{
    __asm
    {
        PUSH len;
        PUSH buff;
        MOV ECX, DWORD PTR DS : [MU_SENDER_CLASS] ;//MU SENDER CLASS
        MOV EDX, MU_SEND_PACKET;
        CALL EDX;
    }
}

std::map<WORD, const char*> packetName =
{
    { 0xFF00, "Join" },
    { 0x06F4, "Server List" },
    { 0x03F4, "Server Info" },
    { 0x00F1, "JoinResult" },
    { 0x01F1, "Login" },
    { 0x00F3, "CharacterList" },
    { 0x03F3, "JoinMap2" },
    { 0x15F3, "JoinMap" },
    { 0x144E, "MuunRideViewPort" },
    { 0x03E7, "NPCInfo" },
    { 0xFF26, "HpUpdate" },
    { 0xFF27, "ManaUpdate" },
    { 0xFF13, "VPMonsterCreate" },
    { 0xFF0E, "LiveClient" },
    { 0xFF0F, "Weather" },
    { 0xFF0D, "Notice" },
    { 0xFF18, "Rotation" },
};

void Print(const char * format, ...)
{
    va_list ap;
    va_start(ap, format);
    vfprintf(fp, format, ap);
    vprintf(format, ap);
    va_end(ap);
}

inline void PacketPrint(LPBYTE buff, DWORD size, const char* szName, const char * szDesc)
{
    static char hex[] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', '∑' };
    static char DecBuff[100];
    unsigned int j, a;

    Print("%s PACKET %s [%d]={0x", szDesc, szName, size);
    for (unsigned int i = 0; i < size; i++)
    {
        j = 0;
        a = (buff[i] >> 4) & 0x0f;
        DecBuff[j++] = hex[a];
        a = buff[i] & 0x0f;
        DecBuff[j++] = hex[a];
        if (i + 1 < size)
        {
            DecBuff[j++] = ',';
            DecBuff[j++] = ' ';
            DecBuff[j++] = '0';
            DecBuff[j++] = 'x';
        }

        DecBuff[j++] = 0;

        fprintf(fp, DecBuff);
        printf(DecBuff);
    }

    Print("}\n");
}

void SendPacket(BYTE* lpMsg, DWORD len, int enc, int unk1)
{
    static BYTE send[8192];
    auto buff = send;
    DWORD size;

    BYTE type = lpMsg[0];

    WORD* p = (WORD*)(lpMsg + (type == 0xC1 ? 2 : 3));

    BYTE proto = (*p)&0xff;
    BYTE subco = ((*p)&0xff00)>>8;

    auto r = packetName.find(*p);
    if (r == packetName.end())
    {
        r = packetName.find(*p | 0xFF00);
    }
    if (r == packetName.end())
    {
        r = packetName.find(*p | 0xFF00);
    }

    if (r != packetName.end())
    {
        PacketPrint(lpMsg, len, (*r).second, "Send>");
    }
    else
    {
        PacketPrint(lpMsg, len, "unknow", "Send>");
    }


    switch (proto)
    {
        case 0xF1:
        {
            switch (subco)
            {
                case 0x01:
                {
#ifdef CLIENT_S9
                    PMSG_IDPASS_OLD* lpMsg2 = (PMSG_IDPASS_OLD*)lpMsg;
                    // Fix account id save for s9
                    DWORD OldProtect;
                    VirtualProtect((LPVOID)0x08B97990, 12, PAGE_EXECUTE_READWRITE, &OldProtect);

                    char AccountID[11];
                    AccountID[10] = 0;
                    memcpy(AccountID, lpMsg2->Id, 10);
                    BuxConvert(AccountID, 10);

                    memcpy((void*)0x08B97990, AccountID, 11);
#endif
                }
                break;
            }
            
        }
        break;
    }

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
        int padding;

        switch (buff[0])
        {
        case 0xC1:
            proto = buff[2];
            size = buff[1];
            enc = 0;
            break;
        case 0xC2:
            proto = buff[3];
            size = MAKEWORD(buff[2], buff[1]);
            enc = 0;
            break;
        case 0xC3:
            enc = 1;
            size = buff[1];
            padding = buff[size - 1];
            if ((size - 3) % 16 != 0)
            {
                printf("0xC3 Error on Encripted packet, data size:%d, raw Size:%d, padding:%d\n", size - 3, size, padding);
                return;
            }
            m_Decryption.ProcessData(&DecBuff[2], &buff[2], size - 3);
            //memcpy(&DecBuff[2], &buff[2], DecSize - 2);
            DecSize = size - padding;
            DecBuff[0] = 0xC1;
            DecBuff[1] = DecSize;
            size = DecSize;
            buff = DecBuff;
            proto = DecBuff[2];
            break;
        case 0xC4:
            enc = 1;
            size = buff[2] | (buff[1]<<8);

            if ((size - 4) % 16 != 0)
            {
                printf("Error on Encripted packet, data size:%d", size - 4);
                return;
            }
            m_Decryption.ProcessData(&DecBuff[3], &buff[3], size - 4);
            //memcpy(&DecBuff[3], &buff[3], DecSize - 3);
            DecSize = size;//-buff[size - 1];
            size = DecSize;
            DecBuff[0] = 0xC2;
            DecBuff[2] = LOBYTE(size);
            DecBuff[1] = HIBYTE(size);
            buff = DecBuff;
            proto = buff[3];
            break;
        }

        WORD* p = (WORD*)(buff + (buff[0] == 0xC1 ? 2 : 3));

        BYTE subco = (BYTE)(((*p) & 0xff00) >> 8);

        auto r = packetName.find(*p);
        auto r2 = packetName.find(*p | 0xFF00);

        if (r == packetName.end())
        {
            r = packetName.find(*p | 0xFF00);
        }
        if (r == packetName.end())
        {
            r = packetName.find(*p | 0xFF00);
        }

        if (r != packetName.end())
        {
            PacketPrint(buff, size, (*r).second, "Recv<");
        }
        else
        {
            PacketPrint(buff, size, "unknow", "Recv<");
        }

        if (unk1 == 1)
            ProtocolCoreA(unk2, proto, buff, size, enc);
        else
            ProtocolCoreB(proto, buff, size, enc);
    }
}

#ifdef CLIENT_S9
auto ConnectToCS = (T_ConnToCS)MU_CONNECT_FUNC;
char szVersion[] = "10525";
char szSerial[18] = "fughy683dfu7teqg";

void Connect()
{
    char szIp[128];
    int port;
    GetPrivateProfileStringA("MU", "URL", "127.0.0.1", szIp, 128, cfgFile);
    port = GetPrivateProfileIntA("MU", "Port", 44405, cfgFile);
    printf("Connecto to %s:%d\n", szIp, port);
    ConnectToCS(szIp, port);
}

void onCsHook2()
{
    Connect();

    _asm
    {
        mov ecx, CONNECT_HOOK2;
        sub ecx, onCsHook2;
        jmp ecx;
    }
}

void __declspec(naked) onCsHook()
{
    Connect();

    _asm
    {
        push eax;
        //call 0x00635371; // it's not needed, 635371 is original Connect function, we use muConnectToCS
        mov edx, 0x004DEA9A; // S9
        jmp edx;
    }
}

int __cdecl loc_sprintf(char* buffer, const char* format, ...)
{
    int ret = -1;

    if (IsBadReadPtr(buffer, 1) || IsBadReadPtr(format, 1))
    {
        printf("IsBadReadPtr sprintf(%p, %p, ...)\n", buffer, format);
        if (!IsBadReadPtr(buffer, 1))
            strcpy(buffer, "<Bad_Format>");
        return ret;
    }

    //printf("sprintf(%s, %s, ...)\n", buffer, format);
    va_list va;
    va_start(va, format);
    ret = vsprintf(buffer, format, va);
    va_end(va);

    return ret;
}

void ProcLoading()
{
    fp = fopen("ParsePacket.log", "a+");
    BYTE key[] = { 0x44, 0x9D, 0x0F, 0xD0, 0x37, 0x22, 0x8F, 0xCB, 0xED, 0x0D, 0x37, 0x04, 0xDE, 0x78, 0x00, 0xE4, 0x33, 0x86, 0x20, 0xC2, 0x79, 0x35, 0x92, 0x26, 0xD4, 0x37, 0x37, 0x30, 0x98, 0xEF, 0xA4, 0xDE };

    m_Encryption.SetKey(key, sizeof(key));
    m_Decryption.SetKey(key, sizeof(key));

    old_Send = main_Send.Set(old_Send, SendPacket);
    old_Recv = main_Recv.Set(old_Recv, ParsePacket);
    old_sprintf = main_sprintf.Set(old_sprintf, loc_sprintf);

    BYTE c[32];
    memset(c, 0x90, 32);

    Hook<void>::Jump((void*)CONNECT_HOOK1, onCsHook);
    Hook<void>::Write((void*)CONNECT_HOOK2, c, 6);
    Hook<void>::Jump((void*)CONNECT_HOOK2, onCsHook2);
    //Hook<void>::ChangeAddress(VERSION_HOOK1, (DWORD)szVersion);
    //Hook<void>::ChangeAddress(VERSION_HOOK2, (DWORD)szVersion);
    //Hook<void>::ChangeAddress(VERSION_HOOK3, (DWORD)szVersion);
    Hook<void>::ChangeAddress(SERIAL_HOOK1, (DWORD)szSerial);
    Hook<void>::ChangeAddress(SERIAL_HOOK2, (DWORD)szSerial);
    Hook<void>::Jump((void*)0x0063562F, (void*)0x0063564C);//remove filter.bmd
    //009C79AC   |> C9              LEAVE
    Hook<void>::Jump((void*)0x009C77C8, (void*)0x009C79AC);// HelpData_Eng.bmd Corrupt
    Hook<void>::WriteByte((void*)CTRL_FREEZE_FIX, 0x02);
    Hook<void>::Write((void*)MAPSRV_DELACCID_FIX, c, 5); // nop call to memcpy in mapserver, because buff source is empty, due to no account id from webzen http (remove new login system)
    Hook<void>::WriteByte((void*)(0x005CA914 + 2), 0x26);// Box of Kundun 2-5 fix
    Hook<void>::WriteByte((void*)(0x005CA8C7 + 3), 0x07);// Box of Heaven fix
    Hook<void>::WriteByte((void*)0x009E5EDC, 0xEB); // S9 -- Cherry Blossom Opening MuRuumy Windows Fix, JNZ -> JMP
    //MemSet(0x00802160, 0x90, 6); // S9 -- Joh option display on Ancients
    //Hook<void>::WriteByte((void*)0x00A31BE6, 0x30); // S9 -- Decreased limit of screen refresh frequency, 60 -> 48
    Hook<void>::WriteByte((void*)0x00A31BE7, 0x7D); // JE -> JGE -- limit check == -> >=

    GetPrivateProfileStringA("MU", "Serial", "fughy683dfu7teqg", szSerial, 18, cfgFile);
    GetPrivateProfileStringA("MU", "Version", "10525", szVersion, 5, cfgFile);

    for (int i = 0; i < 5; i++)
    {
        szVersion[i] = szVersion[i] + (i + 1);
    }
}
#endif
#ifdef CLIENT_S12
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
    //Hook<void>::Write((void*)0x00D42114, c, sizeof(c));

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
    Hook<void>::Write((void*)0x00C7B11C, c, 2);//1.18.70

    //?ItemtooltipBmd
    Hook<void>::WriteByte((void*)0x0085216E, 0xEB);//1.18.70
    //?itemsetoptiontext
    Hook<void>::WriteByte((void*)0x00529b6c, 0xEB);//1.18.70
    //masterskillTooltip
    Hook<void>::WriteByte((void*)0x00b02eb5, 0xEB);//1.18.70
    //SkillToolTipText
    BYTE SKILL_JMP[] = { 0xE9,0xAD,0x00,0x00,0x00,0x90 };
    Hook<void>::Write((void*)0x00CCA2F8, SKILL_JMP, sizeof(SKILL_JMP));//1.18.70

    GetPrivateProfileStringA("MU", "URL", "127.0.0.1", (char*)CLIENIP, 256, cfgFile);
    GetPrivateProfileStringA("MU", "Serial", "fughy683dfu7teqg", (char*)CLIENTSERIAL, 18, cfgFile);
    *((int*)CLIENTPORT) = GetPrivateProfileIntA("MU", "Port", 44405, cfgFile);

    kernel32_GetStartupInfoA.Release();
}
#endif
void _stdcall new_GetStartupInfoA(LPSTARTUPINFOA lpStartupinfo)
{
    ProcLoading();

    GetStartupInfoA(lpStartupinfo);
}

typedef BOOL(WINAPI* MINIDUMPWRITEDUMP)( // Callback ??? ??
    HANDLE hProcess,
    DWORD dwPid,
    HANDLE hFile,
    MINIDUMP_TYPE DumpType,
    CONST PMINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
    CONST PMINIDUMP_USER_STREAM_INFORMATION UserStreamParam,
    CONST PMINIDUMP_CALLBACK_INFORMATION CallbackParam);

LPTOP_LEVEL_EXCEPTION_FILTER PreviousExceptionFilter = NULL;

LONG WINAPI UnHandledExceptionFilter(struct _EXCEPTION_POINTERS* exceptionInfo)
{
    fflush(fp);
    HMODULE    DllHandle = NULL;

    // Windows 2000 ???? ?? DBGHELP? ???? ??? ??? ??.
    DllHandle = LoadLibraryA("DBGHELP.DLL");

    if (DllHandle)
    {
        MINIDUMPWRITEDUMP Dump = (MINIDUMPWRITEDUMP)GetProcAddress(DllHandle, "MiniDumpWriteDump");

        if (Dump)
        {
            char DumpPath[MAX_PATH] = { 0, };
            SYSTEMTIME SystemTime;

            GetLocalTime(&SystemTime);

            sprintf_s(DumpPath, MAX_PATH, "%d-%d-%d_%dh%dm%ds.dmp",
                SystemTime.wYear,
                SystemTime.wMonth,
                SystemTime.wDay,
                SystemTime.wHour,
                SystemTime.wMinute,
                SystemTime.wSecond);

            HANDLE FileHandle = CreateFileA(
                DumpPath,
                GENERIC_WRITE,
                FILE_SHARE_WRITE,
                NULL, CREATE_ALWAYS,
                FILE_ATTRIBUTE_NORMAL,
                NULL);

            if (FileHandle != INVALID_HANDLE_VALUE)
            {
                _MINIDUMP_EXCEPTION_INFORMATION MiniDumpExceptionInfo;

                MiniDumpExceptionInfo.ThreadId = GetCurrentThreadId();
                MiniDumpExceptionInfo.ExceptionPointers = exceptionInfo;
                MiniDumpExceptionInfo.ClientPointers = NULL;

                BOOL Success = Dump(
                    GetCurrentProcess(),
                    GetCurrentProcessId(),
                    FileHandle,
                    (_MINIDUMP_TYPE)(MiniDumpWithUnloadedModules +
                        MiniDumpWithHandleData +
                        MiniDumpWithFullMemory), //webzen...
                    &MiniDumpExceptionInfo,
                    NULL,
                    NULL);

                if (Success)
                {
                    CloseHandle(FileHandle);

                    return EXCEPTION_CONTINUE_SEARCH;// EXCEPTION_EXECUTE_HANDLER;
                }
            }

            CloseHandle(FileHandle);
        }
    }

    return EXCEPTION_CONTINUE_SEARCH;
}

bool UEF = false;
void _stdcall new_OutputDebugStringA(LPCSTR lpOutputString)
{
    printf("OutputDebugString: %s", lpOutputString);
    if (!UEF)
    {
        SetErrorMode(SEM_FAILCRITICALERRORS);
        PreviousExceptionFilter = SetUnhandledExceptionFilter(UnHandledExceptionFilter);
        UEF = true;
    }
    //old_OutputDebugStringA(lpOutputString);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
#ifdef  CLIENT_S9
        ProcLoading();
#endif //  CLIENT_S9
#ifdef CLIENT_S12
        old_GetStartupInfoA = kernel32_GetStartupInfoA.Set(_GetStartupInfoA, new_GetStartupInfoA);
#endif // CLIENT_S12
        old_OutputDebugStringA = kernel32_OutputDebugStringA.Set(_OutputDebugStringA, new_OutputDebugStringA);
        AllocConsole();
        freopen("CONOUT$", "w", stdout);
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