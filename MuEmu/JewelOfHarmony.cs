using MU.Resources;
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
        public byte Type => GetItemType();
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
        public string EffectName => GetEffectName();

        public async void UseRefiningStone(Item refiningStone)
        {
            if (Option == 0)
                return;

            var percent = refiningStone.Number.Number == 7211 ? 25 : 75;

            if (percent < Program.RandomProvider(100))
            {
                Level = 0;
            }
            else
            {
                Level++;
            }
            Item.OnItemChange();
            await Item.Character.Inventory.Delete(refiningStone);
        }

        private byte GetItemType()
        {
            if (Item == null)
                return 0;

            if (Item.Number.Type < ItemType.Staff)
                return 1;

            if (Item.Number.Type == ItemType.Staff)
                return 2;

            if (Item.Number.Type <= ItemType.Boots)
                return 3;

            return 0;
        }
        private int GetEffectValue()
        {
            var joh = ResourceCache.Instance.GetJOH();

            JOHSectionDto dto = null;
            if (Option < 1)
                return 0;

            switch(Type)
            {
                case 1:
                    dto = joh.Weapon[Option-1];
                    break;
                case 2:
                    dto = joh.Staff[Option-1];
                    break;
                case 3:
                    dto = joh.Defense[Option-1];
                    break;
            }

            if (dto == null)
                return 0;

            var type = typeof(JOHSectionDto);
            var prop = type.GetProperty("Level" + Level);
            var get = prop.GetGetMethod();
            return (int)get.Invoke(dto, null);
        }
        private string GetEffectName()
        {
            var joh = ResourceCache.Instance.GetJOH();

            JOHSectionDto dto = null;
            if (Option < 1)
                return "";

            switch (Type)
            {
                case 1:
                    dto = joh.Weapon[Option - 1];
                    break;
                case 2:
                    dto = joh.Staff[Option - 1];
                    break;
                case 3:
                    dto = joh.Defense[Option - 1];
                    break;
            }

            if (dto == null)
                return "";

            return dto.Name;
        }
    }
}
