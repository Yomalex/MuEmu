using BlubLib.Caching;
using MuEmu.Data;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MuEmu.Resources.Map;
using MuEmu.Resources.Game;

namespace MuEmu.Resources
{
    public class ResourceCache
    {
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceCache));

        public static ResourceCache Instance { get; private set; }
        private ResourceLoader _loader;
        private MemoryCache _cache;

        public ResourceCache(string root)
        {
            _loader = new ResourceLoader(root);
            _cache = new MemoryCache();
        }

        public static void Initialize(string root)
        {
            if (Instance != null)
                throw new Exception("Already initialized");

            Instance = new ResourceCache(root);

            Instance.GetItems();
            Instance.GetSkills();
            Instance.GetMaps();
            Instance.GetDefChar();
            Instance.GetShops();
            Instance.GetNPCs();
            Instance.GetJOH();
            Instance.GetGates();
            Instance.GetQuests();
            Instance.GetChaosMixInfo();
        }

        public IDictionary<ushort, ItemInfo> GetItems()
        {
            var cache = _cache.Get<IDictionary<ushort, ItemInfo>>("Items");
            if(cache == null)
            {
                Logger.Information("Items Caching...");
                cache = _loader.LoadItems().ToDictionary(x => x.Number);
                _cache.Set("Items", cache);
            }

            return cache;
        }

        public IDictionary<Spell, SpellInfo> GetSkills()
        {
            var cache = _cache.Get<IDictionary<Spell, SpellInfo>>("Spells");
            if (cache == null)
            {
                Logger.Information("Spells Caching...");
                cache = _loader.LoadSkills().ToDictionary(x => (Spell)x.Number);
                _cache.Set("Spells", cache);
            }

            return cache;
        }

        public IDictionary<Maps, MapInfo> GetMaps()
        {
            var cache = _cache.Get<IDictionary<Maps, MapInfo>>("Maps");
            if (cache == null)
            {
                Logger.Information("Maps Caching...");
                cache = _loader.LoadMaps().ToDictionary(x => (Maps)x.Map);
                _cache.Set("Maps", cache);
            }

            return cache;
        }

        public IDictionary<HeroClass, CharacterInfo> GetDefChar()
        {
            var cache = _cache.Get<IDictionary<HeroClass, CharacterInfo>>("DefClass");
            if (cache == null)
            {
                Logger.Information("DefClass Caching...");
                cache = _loader.LoadDefCharacter().ToDictionary(x => x.Class);
                _cache.Set("DefClass", cache);
            }

            return cache;
        }

        public IDictionary<ushort, ShopInfo> GetShops()
        {
            var cache = _cache.Get<IDictionary<ushort, ShopInfo>>("Shops");
            if (cache == null)
            {
                Logger.Information("Shops Caching...");
                cache = _loader.LoadShops().ToDictionary(x => x.Shop);
                _cache.Set("Shops", cache);
            }

            return cache;
        }

        public IDictionary<ushort, NPCInfo> GetNPCs()
        {
            var cache = _cache.Get<IDictionary<ushort, NPCInfo>>("NPCs");
            if (cache == null)
            {
                Logger.Information("NPCs Caching...");
                cache = _loader.LoadNPCs().ToDictionary(x => x.NPC);
                _cache.Set("NPCs", cache);
            }

            return cache;
        }

        public IDictionary<byte, JewelOfHarmonyOption> GetJOH()
        {
            var cache = _cache.Get<IDictionary<byte, JewelOfHarmonyOption>>("JOH");
            if (cache == null)
            {
                Logger.Information("JOH Caching...");
                cache = _loader.LoadJOH().ToDictionary(x => (byte)(x.Type << 4 | x.Index));
                _cache.Set("JOH", cache);
            }

            return cache;
        }

        public IDictionary<int, Gate> GetGates()
        {
            var cache = _cache.Get<IDictionary<int, Gate>>("Gates");
            if (cache == null)
            {
                Logger.Information("Gates Caching...");
                cache = _loader.LoadGates().ToDictionary(x => x.Number);
                _cache.Set("Gates", cache);
            }

            return cache;
        }

        public IDictionary<int, QuestInfo> GetQuests()
        {
            var cache = _cache.Get<IDictionary<int, QuestInfo>>("Quests");
            if (cache == null)
            {
                Logger.Information("Quests Caching...");
                cache = _loader.LoadQuests().ToDictionary(x => x.Index);
                _cache.Set("Quests", cache);
            }

            return cache;
        }

        public ChaosMixInfo GetChaosMixInfo()
        {
            var cache = _cache.Get<ChaosMixInfo>("ChaosMix");
            if (cache == null)
            {
                Logger.Information("ChaosMix Caching...");
                cache = _loader.LoadChaosBox();
                _cache.Set("ChaosMix", cache);
            }

            return cache;
        }

        public void ReloadShops()
        {
            _cache.Remove("Shops");
            GetShops();
        }

        public void ReloadGates()
        {
            _cache.Remove("Gates");
            GetGates();
        }

        public void ReloadChaosMix()
        {
            _cache.Remove("ChaosMix");
            GetChaosMixInfo();
        }
    }
}
