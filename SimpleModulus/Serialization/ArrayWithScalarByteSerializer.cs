using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WebZen.Serialization
{
    public class ArrayWithScalarSerializer<T> : ISerializerCompiler
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
            var emptyArray = emiter.DefineLabel();
            var end = emiter.DefineLabel();
            Type lType = typeof(T);

            if(lType.IsClass || lType.IsInterface)
            {
                lType = typeof(T).GetMethod("op_Implicit", new[] { typeof(T) }).ReturnType;
            }
            
            using (var length = emiter.DeclareLocal<T>("length"))
            using(var l = emiter.DeclareLocal(lType, "lengthLoop"))
            {
                emiter.CallDeserializerForType(length.LocalType, length);
                if(length.LocalType.IsClass || length.LocalType.IsInterface)
                {
                    emiter.LoadLocal(length);
                    emiter.Call(typeof(T).GetMethod("op_Implicit", new[] { typeof(T) }));
                    emiter.StoreLocal(l);
                }
                else
                {
                    emiter.LoadLocal(length);
                    emiter.StoreLocal(l);
                }

                // if(length < 1) {
                //  value = Array.Empty<>()
                //  return
                // }
                emiter.LoadLocal(l);
                emiter.LoadConstant(1);
                emiter.BranchIfLess(emptyArray);

                // value = new [length]
                emiter.LoadLocal(l);
                emiter.NewArray(elementType);
                emiter.StoreLocal(value);

                var loop = emiter.DefineLabel();
                var loopCheck = emiter.DefineLabel();

                using (var element = emiter.DeclareLocal(elementType, "element"))
                using (var i = emiter.DeclareLocal<int>("i"))
                {
                    emiter.MarkLabel(loop);

                    if (_compiler != null)
                        _compiler.EmitDeserialize(emiter, element);
                    else if (_serializer != null)
                        emiter.CallDeserializer(_serializer, element);
                    else
                        emiter.CallDeserializerForType(elementType, element);

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
                    emiter.LoadLocal(l);
                    emiter.BranchIfLess(loop);
                }
                emiter.Branch(end);
            }

            // value = Array.Empty<>()
            emiter.MarkLabel(emptyArray);
            emiter.Call(typeof(Array)
                .GetMethod(nameof(Array.Empty))
                .GetGenericMethodDefinition()
                .MakeGenericMethod(elementType));
            emiter.StoreLocal(value);
            emiter.MarkLabel(end);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            var elementType = value.LocalType.GetElementType();
            var scalarType = typeof(T);
            using (var length = emiter.DeclareLocal<T>("length"))
            using(var l = emiter.DeclareLocal<int>("lengthLoop"))
            {
                // length = value.Length
                emiter.LoadLocal(value);
                emiter.Call(value.LocalType.GetProperty(nameof(Array.Length)).GetMethod);
                emiter.StoreLocal(l);
                emiter.LoadLocal(l);

                if (scalarType.IsClass || scalarType.IsInterface)
                {
                    emiter.Call(typeof(T).GetMethod("op_Implicit", new[] { l.LocalType }));
                }
                emiter.StoreLocal(length);

                emiter.CallSerializerForType(length.LocalType, length);

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
                    emiter.LoadLocal(l);
                    emiter.BranchIfLess(loop);
                }
            }
        }
    }
}
