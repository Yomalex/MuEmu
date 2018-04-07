using System;
using System.Collections.Generic;
using System.Text;

namespace WebZen.Handlers
{
    public class MessageHandlerAttribute : Attribute
    {
        public readonly Type _type;

        public MessageHandlerAttribute(Type type)
        {
            _type = type;
        }
    }
}
