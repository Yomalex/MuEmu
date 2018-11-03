using BlubLib.Caching;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
