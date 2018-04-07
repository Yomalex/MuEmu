using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Serialization
{
    public class WZMemberAttribute : BlubMemberAttribute
    {

        public WZMemberAttribute(int order)
            : base(order)
        { }

        public WZMemberAttribute(int order, int size)
            : base(order, typeof(BinarySerializer), size)
        { }

        public WZMemberAttribute(int order, Type serializerType, params object[] serializerParameters)
            : base(order, serializerType, serializerParameters)
        { }
    }
}
