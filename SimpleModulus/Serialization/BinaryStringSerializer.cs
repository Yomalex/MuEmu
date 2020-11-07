using BlubLib.Reflection;
using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebZen.Util;

namespace WebZen.Serialization
{
    public class BinaryStringSerializer: ISerializerCompiler
    {
        private readonly int _size;
        public BinaryStringSerializer(int Size)
        {
            _size = Size;
        }

        public bool CanHandle(Type type)
        {
            //throw new NotImplementedException();
            if (type == typeof(string))
                return true;
            return false;
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.LoadConstant(_size);
            emiter.Call(typeof(CString).GetMethod(nameof(CString.ReadString)));
            emiter.StoreLocal(value);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.LoadLocal(value);
            emiter.LoadConstant(_size);
            emiter.Call(typeof(CString).GetMethod(nameof(CString.WriteString),
                new[] { typeof(BinaryWriter), typeof(string), typeof(int) }));
        }
    }
}
