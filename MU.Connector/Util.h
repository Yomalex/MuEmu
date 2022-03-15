#pragma once

void BuxConvert(char* buf, int size);
void Print(FILE* fp, const char* format, ...);
void PacketPrint(FILE* fp, LPBYTE buff, DWORD size, const char* szDesc);
int __cdecl loc_sprintf(char* buffer, const char* format, ...);
int GetPrivateProfileIntHexA(LPCSTR lpAppName, LPCSTR lpKeyName, INT Default, LPCSTR lpFileName);