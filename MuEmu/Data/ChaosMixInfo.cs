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
        public int Count { get; set; }
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
            else if (Level.Length == 2 && Level[0] > item.Plus && Level[1] < item.Plus)
                return false;

            if (Luck != -1 && (item.Luck ? 1 : 0) != Luck) // No match LUCK
                return false;

            if (Skill != -1 && (item.Skill ? 1 : 0) != Skill) // No match Skill
                return false;

            if (Option > (item.Option28*4)) // No match Skill
                return false;

            return true;
        }
    }
    public class MixInfo
    {
        public string Name { get; set; }
        public int GeneralSuccess { get; set; }
        public int BaseCost { get; set; }
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

            var leftItems = (from obj in cbItems
                            where obj.Value.Number != ItemNumber.FromTypeIndex(12, 15) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 13) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 14) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 16) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 22)
                            select obj).ToList();

            var pairIng = (from ing in Ingredients
                          from it in leftItems
                          where ing.Match(it.Value)
                          select new { Address = it.Key, Item = it.Value, Ingredient = ing }).ToList();

            var successRate = GeneralSuccess;
            foreach(var ing in ingOk)
            {
                var scalar = (ing.Value - ing.Key.Count);
                successRate += scalar * 10;
            }

            if (successRate > 100)
                successRate = 100;
            var rand = new Random();
            if (rand.Next(0, 100) > successRate - 1)
            {
                if(ResultFail.IID == -1)
                    @char.Inventory.DeleteChaosBox();

                return ChaosBoxMixResult.Fail;
            }

            @char.Inventory.DeleteChaosBox();

            var prob = rand.Next(0, 100);
            IngredientInfo res = null;
            foreach (var r in ResultSuccess)
            {
                if(prob < r.Success)
                {
                    res = r;
                    break;
                }
                prob -= r.Success;
            }

            if (res == null)
                res = ResultSuccess.First();

            var res2 = pairIng.FirstOrDefault(x => x.Ingredient.IID == res.IID);
            if(res2.Item != null)
            {
                var mix = new Item(res2.Item.Number);
                if(res.Level.Length == 1)
                {
                    mix.Plus = res.Level[0] == 255 ? res2.Item.Plus : (byte)res.Level[0];
                }else if(res.Level.Length == 2)
                {
                    mix.Plus = (byte)rand.Next(res.Level[0], res.Level[1]);
                }else
                {
                    mix.Plus = res2.Item.Plus;
                }

                mix.Luck = res.Luck == -1 ? res2.Item.Luck : (res.Luck > 0 ? true : false);
                mix.Skill = res.Skill == -1 ? res2.Item.Skill : (res.Skill > 0 ? true : false);
                mix.Option28 = (byte)(res.Option == -1 ? res2.Item.Option28 : res.Option);
                mix.Character = @char;
                mix.AccountId = @char.Account.ID;
                mix.CharacterId = @char.Id;
                @char.Inventory.ChaosBox.Add(mix);
            }

            return ChaosBoxMixResult.Success;
        }
    }
    public class ChaosMixInfo
    {
        public List<JewelInfo> AdditionalJewels { get; set; }
        public List<MixInfo> Mixes { get; set; }

        public MixInfo FindMix(Character @char)
        {
            var cbItems = @char.Inventory.ChaosBox.Items;
            var items = from obj in cbItems
                            where obj.Value.Number != ItemNumber.FromTypeIndex(12, 15) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 13) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 14) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 16) &&
                            obj.Value.Number != ItemNumber.FromTypeIndex(14, 22)
                            select obj.Value;

            foreach (var m in Mixes)
            {
                foreach (var i in items)
                {
                    if (m.Ingredients.Any(x => x.Match(i)))
                        return m;
                }
            }

            return null;
        }
    }
}
