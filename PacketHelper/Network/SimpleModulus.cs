using System;
using System.IO;

namespace WebZen.Network
{
    public static class SimpleModulus
    {
        private static readonly uint[] s_saveLoadXor = new uint[] { 0x3F08A79B, 0xE25CC287, 0x93D27AB9, 0x20DEA7BF };
        private static Modulus s_enc;
        private static Modulus s_dec;

        public static byte[] Encoder(byte[] src) => s_enc.Enc(src);
        public static byte[] Decoder(byte[] src) => s_dec.Dec(src);

        public static void LoadEncryptionKey(string file)
        {
            s_enc = LoadKey(file, 4370, true, true, false, true);
        }

        public static void LoadDecryptionKey(string file)
        {
            s_dec = LoadKey(file, 4370, true, false, true, true);
        }

        private static Modulus LoadKey(string file, ushort header, bool loadModulus, bool loadEncKey, bool loadDecKey, bool loadXOrKey)
        {
            var key = new Modulus();

            using (var f = File.OpenRead(file))
            {
                var Size = 6;
                Size += loadModulus ? 16 : 0;
                Size += loadEncKey ? 16 : 0;
                Size += loadDecKey ? 16 : 0;
                Size += loadXOrKey ? 16 : 0;

                var temp = new byte[16];

                if (loadModulus)
                {
                    f.Read(temp, 0, 16);
                    for (var i = 0; i < 4; i++)
                    {
                        key.modulus[i] = s_saveLoadXor[i] ^ BitConverter.ToUInt32(temp, i * 4);
                    }
                }

                if (loadEncKey)
                {
                    f.Read(temp, 0, 16);
                    for (var i = 0; i < 4; i++)
                    {
                        key.encryptionKey[i] = s_saveLoadXor[i] ^ BitConverter.ToUInt32(temp, i * 4);
                    }
                }

                if (loadDecKey)
                {
                    f.Read(temp, 0, 16);
                    for (var i = 0; i < 4; i++)
                    {
                        key.decryptionKey[i] = s_saveLoadXor[i] ^ BitConverter.ToUInt32(temp, i * 4);
                    }
                }

                if (loadXOrKey)
                {
                    f.Read(temp, 0, 16);
                    for (var i = 0; i < 4; i++)
                    {
                        key.xor[i] = s_saveLoadXor[i] ^ BitConverter.ToUInt32(temp, i * 4);
                    }
                }
            }

            return key;
        }

        private static void SaveKey(string file, Modulus key, ushort header, bool loadModulus, bool loadEncKey, bool loadDecKey, bool loadXOrKey)
        {
            using (var f = File.OpenWrite(file))
            {
                var Size = 6;
                Size += loadModulus ? 16 : 0;
                Size += loadEncKey ? 16 : 0;
                Size += loadDecKey ? 16 : 0;
                Size += loadXOrKey ? 16 : 0;

                var temp = 0u;

                if (loadModulus)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        temp = s_saveLoadXor[i] ^ key.modulus[i];
                    }
                    f.Write(BitConverter.GetBytes(temp), 0, 4);
                }

                if (loadEncKey)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        temp = s_saveLoadXor[i] ^ key.encryptionKey[i];
                    }
                    f.Write(BitConverter.GetBytes(temp), 0, 4);
                }

                if (loadDecKey)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        temp = s_saveLoadXor[i] ^ key.decryptionKey[i];
                    }
                    f.Write(BitConverter.GetBytes(temp), 0, 4);
                }

                if (loadXOrKey)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        temp = s_saveLoadXor[i] ^ key.xor[i];
                    }
                    f.Write(BitConverter.GetBytes(temp), 0, 4);
                }
            }
        }
    }

    public class Modulus
    {
        public uint[] xor = new uint[4];
        public uint[] encryptionKey = new uint[4];
        public uint[] decryptionKey = new uint[4];
        public uint[] modulus = new uint[4];

        public byte[] Enc(byte[] src)
        {
            var iDec = ((src.Length + 7) / 8);
            iDec = (iDec + iDec * 4) * 2 + iDec;

            var dest = new byte[iDec];

            var iOriSize = src.Length;
            var iDest = 0;
            var iTempSize2 = 0;

            var tempDest = new byte[11];
            var tempSrc = new byte[8];

            for (int i = 0; i < src.Length; i += 8, iOriSize -= 8, iDest += 11)
            {
                iTempSize2 = iOriSize;
                if (iOriSize >= 8)
                    iTempSize2 = 8;

                Array.Copy(src, i, tempSrc, 0, iTempSize2);

                EncBlock(tempDest, tempSrc, iTempSize2);

                Array.Copy(tempDest, 0, dest, iDest, 11);
            }

            return dest;
        }

        public byte[] Dec(byte[] src)
        {
            var iEnc = src.Length * 8 / 11;
            var dest = new byte[iEnc];

            var decLen = 0;
            var destPos = 0;
            var iResult = 0;
            var tempDest = new byte[8];
            var tempSrc = new byte[11];

            while (decLen < src.Length)
            {
                Array.Copy(src, decLen, tempSrc, 0, 11);

                var tempResult = DecBlock(tempDest, tempSrc);

                Array.Copy(tempDest, 0, dest, destPos, 8);

                if (iResult < 0 || tempResult < 0)
                    throw new InvalidDataException("Error decoding buffer");

                iResult += tempResult;
                decLen += 11;
                destPos += 8;
            }

            return dest;
        }

        private int EncBlock(byte[] dest, byte[] src, int size)
        {
            var EncBuffer = new uint[4];
            var EncValue = 0u;

            for (var i = 0; i < 4; i++)
            {
                var EncSource = BitConverter.ToUInt16(src, i * 2);
                EncBuffer[i] = (xor[i] ^ EncSource ^ EncValue) * encryptionKey[i] % modulus[i];
                EncValue = EncBuffer[i] & 0xffff;
            }

            for (var i = 0; i < 3; i++)
            {
                EncBuffer[i] = EncBuffer[i] ^ xor[i] ^ (EncBuffer[i + 1] & 0xFFFF);
            }

            var iBitPos = 0;

            for (int i = 0; i < 4; i++)
            {
                var EncBuffBytes = BitConverter.GetBytes(EncBuffer[i]);
                iBitPos = AddBits(dest, iBitPos, EncBuffBytes, 0, 16);
                iBitPos = AddBits(dest, iBitPos, EncBuffBytes, 22, 2);
            }

            var btCheckSum = (byte)0xF8;

            for (int i = 0; i < 8; i++)
                btCheckSum ^= src[i];

            var cs = new byte[] { (byte)(btCheckSum ^ size ^ 0x3D), btCheckSum };

            return AddBits(dest, iBitPos, cs, 0, 16);
        }

        private int DecBlock(byte[] dest, byte[] src)
        {
            var tempDest = new byte[4];
            var decBuffer = new uint[4];
            var bitPos = 0;

            for (var i = 0; i < 4; i++)
            {
                AddBits(tempDest, 0, src, bitPos, 16);
                bitPos += 16;
                AddBits(tempDest, 22, src, bitPos, 2);
                bitPos += 2;
                decBuffer[i] = BitConverter.ToUInt32(tempDest, 0);

                //Array.Fill<byte>(tempDest, 0);
                for (var j = 0; j < tempDest.Length; j++)
                    tempDest[j] = 0;
            }

            for (var i = 2; i >= 0; i--)
            {
                decBuffer[i] = (decBuffer[i] ^ xor[i] ^ (decBuffer[i + 1] & 0xFFFF));
            }

            ushort Temp = 0;
            var Temp1 = 0u;

            for (var i = 0; i < 4; i++)
            {
                Temp1 = ((decryptionKey[i] * decBuffer[i]) % modulus[i]) ^ xor[i] ^ Temp;
                Temp = (ushort)(decBuffer[i] & 0xFFFF);
                Array.Copy(BitConverter.GetBytes(Temp1), 0, dest, i * 2, 2);
            }


            //Array.Fill<byte>(tempDest, 0);
            for (var j = 0; j < tempDest.Length; j++)
                tempDest[j] = 0;
            AddBits(tempDest, 0, src, bitPos, 16);

            tempDest[0] = (byte)(tempDest[1] ^ tempDest[0] ^ 0x3D);

            var CheckSum = (byte)0xF8;
            for (var i = 0; i < 8; i++)
                CheckSum ^= dest[i];

            if (CheckSum != tempDest[1])
                return -1;

            return tempDest[0];
        }

        private int AddBits(byte[] dest, int destPos, byte[] src, int srcPos, int srcLen)
        {
            var iSourceBufferBitLen = srcLen + srcPos;
            var iTempBufferLen = GetByteOfBit(iSourceBufferBitLen -1);
            iTempBufferLen += 1 - GetByteOfBit(srcPos);

            // Copy the Source Buffer
            var pTempBuffer = new byte[iTempBufferLen + 1];
            //Array.Fill<byte>(pTempBuffer, 0);
            Array.Copy(src, GetByteOfBit(srcPos), pTempBuffer, 0, iTempBufferLen);

            // Save the Last ibt if exist
            if ((iSourceBufferBitLen % 8) != 0)
            {
                pTempBuffer[iTempBufferLen - 1] &= (byte)(255 << (8 - (iSourceBufferBitLen % 8)));
            }

            // Get the Values to Shift
            var iShiftLeft = (srcPos % 8);
            var iShiftRight = (destPos % 8);

            // Shift the Values to Add the right space of the desired bits
            Shift(pTempBuffer, iTempBufferLen, -iShiftLeft);
            Shift(pTempBuffer, iTempBufferLen + 1, iShiftRight);

            // Copy the the bits of Source to the Dest
            int iNewTempBufferLen = ((iShiftRight <= iShiftLeft) ? 0 : 1) + iTempBufferLen;

            var iDest = GetByteOfBit(destPos);
            for (int i = 0; i < iNewTempBufferLen; i++)
            {
                dest[iDest + i] |= pTempBuffer[i];
            }

            // Return the number of bits of the new Dest Buffer
            return destPos + srcLen;
        }

        private void Shift(byte[] TempBuff, int iSize, int ShiftLen)
        {
            // Case no Shift Len
            if (ShiftLen != 0)
            {
                // Shift Right
                if (ShiftLen > 0)
                {
                    if ((iSize - 1) > 0)
                    {
                        for (int i = (iSize - 1); i > 0; i--)
                        {
                            TempBuff[i] = (byte)((TempBuff[i - 1] << ((8 - ShiftLen))) | (TempBuff[i] >> ShiftLen));
                        }
                    }

                    TempBuff[0] >>= ShiftLen;
                }
                else    // Shift Left
                {
                    ShiftLen = -ShiftLen;

                    if ((iSize - 1) > 0)
                    {
                        for (int i = 0; i < (iSize - 1); i++)
                        {
                            TempBuff[i] = (byte)((TempBuff[i + 1] >> ((8 - ShiftLen))) | (TempBuff[i] << ShiftLen));
                        }
                    }

                    TempBuff[iSize - 1] <<= ShiftLen;
                }
            }
        }

        private int GetByteOfBit(int bit)
        {
            return bit >> 3;
        }
    }
}
