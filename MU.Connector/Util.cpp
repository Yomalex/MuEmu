#include "pch.h"
#include "Util.h"

void BuxConvert(char* buf, int size)
{
    static unsigned char bBuxCode[3] = { 0xFC, 0xCF, 0xAB };
    for (int n = 0; n < size; n++)
    {
        buf[n] ^= bBuxCode[n % 3];		// Nice trick from WebZen
    }
}

void Print(FILE * fp, const char* format, ...)
{
    va_list ap;
    va_start(ap, format);
    if (fp != NULL)
        vfprintf(fp, format, ap);
    vprintf(format, ap);
    va_end(ap);
}

void PacketPrint(FILE * fp, LPBYTE buff, DWORD size, const char* szDesc)
{
    static char itoab[20];
    static char DecBuff[1000];
    static char DecBuff2[1000];
    unsigned int j, a;

    ZeroMemory(DecBuff, sizeof(DecBuff));
    ZeroMemory(DecBuff2, sizeof(DecBuff));
    Print(fp, "%s PACKET [%d]={\n", szDesc, size);
    for (unsigned int i = 0; i < size; i++)
    {
        if ((i % 16) == 0 && i != 0)
        {
            Print(fp, "%s %s\n", DecBuff, DecBuff2);
            ZeroMemory(DecBuff, sizeof(DecBuff));
            ZeroMemory(DecBuff2, sizeof(DecBuff));
        }
        switch (buff[i])
        {
        case 0x0A:
        case 0x0D:
            itoab[0] = '·';
            itoab[1] = '\0';
            break;
        default:
            sprintf(itoab, "%c", buff[i]);
            break;
        }
        strcat(DecBuff2, itoab);
        sprintf(itoab, "%02X%s", buff[i], (i+1<size)?" ":"");
        strcat(DecBuff, itoab);
    }
    if((size%16))
        Print(fp, "%s %s\n", DecBuff, DecBuff2);

    Print(fp, "}\n");
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

int GetPrivateProfileIntHexA(LPCSTR lpAppName, LPCSTR lpKeyName, INT Default, LPCSTR lpFileName)
{
    static char Buffer[10];
    sprintf(Buffer, "%02X", Default);
    GetPrivateProfileStringA(lpAppName, lpKeyName, Buffer, Buffer, sizeof(Buffer), lpFileName);
    
    return std::stoi(Buffer, nullptr, 16);
}