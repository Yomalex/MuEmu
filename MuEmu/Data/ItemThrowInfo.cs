using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Data
{
    public class ItemThrowInfo
    {
        public ItemNumber Number { get; set; }
        public ushort Plus { get; set; }
        public ushort LevelMin { get; set; }
        public Storage Storage { get; set; }
    }
}
