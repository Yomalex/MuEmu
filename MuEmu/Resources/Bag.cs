using MuEmu.Resources.XML;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu.Resources
{
    public class Bag : BagDto
    {
        public Bag(BagDto basea)
        {
            Extensions.AnonymousMap(this, basea);
        }

        public Item[] GetReward()
        {
            var result = new List<Item>();
            for(var i = 0; i < DropItemCount; i++)
            {
                if (DropItemRate < Program.RandomProvider(100))
                    continue;

                var tmp = Item[Program.RandomProvider(Item.Length-1)];
                var OptionExe = (byte)Program.RandomProvider(tmp.MaxExcellent, tmp.MinExcellent);
                var Option28 = (byte)Program.RandomProvider(tmp.MaxOption, tmp.MinOption);
                var Plus = (byte)Program.RandomProvider(tmp.MaxLevel, tmp.MinLevel);
                var Luck = Program.RandomProvider(tmp.Luck ? 1:0)!=0 ? true : false;
                var Skill = Program.RandomProvider(tmp.Skill ? 1:0)!=0 ? true : false;
                result.Add(new Item(tmp.Number, Options: new { Plus, Option28, OptionExe, Luck, Skill }));
            }

            if (DropZenRate <= Program.RandomProvider(100))
                result.Add(MuEmu.Item.Zen((uint)Program.RandomProvider(MaxZen, MinZen)));

            return result.ToArray();
        }
    }
}
