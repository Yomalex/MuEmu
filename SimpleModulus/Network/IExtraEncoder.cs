using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebZen.Network
{
    internal interface IExtraEncoder
    {
        void Encoder(MemoryStream ms);
    }
}
