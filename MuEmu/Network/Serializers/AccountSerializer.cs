using BlubLib.Reflection;
using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MuEmu.Network.Serializers
{
    public class AccountSerializer : ISerializerCompiler
    {
        public bool CanHandle(Type type)
        {
            throw new NotImplementedException();
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            // value = BinaryReaderExtensions.ReadIPEndPoint(reader)
            emiter.LoadArgument(1);
            emiter.LoadConstant(10);
            emiter.CallVirtual(typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadBytes), new[] { typeof(int) }));
            emiter.StoreLocal(value);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            // BinaryWriterExtensions.Write(writer, value)
            emiter.LoadArgument(1);
            emiter.LoadLocalAddress(value);
            emiter.CallVirtual(typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), new[] { typeof(byte[]) }));
        }
    }
}
