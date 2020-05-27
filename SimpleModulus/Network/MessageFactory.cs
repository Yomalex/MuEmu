using BlubLib;
using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebZen.Network
{
    public class MessageFactory
    {
        private readonly Dictionary<ushort, Type> _typeLookup = new Dictionary<ushort, Type>();
        private readonly Dictionary<Type, ushort> _opCodeLookup = new Dictionary<Type, ushort>();

        protected void Register<T>(ushort opCode)
            where T : new()
        {
            var type = typeof(T);
            _opCodeLookup.Add(type, opCode);

            if(!_typeLookup.ContainsKey(opCode))
                _typeLookup.Add(opCode, type);
        }

        public ushort GetOpCode(Type type)
        {
            ushort opCode;
            if (_opCodeLookup.TryGetValue(type, out opCode))
                return opCode;

            throw new Exception($"No opcode found for type {type.FullName}");
        }

        public object GetMessage(ushort opCode, Stream stream)
        {
            Type type;
            if (!_typeLookup.TryGetValue(opCode, out type))
                throw new Exception($"No type found for opcode {opCode}");

            try
            {
                return Serializer.Deserialize(stream, type);
            }catch(Exception ex)
            {
                throw new Exception("Type: " + type + "OPCode: " + opCode, ex);
            }
        }

        public object GetMessage(ushort opCode, BinaryReader reader)
        {
            Type type;
            if (!_typeLookup.TryGetValue(opCode, out type))
                throw new ArgumentException($"bad OPCode {opCode}");

            return Serializer.Deserialize(reader, type);
        }

        public bool ContainsType(Type type)
        {
            return _opCodeLookup.ContainsKey(type);
        }

        public bool ContainsOpCode(ushort opCode)
        {
            return _typeLookup.ContainsKey(opCode);
        }
    }

    public class MessageFactory<TOpCode, TMessage> : MessageFactory
    {
        protected void Register<T>(TOpCode opCode)
            where T : TMessage, new()
        {
            Register<T>(DynamicCast<ushort>.From(opCode));
        }

        public new TOpCode GetOpCode(Type type)
        {
            return DynamicCast<TOpCode>.From(base.GetOpCode(type));
        }

        public TMessage GetMessage(TOpCode opCode, Stream stream)
        {
            return (TMessage)GetMessage(DynamicCast<ushort>.From(opCode), stream);
        }

        public TMessage GetMessage(TOpCode opCode, BinaryReader reader)
        {
            return (TMessage)GetMessage(DynamicCast<ushort>.From(opCode), reader);
        }

        public bool ContainsOpCode(TOpCode opCode)
        {
            return ContainsOpCode(DynamicCast<ushort>.From(opCode));
        }
    }
}
