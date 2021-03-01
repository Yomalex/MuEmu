using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Serialization
{
    public class ArraySerializer : ISerializerCompiler
    {
        private readonly ISerializerCompiler _compiler;
        private readonly ISerializer _serializer;

        public bool CanHandle(Type type)
        {
            throw new NotImplementedException();
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            var elementType = value.LocalType.GetElementType();

            //value = new Type[100];
            emiter.LoadConstant(100);
            emiter.NewArray(elementType);
            emiter.StoreLocal(value);
            var ex = emiter.DeclareLocal<Exception>("_ex");

            var loop = emiter.DefineLabel();
            var loopCheck = emiter.DefineLabel();

            using (var element = emiter.DeclareLocal(elementType, "element"))
            using (var i = emiter.DeclareLocal<int>("i"))
            {
                emiter.MarkLabel(loop);

                var eb = emiter.BeginExceptionBlock();
                if (_compiler != null)
                    _compiler.EmitDeserialize(emiter, element);
                else if (_serializer != null)
                    emiter.CallDeserializer(_serializer, element);
                else
                    emiter.CallDeserializerForType(elementType, element);
                var cb = emiter.BeginCatchBlock<Exception>(eb);
                emiter.StoreLocal(ex);
                emiter.EndCatchBlock(cb);
                emiter.EndExceptionBlock(eb);

                // value[i] = element
                emiter.LoadLocal(value);
                emiter.LoadLocal(i);
                emiter.LoadLocal(element);
                emiter.StoreElement(elementType);

                // ++i
                emiter.LoadLocal(i);
                emiter.LoadConstant(1);
                emiter.Add();
                emiter.StoreLocal(i);

                // i < length
                emiter.MarkLabel(loopCheck);
                emiter.LoadLocal(i);
                emiter.LoadConstant(100);
                emiter.BranchIfLess(loop);
            }
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            var elementType = value.LocalType.GetElementType();
            using (var length = emiter.DeclareLocal<int>("length"))
            {
                // length = value.Length
                emiter.LoadLocal(value);
                emiter.Call(value.LocalType.GetProperty(nameof(Array.Length)).GetMethod);
                emiter.StoreLocal(length);

                //emiter.CallSerializerForType(length.LocalType, length);

                var loop = emiter.DefineLabel();
                var loopCheck = emiter.DefineLabel();

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

                    if (_compiler != null)
                        _compiler.EmitSerialize(emiter, element);
                    else if (_serializer != null)
                        emiter.CallSerializer(_serializer, element);
                    else
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
                }
            }
        }
    }
}
