using BlubLib.Reflection;
using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebZen.Serialization
{
    public class BinarySerializer : ISerializerCompiler
    {
        private readonly int _size;
        public BinarySerializer(int Size)
        {
            _size = Size;
        }

        public bool CanHandle(Type type)
        {
            //throw new NotImplementedException();
            if (type == typeof(byte[]))
                return true;
            return false;
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            // value = BinaryReaderExtensions.ReadIPEndPoint(reader)
            var elementType = value.LocalType.GetElementType();
            // value = new [length]
            emiter.LoadConstant(_size);
            emiter.NewArray(elementType);
            emiter.StoreLocal(value);
            emiter.LoadArgument(1);
            emiter.LoadConstant(_size);
            emiter.Call(ReflectionHelper.GetMethod((BinaryReader x) => x.ReadBytes(default(int))));
            emiter.StoreLocal(value);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            using (var length = emiter.DeclareLocal<int>("length"))
            {
                // length = value.Length
                emiter.LoadLocal(value);
                emiter.Call(value.LocalType.GetProperty(nameof(Array.Length)).GetMethod);
                emiter.StoreLocal(length);

                var elementType = value.LocalType.GetElementType();

                var loop = emiter.DefineLabel();
                var loopCheck = emiter.DefineLabel();
                var loopFill = emiter.DefineLabel();
                var loopFillCheck = emiter.DefineLabel();

                using (var element = emiter.DeclareLocal(elementType, "element"))
                using (var i = emiter.DeclareLocal<int>("i"))
                {
                    emiter.Branch(loopCheck);
                    emiter.MarkLabel(loop);

                    // element = value[i]
                    emiter.LoadLocal(value);
                    emiter.LoadLocal(i);
                    emiter.LoadElement(elementType);
                    emiter.StoreLocal(element);

                    emiter.CallSerializerForType(elementType, element);

                    // ++i
                    emiter.LoadLocal(i);
                    emiter.LoadConstant(1);
                    emiter.Add();
                    emiter.StoreLocal(i);

                    // i < length
                    emiter.MarkLabel(loopCheck);
                    emiter.LoadLocal(i);
                    emiter.LoadLocal(length);
                    emiter.BranchIfLess(loop);

                    emiter.Branch(loopFillCheck);
                    emiter.MarkLabel(loopFill);
                    // element = 0
                    emiter.LoadConstant(0);
                    emiter.StoreLocal(element);

                    emiter.CallSerializerForType(elementType, element);
                    // ++i
                    emiter.LoadLocal(i);
                    emiter.LoadConstant(1);
                    emiter.Add();
                    emiter.StoreLocal(i);

                    emiter.MarkLabel(loopFillCheck);
                    emiter.LoadLocal(i);
                    emiter.LoadConstant(_size);
                    emiter.BranchIfLess(loopFill);
                }
            }
        }
    }
}
