using BlubLib.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Serialization
{
    public class WZContractAttribute : BlubContractAttribute
    {
        public bool Serialized { get; set; }

        public bool LongMessage { get; set; }

        public Type ExtraEncode { get; set; }

        public WZContractAttribute()
            : base()
        { }

        public WZContractAttribute(Type serializerType, params object[] serializerParameters)
            : base(serializerType, serializerParameters)
        { }
    }
}
