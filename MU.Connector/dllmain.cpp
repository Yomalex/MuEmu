// dllmain.cpp : Define el punto de entrada de la aplicación DLL.
#include "pch.h"
#include "../../CryptoPP/cryptlib.h"
#include "../../CryptoPP/modes.h"
#include "../../CryptoPP/aes.h"
#include "Offsets.h"
#include "Protocol.h"
#include "Util.h"
#include "MiniDump.h"
#pragma comment(lib,"../../CryptoPP/Win32/Output/Release/cryptlib.lib")

LPBYTE _GetStartupInfoA = (LPBYTE)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "GetStartupInfoA");
LPBYTE _OutputDebugStringA = (LPBYTE)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "OutputDebugStringA");

FILE* fp=NULL;
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
        if (oldFunc == nullptr)
            return (T)(copy + 1);

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

    static void ChangeFunctionAddress(void* offset, void* target)
    {
        if (offset == nullptr)
            return;

        int size = 5;
        VirtualProtect(offset, size, PAGE_EXECUTE_READWRITE, &VPOld);
        BYTE * newJump = (BYTE*)offset;
        *((DWORD*)&newJump[1]) = ((DWORD)target) - ((DWORD)offset) - 5;
        VirtualProtect(offset, size, VPOld, &VPOld);
        Print(fp, "ChangeFunctionAddress from %p to %p\n", offset, target);
    }

    static void Jump(void* offset, void* target)
    {
        if (offset == nullptr)
            return;

        BYTE newJump[5] = { 0xE9, 0, 0, 0, 0 };
        VirtualProtect(offset, sizeof(newJump), PAGE_EXECUTE_READWRITE, &VPOld);
        *((DWORD*)&newJump[1]) = ((DWORD)target) - ((DWORD)offset) - 5;
        memcpy(offset, newJump, sizeof(newJump));
        VirtualProtect(offset, sizeof(newJump), VPOld, &VPOld);
        Print(fp, "Jump from %p to %p\n", offset, target);
    }

    static void Call(void* offset, void* target)
    {
        if (offset == nullptr)
            return;

        BYTE newJump[5] = { 0xE8, 0, 0, 0, 0 };
        VirtualProtect(offset, sizeof(newJump), PAGE_EXECUTE_READWRITE, &VPOld);
        *((DWORD*)&newJump[1]) = ((DWORD)target) - ((DWORD)offset) - 5;
        memcpy(offset, newJump, sizeof(newJump));
        VirtualProtect(offset, sizeof(newJump), VPOld, &VPOld);
        Print(fp, "Call from %p to %p\n", offset, target);
    }

    static void Write(void* offset, BYTE array[], int size)
    {
        if (offset == nullptr)
            return;

        VirtualProtect(offset, size, PAGE_EXECUTE_READWRITE, &VPOld);
        memcpy(offset, array, size);
        VirtualProtect(offset, size, VPOld, &VPOld);
        char buff[1024];
        char buff2[1024];
        for (auto n =0; n < size; n++)
        {
            sprintf(buff + n, "%c", array[n]);
            sprintf(buff2 + n*2, "%02X", array[n]);
        }
        Print(fp, "Write to %p, %s %s\n", offset, buff2, buff);
    }

    static void WriteByte(void* offset, BYTE bt)
    {
        if (offset == nullptr)
            return;

        VirtualProtect(offset, 1, PAGE_EXECUTE_READWRITE, &VPOld);
        *((LPBYTE)offset) = bt;
        VirtualProtect(offset, 1, VPOld, &VPOld);
        Print(fp, "Write to %p, %02X\n", offset, bt);
    }

    static void ChangeAddress(DWORD Addr, DWORD AddrNew)
    {
        if (Addr == 0)
            return;

        DWORD OldProtect;
        VirtualProtect((LPVOID)Addr, 4, PAGE_EXECUTE_READWRITE, &OldProtect);
        __asm
        {
            MOV EAX, Addr;
            MOV EDX, AddrNew;
            MOV DWORD PTR DS : [EAX] , EDX;
        }
        VirtualProtect((LPVOID)Addr, 4, OldProtect, &OldProtect);
        Print(fp, "ChangeAddress on %08X to %08X\n", Addr, AddrNew);
    }
};
template<typename T>
DWORD Hook<T>::VPOld = 0;

// Hooks
Hook<T_GetStartupInfoA> kernel32_GetStartupInfoA;
Hook<T_OutputDebugStringA> kernel32_OutputDebugStringA;
Hook<T_WZSend> main_Send;
Hook<T_WZSendS16> main_SendS16;
Hook<T_WZSendS16> main_SendS161;
Hook<T_WZSendS16> main_SendS162;
Hook<T_WZRecv> main_Recv;
Hook<T_sprintf> main_sprintf;
Hook<T_ProcCoreA> main_procCoreA;
Hook<T_ProcCoreB> main_procCoreB;
T_GetStartupInfoA old_GetStartupInfoA;
T_OutputDebugStringA old_OutputDebugStringA;

// Crypto
CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Encryption* m_Encryption;
CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Decryption* m_Decryption;

// Functions
auto old_Send = (T_WZSend)SEND_PACKET_HOOK;// 0x00BAEBDD;
auto old_SendS16 = (T_WZSendS16)SEND_PACKET_HOOK;// 0x00BAEBDD;
auto old_SendS161 = (T_WZSendS16)nullptr;// 0x00BAEBDD;
auto old_SendS162 = (T_WZSendS16)nullptr;// 0x00BAEBDD;
auto old_Recv = (T_WZRecv)PARSE_PACKET_HOOK;// 0x00C19CF5;
auto ProtocolCoreA = (T_ProcCoreA)PROTOCOL_CORE1;// 0xBE50E1;
auto ProtocolCoreB = (T_ProcCoreB)PROTOCOL_CORE2;// 0xC150A9;
auto old_sprintf = (T_sprintf)0x00D0A18A;
auto WZSender = (T_WZSender)MU_SEND_PACKET;
auto WZParser = (T_WZParse)PARSE_PACKET_STREAM;

// Variables
auto WZSenderClass = (DWORD**)MU_SENDER_CLASS;
char szIp[50];
char szVersion[] = CLIENT_VERSION;
char szSerial[18] = "fughy683dfu7teqg";
int iPort = 44405;
LPTOP_LEVEL_EXCEPTION_FILTER PreviousExceptionFilter = NULL;
bool UEF = false;
bool Initialized = false;

// Constants
const char* cfgFile = ".\\config.ini";
std::map<std::string, void*> cmd = {
    {"URL", szIp},
    {"Version", szVersion},
    {"Serial", szSerial},
    {"Port", &iPort}
};
std::map<std::string, int> cmdl = {
    {"URL", sizeof(szIp)},
    {"Version", sizeof(szVersion)},
    {"Serial", sizeof(szSerial)},
    {"Port", sizeof(iPort)}
};
BYTE key[] = { 0x44, 0x9D, 0x0F, 0xD0, 0x37, 0x22, 0x8F, 0xCB, 0xED, 0x0D, 0x37, 0x04, 0xDE, 0x78, 0x00, 0xE4, 0x33, 0x86, 0x20, 0xC2, 0x79, 0x35, 0x92, 0x26, 0xD4, 0x37, 0x37, 0x30, 0x98, 0xEF, 0xA4, 0xDE };
BYTE xORKey[] = { 0xAB, 0x11, 0xCD, 0xFE, 0x18, 0x23, 0xC5, 0xA3, 0xCA, 0x33, 0xC1, 0xCC, 0x66,
                0x67, 0x21, 0xF3, 0x32, 0x12, 0x15, 0x35, 0x29, 0xFF, 0xFE, 0x1D, 0x44, 0xEF,
                0xCD, 0x41, 0x26, 0x3C, 0x4E, 0x4D };

void XOrData(bool Encode, BYTE * lpBuffer, int size, int offset)
{
    if (Encode)
    {
        for (auto n = offset; n < size-1; n++)
        {
            lpBuffer[n + 1] ^= lpBuffer[n] ^ xORKey[n % 32];
        }
    }
    else
    {
        for (auto n = size-1; n > offset; n--)
        {
            lpBuffer[n] ^= lpBuffer[n-1] ^ xORKey[n % 32];
        }
    }
}

void SendPacket(BYTE* lpMsg, DWORD len, int enc, int unk1)
{
    static BYTE send[8192];
    auto buff = send;
    DWORD size;

    BYTE type = lpMsg[0];

    auto Offset = (type == 0xC1 ? 2 : 3);

    memcpy(send, lpMsg, len);
    XOrData(false, send, len, Offset);
    PacketPrint(fp, send, len, enc ? "SendEnc>" : "Send>");

    WORD* p = (WORD*)(send + Offset);
    BYTE proto = (*p)&0xff;
    BYTE subco = ((*p)&0xff00)>>8;
    
    switch (proto)
    {
        case 0xF1:
        {
            switch (subco)
            {
                case 0x01:
                {
#ifdef CLIENT_S91
                    PMSG_IDPASS_OLD* lpMsg2 = (PMSG_IDPASS_OLD*)send;
                    // Fix account id save for s9
                    DWORD OldProtect;
                    VirtualProtect((LPVOID)0x08B97990, 12, PAGE_EXECUTE_READWRITE, &OldProtect);

                    char AccountID[11];
                    AccountID[10] = 0;
                    memcpy(AccountID, lpMsg2->Id, 10);
                    BuxConvert(AccountID, 10);

                    memcpy((void*)0x08B97990, AccountID, 11);
                    Print(fp, "Account Info:\nAccount:%s\nVersion:%s\nSerial:%s", AccountID, lpMsg2->CliVersion, lpMsg2->CliSerial);
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
        m_Encryption->ProcessData(send + offset, lpMsg+offset, newSize);
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

        PacketPrint(fp, send, size, "m_Encryption");
    }
    else
    {
        buff = lpMsg;
        size = len;
    }

    fflush(fp);

    DWORD* d = *WZSenderClass;
    WZSender(d, buff, size);
}

void SendPacketS16(BYTE* lpMsg, DWORD len)
{
    SendPacket(lpMsg, len, 0, 0);
}

void ProtocolCoreAEx(int unk, int head, BYTE* buff, int size, int enc)
{
}

BYTE NewEncPacket[8192];
BOOL ProtocolCoreBDll(BYTE head, BYTE* buff, DWORD size, int enc)
{
    auto subcode = buff[0] == 0xC1 ? buff[3] : buff[4];
    switch (head)
    {
    case 0xFA:
        switch (subcode)
        {
            case 0x00:
            {
                PMSG_AHKEY* pShared = (PMSG_AHKEY*)buff;
                m_Encryption->SetKey(pShared->PreSharedKey, sizeof(pShared->PreSharedKey));
                m_Decryption->SetKey(pShared->PreSharedKey, sizeof(pShared->PreSharedKey));
                Print(fp, "New PSKey recived\n");
                PacketPrint(fp, pShared->PreSharedKey, sizeof(pShared->PreSharedKey), "PSK");
            }
            return 1;
        }
        break;
    }
    return ProtocolCoreB(head, buff, size, enc);
}

BOOL ProtocolCoreBEx(BYTE head, BYTE* buff, DWORD size, int enc)
{
    static BYTE DecBuff[7024];
    unsigned int DecSize;

    auto Type = buff[0];
    auto Offset = (Type & 1) == 1 ? 2 : 3;
    WORD Size = (Type & 1) == 1 ? buff[1] : MAKEWORD(buff[2], buff[1]);
    auto OPCode = buff[Offset];
    auto Encode = Type >= 0xC3 ? 1 : 0;
    auto Padding = buff[Size - 1];

    if (Encode)
    {
        auto DataSize = Size - Offset - 1;
        if (DataSize % 16)
        {
            Print(fp, "0xC3 Error on Encripted packet, data size:%d, raw Size:%d, padding:%d\n", DataSize, Size, Padding);
            return 0;
        }

        m_Decryption->ProcessData(&DecBuff[Offset], &buff[Offset], DataSize);
        DecSize = Size - Padding;
        DecBuff[0] = Type - 2;
        if ((Type & 1) == 1)
        {
            DecBuff[1] = DecSize;
        }
        else
        {
            DecBuff[1] = DecSize & 0xFF;
            DecBuff[2] = (DecSize & 0xFF00) >> 8;
        }
        Size = DecSize;
        buff = DecBuff;
        OPCode = DecBuff[Offset];
    }

    PacketPrint(fp, buff, Size, Encode ? "RecvDec" : "Recv");
    Print(fp, "%p %02X\n", buff, OPCode);
    return ProtocolCoreBDll(OPCode, buff, size, 1);
}

void ParsePacket(void* PackStream, int unk1, int unk2)
{
    static BYTE DecBuff[7024];
    BYTE* buff;
    unsigned int DecSize;

    while (true)
    {
        buff = WZParser(PackStream);
        if (!buff)
            break;

        auto Type = buff[0];
        auto Offset = (Type & 1) == 1 ? 2 : 3;
        WORD Size = (Type & 1) == 1 ? buff[1] : MAKEWORD(buff[2], buff[1]);
        auto OPCode = buff[Offset];
        auto Encode = Type >= 0xC3 ? 1 : 0;
        auto Padding = buff[Size - 1];

        if (Encode)
        {
            auto DataSize = Size - Offset - 1;
            if (DataSize % 16)
            {
                Print(fp, "0xC3 Error on Encripted packet, data size:%d, raw Size:%d, padding:%d\n", DataSize, Size, Padding);
                return;
            }

            m_Decryption->ProcessData(&DecBuff[Offset], &buff[Offset], DataSize);
            DecSize = Size - Padding;
            DecBuff[0] = Type-2;
            if ((Type & 1) == 1)
            {
                DecBuff[1] = DecSize;
            }
            else
            {
                DecBuff[1] = DecSize & 0xFF;
                DecBuff[2] = (DecSize&0xFF00)>>8;
            }
            Size = DecSize;
            buff = DecBuff;
            OPCode = DecBuff[Offset];
        }

        PacketPrint(fp, buff, Size, Encode ? "RecvDec" : "Recv");
        Print(fp, "%02X unk1:%d unk2:%d\n", OPCode, unk1, unk2);

        if (unk1 == 1)
            ProtocolCoreA(unk2, OPCode, buff, Size, Encode);
        else
            ProtocolCoreBDll(OPCode, buff, Size, Encode);
    }
}

WSABUF StrToByteArray(std::string s)
{
    WSABUF result;
    if (s.find("<") != std::string::npos)
    {
        auto pos = s.find(">");
        auto sub = s.substr(1, pos-1);
        auto source = cmd[sub];
        result.len = cmdl[sub];
        result.buf = (char*)new BYTE[result.len];
        memcpy(result.buf, source, result.len);
        return result;
    }

    result.len = s.length()/2;
    result.buf = (char*)new BYTE[result.len];

    for (auto i = 0; i < result.len; i++)
    {
        result.buf[i] = (BYTE)std::stoi(s.substr(i * 2, 2), nullptr, 16);
    }

    return result;
}

std::map<void*,WSABUF> ParserINI(LPCSTR lpAppName)
{
    static char buffer[1024];
    auto length = GetPrivateProfileSectionA(lpAppName, buffer, sizeof(buffer), cfgFile);
    std::string delimiter = "=";
    std::map<void*, WSABUF> result;

    Print(fp, "ParserINI(%s:%d)=%s\n", lpAppName, length, buffer);

    for (auto i = 0; i < length;)
    {
        auto pBuf = buffer + i;
        std::string s = pBuf;
        auto pos = s.find(delimiter);
        std::string key = s.substr(0, pos);
        std::string value = s.substr(pos + delimiter.length());
        auto ikey = (void*)std::stoi(key, nullptr, 16);
        result[ikey] = StrToByteArray(value);
        i += strlen(pBuf) + 1;
    }

    return result;
}

int CheckIntegrityS16Kor()
{
    Print(fp, "CheckIntegrity:True\n");
    return 1;
}

__declspec(naked) void FixLogin()
{
#define MAIN_FIX_LOGIN_HOOK								0x00D3865B
#define MAIN_FIX_LOGIN_JMP								0x00D38728
#define MAIN_FIX_LOGIN_VALUE							0x2010
    
    static const DWORD JMP_1 = MAIN_FIX_LOGIN_JMP;

    _asm
    {
        Lea Eax, NewEncPacket
        Mov Dword Ptr Ss : [Ebp - MAIN_FIX_LOGIN_VALUE] , Eax
    }

    _asm
    {
        Jmp[JMP_1]
    }
}

void MainConfiguration()
{
    if (Initialized)
        return;

    m_Encryption = new CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Encryption();
    m_Decryption = new CryptoPP::ECB_Mode<CryptoPP::Rijndael>::Decryption();

    m_Encryption->SetKey(key, sizeof(key));
    m_Decryption->SetKey(key, sizeof(key));

    GetPrivateProfileStringA("MU", "URL", "127.0.0.1", szIp, sizeof(szIp), cfgFile);
    iPort = GetPrivateProfileIntA("MU", "Port", 44405, cfgFile);
    GetPrivateProfileStringA("MU", "Serial", "fughy683dfu7teqg", szSerial, 18, cfgFile);
    GetPrivateProfileStringA("MU", "Version", CLIENT_VERSION, szVersion, sizeof(szVersion), cfgFile);
    if (GetPrivateProfileIntA("MU", "Console", 0, cfgFile) == 1)
    {
        AllocConsole();
        freopen("CONOUT$", "w", stdout);
    }
    if (GetPrivateProfileIntA("MU", "Log", 0, cfgFile) == 1)
    {
        fp = fopen("ParsePacket.log", "w");
    }
    for (int i = 0; i < 5; i++)
        szVersion[i] = szVersion[i] + (i + 1);

    // Offset
    old_Send = (T_WZSend)GetPrivateProfileIntHexA("OFFSET", "Send", SEND_PACKET_HOOK, cfgFile);
    old_SendS16 = (T_WZSendS16)GetPrivateProfileIntHexA("OFFSET", "SendS16", 0, cfgFile);
    old_SendS161 = (T_WZSendS16)GetPrivateProfileIntHexA("OFFSET", "SendS161", 0, cfgFile);
    old_SendS162 = (T_WZSendS16)GetPrivateProfileIntHexA("OFFSET", "SendS162", 0, cfgFile);
    old_Recv = (T_WZRecv)GetPrivateProfileIntHexA("OFFSET", "Recv", PARSE_PACKET_HOOK, cfgFile);
    ProtocolCoreA = (T_ProcCoreA)GetPrivateProfileIntHexA("OFFSET", "CoreA", PROTOCOL_CORE1, cfgFile);
    ProtocolCoreB = (T_ProcCoreB)GetPrivateProfileIntHexA("OFFSET", "CoreB", PROTOCOL_CORE2, cfgFile);
    WZSender = (T_WZSender)GetPrivateProfileIntHexA("OFFSET", "SendPacket", MU_SEND_PACKET, cfgFile);
    WZParser = (T_WZParse)GetPrivateProfileIntHexA("OFFSET", "ParsePacket", PARSE_PACKET_STREAM, cfgFile);
    WZSenderClass = (DWORD**)GetPrivateProfileIntHexA("OFFSET", "SenderClass", MU_SENDER_CLASS, cfgFile);
    auto pCoreAHook = (T_ProcCoreA)GetPrivateProfileIntHexA("OFFSET", "ProcCoreAHook", 0, cfgFile);
    auto pCoreBHook = (T_ProcCoreB)GetPrivateProfileIntHexA("OFFSET", "ProcCoreBHook", 0, cfgFile);

    auto sprintfFix = GetPrivateProfileIntA("MU", "sprintfFix", 0, cfgFile) == 1;

    // Hooks
    old_Send = main_Send.Set(old_Send, SendPacket);
    main_SendS16.ChangeFunctionAddress(old_SendS16, SendPacketS16);
    main_SendS161.ChangeFunctionAddress(old_SendS161, SendPacketS16);
    main_SendS162.ChangeFunctionAddress(old_SendS162, SendPacketS16);
    old_Recv = main_Recv.Set(old_Recv, ParsePacket);
    main_procCoreA.Set(pCoreAHook, ProtocolCoreAEx);
    main_procCoreB.ChangeFunctionAddress(pCoreBHook, ProtocolCoreBEx);

    if(sprintfFix)
        old_sprintf = main_sprintf.Set(old_sprintf, loc_sprintf);

    char buff[100];
        
    //Jumps
    BYTE data[4];
    for (auto i : ParserINI("JUMPS"))
    {
        for (auto n = 0; n < 4; n++)
            data[3 - n] = i.second.buf[n];

        Hook<void>::Jump(i.first, (VOID*)(*(DWORD*)data));
        delete[] i.second.buf;
    }

    //CheckIntegrity
    for (auto i : ParserINI("CheckIntegrity"))
    {
        Hook<void>::ChangeFunctionAddress(i.first, CheckIntegrityS16Kor);
        delete[] i.second.buf;
    }

    //Call
    for (auto i : ParserINI("Call"))
    {
        for (auto n = 0; n < 4; n++)
            data[3 - n] = i.second.buf[n];

        Hook<void>::Call(i.first, (VOID*)(*(DWORD*)data));
        delete[] i.second.buf;
    }

    //WriteByte
    for (auto &i : ParserINI("WriteByte"))
    {
        Hook<void>::WriteByte(i.first, *i.second.buf);
        delete[] i.second.buf;
    }

    //Dump
    for (auto& i : ParserINI("Dump"))
    {
        //Hook<void>::WriteByte(i.first, *i.second.buf);
        PacketPrint(fp, (BYTE*)i.first, *i.second.buf, "Dump");
        delete[] i.second.buf;
    }

    //Write
    for (auto &i : ParserINI("Write"))
    {
        Hook<void>::Write(i.first, (BYTE*)i.second.buf, i.second.len);
        delete[] i.second.buf;
    }
    //ChangeAddress
    for (auto &i : ParserINI("ChangeAddress"))
    {
        Hook<void>::ChangeAddress((DWORD)i.first, (DWORD)i.second.buf);
        //delete[] i.second.buf;
    }

    Initialized = true;
}

void _stdcall new_GetStartupInfoA(LPSTARTUPINFOA lpStartupinfo)
{
    MainConfiguration();
    kernel32_GetStartupInfoA.Release();
    GetStartupInfoA(lpStartupinfo);
}

void _stdcall new_OutputDebugStringA(LPCSTR lpOutputString)
{
    Print(fp, "OutputDebugString: %s", lpOutputString);
    if (!UEF)
    {
        SetErrorMode(SEM_FAILCRITICALERRORS);
        PreviousExceptionFilter = SetUnhandledExceptionFilter(UnHandledExceptionFilter);
        UEF = true;
    }
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
        old_OutputDebugStringA = kernel32_OutputDebugStringA.Set(_OutputDebugStringA, new_OutputDebugStringA);
        break;
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        fclose(fp);
        delete m_Encryption;
        delete m_Decryption;
        break;
    }
    return TRUE;
}

extern "C" _declspec(dllexport) void Init()
{
}
extern "C" __declspec(dllexport) void Entry() {}