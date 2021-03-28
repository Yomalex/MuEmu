using MU.Resources.Game;
using MuEmu.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class JewelOfHarmony
    {
        public byte Option { get; set; }
        public byte Level { get; set; }
        public Item Item { get; set; }
        public byte Type => (byte)((Item!=null)?(Item.Number < 5 ? 1 : (Item.Number >= 5 && Item.Number < 6 ? 2 : (Item.Number >= 6 && Item.Number < 12 ? 3 : 0))):0);
        public byte Index => (byte)(Type << 4 | Option);

        public static implicit operator byte(JewelOfHarmony a)
        {
            return (byte)(a.Option << 4 | a.Level);
        }

        public static implicit operator JewelOfHarmony(byte a)
        {
            return new JewelOfHarmony { Option = (byte)(a >> 4), Level = (byte)(a & 0x0f) };
        }

        public int EffectValue => GetEffectValue();

        private int GetEffectValue()
        {
            var joh = ResourceCache.Instance.GetJOH();

            JOHSectionDto dto = null;
            switch(Type)
            {
                case 1:
                    dto = joh.Weapon[Index];
                    break;
                case 2:
                    dto = joh.Pet[Index];
                    break;
                case 3:
                    dto = joh.Defense[Index];
                    break;
            }

            if (dto == null)
                return 0;

            var type = typeof(JOHSectionDto);
            var prop = type.GetProperty("Level" + Level);
            var get = prop.GetGetMethod();
            return (int)get.Invoke(dto, null);
        }
    }
}
