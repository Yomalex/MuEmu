using MU.Resources;
using MuEmu.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Data
{
    public class JewelInfo
    {
        public ItemNumber ItemNumber { get; set; }
        public int Success { get; set; }
    }
    public class IngredientInfo
    {
        public int IID { get; set; }
        public ItemNumber ItemNumber { get; set; }
        public int[] Level { get; set; }
        public int Luck { get; set; }
        public int Skill { get; set; }
        public int Option { get; set; }
        public int Excellent { get; set; }
        public int Count { get; set; }
        public int Harmony { get; set; }
        public int Success { get; set; }

        public bool Match(Item item)
        {
            if(ItemNumber == ItemNumber.Invalid) // AnyItem
            {

            }else if(ItemNumber.Type == ItemType.Invalid && ItemNumber.Index == item.Number.Index) // AnyType
            {

            }else if(ItemNumber.Index == 0x1FF && ItemNumber.Type == item.Number.Type) // AnyIndex
            {

            }else if(ItemNumber == item.Number) // ExactMatch
            {

            }else
            {
                return false;
            }

            if (Level.Length == 1 && Level[0] != 255 && Level[0] != item.Plus)
                return false;
            else if (Level.Length == 2 && Level[0] >= item.Plus && Level[1] <= item.Plus)
                return false;

            if (Luck != -1 && (item.Luck ? 1 : 0) != Luck) // No match LUCK
                return false;

            if (Skill != -1 && (item.Skill ? 1 : 0) != Skill) // No match Skill
                return false;

            if (Option > (item.Option28*4)) // No match Option
                return false;

            if (Excellent > item.ExcellentCount || (Excellent == 0 && item.ExcellentCount != 0))
                return false;

            if(Harmony > item.Harmony.Option)
                return false;

            return true;
        }
    }
    public class MixInfo
    {
        public string Name { get; set; }
        public int GeneralSuccess { get; set; }
        public int BaseCost { get; set; }
        public int NPC { get; set; }
        public List<IngredientInfo> Ingredients { get; set; }
        public List<IngredientInfo> ResultSuccess { get; set; }
        public IngredientInfo ResultFail { get; set; }

        public ChaosBoxMixResult Execute(Character @char)
        {
            var cbItems = @char.Inventory.ChaosBox.Items;
            var items = cbItems.Select(x => x.Value);

            if(BaseCost > @char.Money)
                return ChaosBoxMixResult.InsufficientMoney;

            @char.Money -= (uint)BaseCost;

            var ingOk = new Dictionary<IngredientInfo, int>();

            foreach(var ing in Ingredients)
            {
                var count = items.Where(x => ing.Match(x)).Count();
                if (ing.Count > count)
                    return ChaosBoxMixResult.Fail;

                ingOk.Add(ing, count);
            }

            var pairIng = (from ing in Ingredients
                          from it in cbItems
                          where ing.Match(it.Value)
                          select new { Address = it.Key, Item = it.Value, Ingredient = ing }).ToList();

            var leftItems = (from it in cbItems
                            where !pairIng.Any(x => x.Item == it.Value)
                            select it).ToList();

            var successRate = GeneralSuccess;
            foreach(var ing in ingOk)
            {
                var scalar = (ing.Value - ing.Key.Count);
                successRate += scalar * 10;
            }

            if (successRate > 100)
                successRate = 100;
            var rand = new Random();
            ChaosBoxMixResult result = ChaosBoxMixResult.Success;

            if (rand.Next(0, 100) > successRate - 1)
            {
                result = ChaosBoxMixResult.Fail;
            }

            @char.Inventory.DeleteChaosBox();

            var prob = rand.Next(0, 100);
            IngredientInfo res = null;
            if (result == ChaosBoxMixResult.Success)
            {
                foreach (var r in ResultSuccess)
                {
                    if (prob < r.Success)
                    {
                        res = r;
                        break;
                    }
                    prob -= r.Success;
                }
            }
            else
            {
                res = ResultFail;
            }

            if (res == null)
                res = ResultSuccess.First();

            var res2 = pairIng.FirstOrDefault(x => x.Ingredient.IID == res.IID);

            if (res.ItemNumber != ItemNumber.Invalid || res2 != null)
            {
                var mix = new Item(res.ItemNumber != ItemNumber.Invalid ? res.ItemNumber : res2.Item.Number);

                if (res.Level.Length == 1)
                {
                    mix.Plus = res.Level[0] == 255 ? res2.Item.Plus : (byte)res.Level[0];
                }
                else if (res.Level.Length == 2)
                {
                    mix.Plus = (byte)rand.Next(res.Level[0], res.Level[1]);
                }
                else
                {
                    mix.Plus = res2?.Item.Plus ?? 0;
                }

                if (res.Harmony != -1)
                {
                    mix.Harmony.Option = (byte)res.Harmony;
                    mix.Harmony.Level = 0;
                }
                else
                {
                    mix.Harmony.Option = res2?.Item.Harmony.Option ?? 0;
                    mix.Harmony.Level = res2?.Item.Harmony.Level ?? 0;
                }

                mix.Luck = res.Luck == -1 ? res2?.Item.Luck ?? false : (res.Luck > 0 ? true : false);
                mix.Skill = res.Skill == -1 ? res2?.Item.Skill ?? false : (res.Skill > 0 ? true : false);
                mix.Option28 = (byte)(res.Option == -1 ? res2?.Item.Option28 ?? 0x00 : res.Option);
                mix.OptionExe = (byte)(res.Excellent == -1 ? res2?.Item.OptionExe ?? 0x00 : res.Excellent);
                mix.Character = @char;
                mix.Account = @char.Account;
                mix.Character = @char;
                @char.Inventory.ChaosBox.Add(mix);
            }

            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
    public class ChaosMixInfo
    {
        public List<JewelInfo> AdditionalJewels { get; set; }
        public List<MixInfo> Mixes { get; set; }

        public MixInfo FindMix(Character @char)
        {
            var cbItems = @char.Inventory.ChaosBox.Items;
            var items = from obj in cbItems select obj.Value;
            var MixMatching = new Dictionary<MixInfo, int>();
            var npc = @char.Player.Window as Monster;

            foreach (var m in Mixes.Where(x => x.NPC == npc.Info.Monster))
            {
                var ingCount = 0;
                var iteCount = 0;
                var itemsUsed = new List<Item>();
                foreach(var ing in m.Ingredients)
                {
                    ingCount += ing.Count;
                    var a = items.Where(x => ing.Match(x));
                    itemsUsed.AddRange(a);
                    iteCount += a.Count();
                }
                var res = (float)iteCount;

                res /= ingCount;
                res *= 100.0f;
                var leftItems = items.Except(itemsUsed);
                var leftItemsWithOutJewels = leftItems.Where(x => x.Number != ItemNumber.FromTypeIndex(14, 13) && x.Number != ItemNumber.FromTypeIndex(14, 14));

                if (leftItemsWithOutJewels.Count() > 0)
                {
                    res = 0;
                }

                MixMatching.Add(m, (int)res);
            }

            return MixMatching
                //.OrderByDescending(x => x.Value)
                .Where(x => x.Value == 100)
                .Select(x => x.Key)
                .FirstOrDefault();
        }
    }
}
