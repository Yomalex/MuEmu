#pragma once

struct PBMSG_HEAD	// Packet - Byte Type
{
public:
	void set(LPBYTE lpBuf, BYTE head, BYTE size)	// line : 18
	{
		lpBuf[0] = 0xC1;
		lpBuf[1] = size;
		lpBuf[2] = head;
	};	// line : 22

	void setE(LPBYTE lpBuf, BYTE head, BYTE size)	// line : 25
	{
		lpBuf[0] = 0xC3;
		lpBuf[1] = size;
		lpBuf[2] = head;
	};	// line : 29

	BYTE c;
	BYTE size;
	BYTE headcode;
};

#pragma pack(1)
struct PMSG_IDPASS_OLD
{
	PBMSG_HEAD h;
	BYTE subcode;	// 3
	char Id[10];	// 4
	char Pass[20];	// E
	DWORD TickCount;	// 18
	BYTE CliVersion[5];	// 1C
	BYTE CliSerial[16];	// 21
};
struct PMSG_AHKEY
{
	PBMSG_HEAD h;
	BYTE subcode;	// 3
	BYTE PreSharedKey[32];
};
struct PMSG_CONNECT_CLIENT_RECV
{
	PBMSG_HEAD header; // C1:F1:00
	BYTE SubHead;
	BYTE result;
	BYTE junk1;
	BYTE indexH;
	BYTE junk2[4];
	BYTE indexL;
	BYTE version[5];
	DWORD Key;
};
#pragma pack()