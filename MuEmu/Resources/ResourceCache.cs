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
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceCache));

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
    }
}
