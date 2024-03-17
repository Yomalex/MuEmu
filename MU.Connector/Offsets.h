#pragma once

//#define CLIENT_S9 //1.05.25
#define CLIENT_S12 //1.18.70

typedef BYTE* (__thiscall* T_WZParse)(void* thisPtr);
typedef void(__thiscall* T_WZSender)(DWORD* thisPtr, BYTE* buff, DWORD len);
typedef void(_stdcall* T_GetStartupInfoA)(LPSTARTUPINFOA lpStartupinfo);
typedef void(_stdcall* T_OutputDebugStringA)(LPCSTR lpOutputString);
typedef void(*T_WZSend)(LPBYTE, DWORD, int, int);
typedef void(*T_WZSendS16)(LPBYTE, DWORD);
typedef void(*T_WZRecv)(LPBYTE, int, int);
typedef void(*T_sprintf)(char*, const char*, ...);
typedef void(*T_ProcCoreA)(int, int, BYTE*, int, int);
typedef BOOL(*T_ProcCoreB)(DWORD, BYTE*, DWORD, DWORD);
typedef void (*T_ConnToCS)(char* hostname, int port);

#ifdef CLIENT_S9
#define CLIENT_VERSION "10525"

#define CTRL_FREEZE_FIX (0x0053125F+1) // S9
#define MAPSRV_DELACCID_FIX 0x004FAC1D // S9

#define DISCONNECT_FUNC 0x004E1B9E

#define MU_CONNECT_FUNC 0x0063CEDA // S9
#define CONNECT_HOOK1 0x004DEA94 // S9
#define CONNECT_HOOK2 0x0043EA60 // S9
#define GSCONNECT_HOOK1 0x09E568EF // S9
#define GSCONNECT_HOOK2 0x004FAB84 // S9
#define CHATSERVER_PORT 0x004BAC22+1 // S9

#define SEND_PACKET_HOOK 0x004393AE // S9
#define MU_SEND_PACKET 0x0043976D // S9
#define MU_SENDER_CLASS 0x120703C // S9
#define PARSE_PACKET_HOOK 0x00669EC7 // S9
#define PARSE_PACKET_STREAM 0x0063B42D // S9
#define PROTOCOL_CORE1 0x0067430E // S9
#define PROTOCOL_CORE2 0x0067084A // S9

#define VERSION_HOOK1 0x006E86A8+3 // S9
#define VERSION_HOOK2 0x004FAD4F+3 // S9
#define VERSION_HOOK3 0x09ED4BAF+3 // S9
#define SERIAL_HOOK1 0x006E86EB+3 // S9
#define SERIAL_HOOK2 0x004FAD92+3 // S9
#endif

/// MU Online Season 12 V1.18.70 Offsets
#ifdef CLIENT_S12
#define CLIENT_VERSION "11870"

#define SEND_PACKET_HOOK 0x00BAEBDD // S12
#define MU_SEND_PACKET 0x00BAF008 // S12
#define MU_SENDER_CLASS 0x159F3E4 // S12
#define PARSE_PACKET_HOOK 0x00C19CF5
#define PARSE_PACKET_STREAM 0xBAFACC // S12
#define PROTOCOL_CORE1 0xBE50E1 // S12
#define PROTOCOL_CORE2 0xC150A9 // S12

#define CLIENIP 0x01596520
#define CLIENTPORT 0x01595A54
#define CLIENTVERSION 0x0159F3C8
#define CLIENTSERIAL CLIENTVERSION+8
#define SERIALOUT 0xA17F99C
#endif