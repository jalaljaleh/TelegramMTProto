﻿using System;
using System.Numerics;
using System.Security.Cryptography;

namespace TelegramMTProto
{
    internal class TelegramMTProtoPacket : IDisposable
    {
        public TelegramMTProtoPacket()
        {
            _encryptCountBuf = new byte[16];
            _decryptCountBuf = new byte[16];
            _encryptNum = 0;
            _decryptNum = 0;
        }  
        private ProtocolType _protocolType;
        private ICryptoTransform _cryptoTransformEncrypt;
        private ICryptoTransform _cryptoTransformDecrypt;
        public ProtocolType ProtocolType { get => _protocolType; private set { } }

        private uint _encryptNum;
        private uint _decryptNum;
        private byte[] _encryptKey;
        private byte[] _encryptIv;
        private byte[] _decryptKey;
        private byte[] _decryptIv;
        private byte[] _encryptCountBuf;
        private byte[] _decryptCountBuf;
        public void SetInitBufferObfuscated2(in byte[] randomBuffer, in string secret)
        {
            var reversed = randomBuffer.SubArray(8, 48);
            Array.Reverse(reversed);
            var key = randomBuffer.SubArray(8, 32);
            var keyReversed = reversed.SubArray(0, 32);
            var binSecret = secret.HexToByteArray();
            key = ComputeHashsum(ByteUtils.Combine(key, binSecret));
            keyReversed = ComputeHashsum(ByteUtils.Combine(keyReversed, binSecret));

            _encryptKey = keyReversed;
            _encryptIv = reversed.SubArray(32, 16);
            _decryptKey = key;
            _decryptIv = randomBuffer.SubArray(40, 16);

            _cryptoTransformEncrypt = CreateEncryptorFromAes(_encryptKey);
            _cryptoTransformDecrypt = CreateEncryptorFromAes(_decryptKey);

            var decryptedBuffer = DecryptObfuscated2(randomBuffer, randomBuffer.Length);
            for (var i = 56; i < decryptedBuffer.Length; i++)
            {
                randomBuffer[i] = decryptedBuffer[i];
            }

            byte[] protocolResult = randomBuffer.SubArray(56, 4);
            if (protocolResult[0] == 0xef && protocolResult[1] == 0xef && protocolResult[2] == 0xef && protocolResult[3] == 0xef)
            {
                _protocolType = ProtocolType.AbridgedObfuscated2;
            }
            else if (protocolResult[0] == 0xee && protocolResult[1] == 0xee && protocolResult[2] == 0xee && protocolResult[3] == 0xee)
            {
                _protocolType = ProtocolType.IntermediateObfuscated2;
            }
            else
            {
                _protocolType = ProtocolType.None;
            }
            Array.Clear(reversed, 0, reversed.Length);
            Array.Clear(key, 0, key.Length);
            Array.Clear(keyReversed, 0, keyReversed.Length);
            Array.Clear(binSecret, 0, binSecret.Length);
            Array.Clear(decryptedBuffer, 0, decryptedBuffer.Length);
            Array.Clear(protocolResult, 0, protocolResult.Length);
        }
        public byte[] GetInitBufferObfuscated2(in ProtocolType protocolType)
        {
            _protocolType = protocolType;

            var buffer = new byte[64];
            while (true)
            {
                buffer.GenerateRandomBytes();

                var val = buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0];
                var val2 = buffer[7] << 24 | buffer[6] << 16 | buffer[5] << 8 | buffer[4];
                if (buffer[0] != 0xef
                    && val != 0x44414548
                    && val != 0x54534f50
                    && val != 0x20544547
                    && val != 0x4954504f
                    && val2 != 0x00000000)
                {
                    switch (_protocolType)
                    {
                        case ProtocolType.AbridgedObfuscated2:
                            buffer[56] = buffer[57] = buffer[58] = buffer[59] = 0xef;
                            break;
                        case ProtocolType.IntermediateObfuscated2:
                            buffer[56] = buffer[57] = buffer[58] = buffer[59] = 0xee;
                            break;
                        case ProtocolType.None:
                            return null;
                    }
                    break;
                }
            }
            var keyIvEncrypt = buffer.SubArray(8, 48);
            _encryptKey = keyIvEncrypt.SubArray(0, 32);
            _encryptIv = keyIvEncrypt.SubArray(32, 16);

            Array.Reverse(keyIvEncrypt);
            _decryptKey = keyIvEncrypt.SubArray(0, 32);
            _decryptIv = keyIvEncrypt.SubArray(32, 16);

            _cryptoTransformEncrypt = CreateEncryptorFromAes(_encryptKey);
            _cryptoTransformDecrypt = CreateEncryptorFromAes(_decryptKey);

            var encryptedBuffer = EncryptObfuscated2(buffer, buffer.Length);
            for (var i = 56; i < encryptedBuffer.Length; i++)
            {
                buffer[i] = encryptedBuffer[i];
            }
            Array.Clear(keyIvEncrypt, 0, keyIvEncrypt.Length);
            Array.Clear(encryptedBuffer, 0, encryptedBuffer.Length);
            return buffer;
        }
        private byte[] AesCtr128Encrypt(in byte[] buffer, in int length, ref byte[] ivec, ref byte[] ecountBuf, ref uint number)
        {
            var output = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (number == 0)
                {
                    ecountBuf = _cryptoTransformEncrypt.TransformFinalBlock(ivec, 0, ivec.Length);
                    Array.Reverse(ivec);
                    var bigInteger = new BigInteger(ByteUtils.Combine(ivec, new byte[] { 0x00 }));
                    bigInteger++;
                    var bigIntegerArray = bigInteger.ToByteArray();
                    Buffer.BlockCopy(bigIntegerArray, 0, ivec, 0, Math.Min(bigIntegerArray.Length, ivec.Length));
                    Array.Reverse(ivec);
                    Array.Clear(bigIntegerArray, 0, bigIntegerArray.Length);
                }
                output[i] = (byte)(buffer[i] ^ ecountBuf[number]);
                number = (number + 1) % 16;
            }
            return output;
        }
        private byte[] AesCtr128Decrypt(in byte[] buffer, in int length, ref byte[] ivdc, ref byte[] dcountBuf, ref uint number)
        {
            var output = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (number == 0)
                {
                    dcountBuf = _cryptoTransformDecrypt.TransformFinalBlock(ivdc, 0, ivdc.Length);
                    Array.Reverse(ivdc);
                    var bigInteger = new BigInteger(ByteUtils.Combine(ivdc, new byte[] { 0x00 }));
                    bigInteger++;
                    var bigIntegerArray = bigInteger.ToByteArray();
                    Buffer.BlockCopy(bigIntegerArray, 0, ivdc, 0, Math.Min(bigIntegerArray.Length, ivdc.Length));
                    Array.Reverse(ivdc);
                    Array.Clear(bigIntegerArray, 0, bigIntegerArray.Length);
                }
                output[i] = (byte)(buffer[i] ^ dcountBuf[number]);
                number = (number + 1) % 16;
            }
            return output;
        }
        public void Clear()
        {
            _encryptKey = null;
            _encryptIv = null;
            _decryptKey = null;
            _decryptIv = null;
            _encryptCountBuf = null;
            _encryptNum = 0;
            _decryptCountBuf = null;
            _decryptNum = 0;
        }
        public static ICryptoTransform CreateEncryptorFromAes(in byte[] key)
        {
            using (var aesManaged = new AesManaged())
            {
                aesManaged.Key = key;
                aesManaged.Mode = CipherMode.ECB;
                aesManaged.Padding = PaddingMode.None;
                return aesManaged.CreateEncryptor();
            }
        }
        public static byte[] ComputeHashsum(in byte[] data)
        {
            using (var sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(data, 0, data.Length);
            }
        }
        public byte[] EncryptObfuscated2(in byte[] data, in int length)
        {
            return AesCtr128Encrypt(data, length, ref _encryptIv, ref _encryptCountBuf, ref _encryptNum);
        }
        public byte[] DecryptObfuscated2(in byte[] data, in int length)
        {
            return AesCtr128Decrypt(data, length, ref _decryptIv, ref _decryptCountBuf, ref _decryptNum);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(in bool isDisposing)
        {
            if (!isDisposing)
            {
                return;
            }
            Clear();
            if (_cryptoTransformDecrypt != null)
            {
                try
                {
                    _cryptoTransformDecrypt.Dispose();
                    _cryptoTransformDecrypt = null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if (_cryptoTransformEncrypt != null)
            {
                try
                {
                    _cryptoTransformEncrypt.Dispose();
                    _cryptoTransformEncrypt = null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}