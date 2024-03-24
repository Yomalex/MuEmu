using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebZen.Serialization;

namespace MuEmu.Network.Data
{
    public abstract class AInventoryDto
    {
        [WZMember(0)]
        public byte Index { get; set; }

        [WZMember(1, 12)]
        public virtual byte[] Item { get; set; }
        public AInventoryDto()
        {

        }
        public AInventoryDto(byte id, byte[] item)
        {
        }
    }

    [WZContract]
    public class InventoryDto : AInventoryDto
    {
        public InventoryDto()
        {

        }
        public InventoryDto(byte id, byte[] item)
        {
            Index = id;
            Item = item;
        }
    }
    [WZContract]
    public class InventoryS17Dto : AInventoryDto
    {
        /*[WZMember(0)]
        public byte Index { get; set; }*/

        [WZMember(1, 15)]
        public override byte[] Item { get; set; }

        public InventoryS17Dto()
        {

        }
        public InventoryS17Dto(byte id, byte[] item)
        {
            Index = id;
            Item = item;
        }
    }
}
