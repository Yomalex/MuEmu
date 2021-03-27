using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using MuEmu.Resources;
using System.Drawing;
using MU.Network.Game;
using MuEmu.Network.Data;
using MuEmu.Events.BloodCastle;
using Serilog;
using Serilog.Core;
using MuEmu.Entity;
using WebZen.Util;
using MuEmu.Resources.Map;
using System.Threading.Tasks;
using MuEmu.Util;
using MU.Resources;
using MU.Network.Guild;
using MU.Network.GensSystem;

namespace MuEmu
{
    public class DelayedMessage
    {
        public Player Player;
        public DateTimeOffset Time;
        public object Message;
    }

    public class SubSystem
    {
        public static GameContext db;
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(SubSystem));
        private Thread _workerDelayed;
        private Thread _workerViewPort;
        private Thread _workerEvents;
        private Thread _workerSavePlayers;
        private Thread _workerIA;
        private List<DelayedMessage> _delayedMessages;
        public static SubSystem Instance { get; set; }

        public SubSystem()
        {
            _delayedMessages = new List<DelayedMessage>();
            _workerDelayed = new Thread(WorkerDelayed);
            _workerViewPort = new Thread(WorkerViewPort);
            _workerEvents = new Thread(WorkerEvents);
            _workerSavePlayers = new Thread(WorkerSavePlayers);
            _workerIA = new Thread(WorkerIA);
        }

        public void AddDelayedMessage(Player plr, TimeSpan time, object message)
        {
            if (plr == null)
                return;

            _delayedMessages.Add(new DelayedMessage { Player = plr, Message = message, Time = DateTimeOffset.Now.Add(time) });
        }

        private static async void WorkerDelayed()
        {
            while(true)
            {
                try
                {
                    var toSend = (from msg in Instance._delayedMessages
                                  where DateTimeOffset.Now >= msg.Time
                                  select msg).ToList();

                    foreach (var msg in toSend)
                    {
                        await msg.Player.Session.SendAsync(msg.Message);
                    }

                    Instance._delayedMessages = (from msg in Instance._delayedMessages
                                                 where DateTimeOffset.Now < msg.Time
                                                 select msg).ToList();

                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Logger.Error(e, DateTime.Now.ToString());
                }
            }
        }

        private static void WorkerViewPort()
        {
            while(true)
            {
                try
                {
                    foreach (var map in ResourceCache.Instance.GetMaps().Values)
                    {
                        var deadPlayers = new List<Character>();
                        foreach (var @char in map.Players)
                        {
                            @char.Friends.Update();
                            switch (@char.State)
                            {
                                case ObjectState.Regen:
                                    @char.State = ObjectState.Live;
                                    @char.Spells.ClearAll();
                                    break;
                                case ObjectState.Live:
                                    // Clear dead buffers
                                    @char.Spells.ClearBuffTimeOut();

                                    PlayerPlrViewport(map, @char);
                                    PlayerMonsViewport(map, @char);
                                    PlayerItemViewPort(map, @char);

                                    @char.Autorecovery();
                                    break;
                                case ObjectState.Dying:
                                    @char.State = ObjectState.Die;
                                    break;
                                //case ObjectState.Dying2:
                                //    obj.State = ObjectState.Die;
                                //    break;
                                case ObjectState.Die:
                                    @char.State = ObjectState.WaitRegen;
                                    break;
                                case ObjectState.WaitRegen:
                                    deadPlayers.Add(@char);
                                    break;
                            }
                        }
                        Friends.AddSet = false;

                        foreach (var @char in deadPlayers)
                        {
                            @char.TryRegen();
                        }

                        // update monster section
                        lock(map.Monsters)
                        foreach(var obj in map.Monsters.Where(x => x.Active))
                        {
                            switch(obj.State)
                            {
                                case ObjectState.Regen:
                                    obj.State = ObjectState.Live;
                                    obj.Spells.ClearAll();
                                    break;
                                case ObjectState.Live:
                                    obj.Spells.ClearBuffTimeOut();
                                    //obj.Update();
                                    break;
                                case ObjectState.Dying:
                                    obj.State = ObjectState.Die;
                                    break;
                                //case ObjectState.Dying2:
                                //    obj.State = ObjectState.Die;
                                //    break;
                                case ObjectState.Die:
                                    obj.State = ObjectState.WaitRegen;
                                    break;
                                case ObjectState.WaitRegen:
                                    obj.TryRegen();
                                    break;
                            }
                        }

                        // Update item section
                        lock (map.Items)
                        {
                            foreach (var it in map.Items)
                            {
                                switch (it.State)
                                {
                                    case ItemState.Creating:
                                        it.State = ItemState.Created;
                                        break;
                                    case ItemState.Created:
                                        if (it.validTime < DateTimeOffset.Now)
                                            it.State = ItemState.Deleting;
                                        break;
                                    case ItemState.Deleting:
                                        it.State = ItemState.Deleted;
                                        break;
                                }
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Logger.Error(e, DateTime.Now.ToString());
                }
            }
        }

        private static async void PlayerPlrViewport(MapInfo Map, Character plr)
        {
            var pos = plr.Position;
            pos.Offset(15, 15);

            PartyManager.SendAll(plr.Party);
            lock (plr.PlayersVP)
            {

                var playerVP = from obj in Map.Players
                               let rect = new Rectangle(obj.Position, new Size(30, 30))
                               where rect.Contains(pos) && obj.Player.Session.ID != plr.Player.Session.ID
                               select obj;

                var PShop = (from obj in playerVP
                             where obj.Shop.Open
                             select obj).ToList();

                var newPlr = (from obj in playerVP
                              where obj.State == ObjectState.Regen
                              select obj).ToList();

                var existPlr = (from obj in playerVP
                                where !plr.PlayersVP.Contains(obj.Player) && obj.State == ObjectState.Live
                                select obj).ToList();

                var deadPlr = (from obj in playerVP
                               where obj.State == ObjectState.WaitRegen
                               select obj.Player).ToList();

                var lostPlr = (from obj in plr.PlayersVP
                               where !playerVP.Contains(obj.Character)
                               select obj).ToList();

                plr.PlayersVP.AddRange(newPlr.Select(x => x.Player));
                plr.PlayersVP.AddRange(existPlr.Select(x => x.Player));

                var guildVP = newPlr
                    .Where(x => x.Guild != null)
                    .Select(x => new GuildViewPortDto
                    {
                        ID = x.Guild.Index,
                        Number = x.Index,
                        RelationShip = plr.Guild?.GetRelation(x.Guild) ?? GuildRelation.None,
                        CastleState = 0,
                        Status = x.Guild.Find(x.Name).Rank,
                        Type = x.Guild.Type,
                    }).ToList();

                guildVP.AddRange(existPlr
                    .Where(x => x.Guild != null)
                    .Select(x => new GuildViewPortDto
                    {
                        ID = x.Guild.Index,
                        Number = x.Index,
                        RelationShip = plr.Guild?.GetRelation(x.Guild) ?? GuildRelation.None,
                        CastleState = 0,
                        Status = x.Guild.Find(x.Name).Rank,
                        Type = x.Guild.Type,
                    }));

                var gensVP = newPlr
                    .Where(x => x.Gens.Influence != GensType.None)
                    .Select(x => new VPGensDto
                    {
                        Influence = x.Gens.Influence,
                        iGensClass = 0,
                        iGensRanking = 0,
                        iContributePoint = 100,
                        wzNumber = x.Index.ShufleEnding()
                    }).ToList();

                gensVP.AddRange(existPlr.Where(x => x.Gens.Influence != GensType.None)
                    .Select(x => new VPGensDto
                    {
                        Influence = x.Gens.Influence,
                        iGensClass = 1,
                        iGensRanking = 1,
                        iContributePoint = 100,
                        wzNumber = x.Index.ShufleEnding()
                    }));

                foreach (var it in deadPlr)
                    plr.PlayersVP.Remove(it);
                foreach (var it in lostPlr)
                    plr.PlayersVP.Remove(it);

            var addPlr = new List<VPCreateAbs>();
            switch(Program.Season)
            {
                case 9:
                    addPlr.AddRange(newPlr.Select(x => new VPCreateS9Dto
                    {
                        CharSet = x.Inventory.GetCharset(),
                        DirAndPkLevel = (byte)((x.Direction << 4) | 0),
                        Name = x.Name,
                        Number = (ushort)(x.Player.Session.ID | 0x8000),
                        Position = x.Position,
                        TPosition = x.TPosition,
                        ViewSkillState = x.Spells.ViewSkillStates,
                        Player = x.Player,
                        PentagramMainAttribute = 0,
                        CurLife = (uint)x.Health,
                        MaxLife = (uint)x.MaxHealth,
                        Level = x.Level,
                        MuunItem = 0xffff,
                        MuunRideItem = 0xffff,
                        MuunSubItem = 0xffff,
                        ServerCodeOfHomeWorld = 0,
                    }));
                    addPlr.AddRange(existPlr.Select(x => new VPCreateS9Dto
                    {
                        CharSet = x.Inventory.GetCharset(),
                        DirAndPkLevel = (byte)((x.Direction << 4) | 0),
                        Name = x.Name,
                        Number = (ushort)x.Player.Session.ID,
                        Position = x.Position,
                        TPosition = x.TPosition,
                        ViewSkillState = x.Spells.ViewSkillStates,
                        Player = x.Player,
                        PentagramMainAttribute = 0,
                        CurLife = (uint)x.Health,
                        MaxLife = (uint)x.MaxHealth,
                        Level = x.Level,
                        MuunItem = 0xffff,
                        MuunRideItem = 0xffff,
                        MuunSubItem = 0xffff,
                        ServerCodeOfHomeWorld = 0,
                    }));
                    break;
                default:
                    addPlr.AddRange(newPlr.Select(x => new VPCreateDto
                    {
                        CharSet = x.Inventory.GetCharset(),
                        DirAndPkLevel = (byte)((x.Direction << 4) | 0),
                        Name = x.Name,
                        Number = (ushort)(x.Player.Session.ID|0x8000),
                        Position = x.Position,
                        TPosition = x.TPosition,
                        ViewSkillState = x.Spells.ViewSkillStates,
                        Player = x.Player
                    }));
                    addPlr.AddRange(existPlr.Select(x => new VPCreateDto
                    {
                        CharSet = x.Inventory.GetCharset(),
                        DirAndPkLevel = (byte)((x.Direction << 4) | 0),
                        Name = x.Name,
                        Number = (ushort)x.Player.Session.ID,
                        Position = x.Position,
                        TPosition = x.TPosition,
                        ViewSkillState = x.Spells.ViewSkillStates,
                        Player = x.Player
                    }));
                    break;
            }

            
            switch (Program.Season)
            {
                case 9:
                    if (addPlr.Any())
                        plr.Player.Session.SendAsync(new SViewPortCreateS9 { ViewPort = addPlr.Select(x => (VPCreateS9Dto)x).ToArray() });
                    break;
                default:
                    if (addPlr.Any())
                        plr.Player.Session.SendAsync(new SViewPortCreate { ViewPort = addPlr.Select(x => (VPCreateDto)x).ToArray() });
                    break;
            }

            if (lostPlr.Any())
                plr.Player.Session.SendAsync(new SViewPortDestroy(lostPlr.Select(x => new VPDestroyDto((ushort)x.Session.ID)).ToArray()));

            if(PShop.Any())
                plr.Player.Session.SendAsync(new SViewPortPShop { VPShops = PShop.Select(x => new VPPShopDto { btName = x.Shop.Name.GetBytes(), wzNumber = x.Index.ShufleEnding() }).ToArray() });

            if (guildVP.Any())
                plr.Player.Session.SendAsync(new SGuildViewPort { Guilds = guildVP.ToArray() });

            if (gensVP.Any())
                plr.Player.Session.SendAsync(new SViewPortGens { VPGens = gensVP.ToArray() });
            }
        }

        private static async void PlayerMonsViewport(MapInfo Map, Character plr)
        {
            List<Monsters.Monster> newObj;
            List<Monsters.Monster> existObj;
            List<Monsters.Monster> deadObj;
            List<Monsters.Monster> lostObj;
            List<ushort> oldVP;
            List<ushort> missing;
            lock (Map.Monsters)
            {
                oldVP = plr.MonstersVP;
                var targetVP = Map.Monsters.Where(x => x.Active);
                //var pos = plr.Position;
                //pos.Offset(15, 15);

                var playerVP = from obj in targetVP
                               where obj.Position.Substract(plr.Position).LengthSquared() <= 10
                               select obj;

                //var playerVP = targetVP;

                newObj = (from obj in playerVP
                              where obj.State == ObjectState.Regen && obj.Active
                              select obj).ToList();

                existObj = (from obj in playerVP
                                where !oldVP.Contains(obj.Index) && obj.State == ObjectState.Live && obj.Active
                                select obj).ToList();

                deadObj = (from obj in playerVP
                           where obj.State == ObjectState.WaitRegen || obj.Active == false
                           select obj).ToList();

                lostObj = (from obj in targetVP
                           where !(obj.Position.Substract(plr.Position).LengthSquared() <= 10) && oldVP.Contains(obj.Index)
                           select obj).ToList();

                missing = (from old in oldVP
                          where !targetVP.Any(x => x.Index == old)
                          select old).ToList();
            }

            foreach (var it in deadObj)
            {
                oldVP.Remove(it.Index);
                it.ViewPort.Remove(plr.Player);
            }
            foreach (var it in lostObj)
            {
                oldVP.Remove(it.Index);
                it.ViewPort.Remove(plr.Player);
            }

            oldVP.AddRange(newObj.Select(x => x.Index));
            oldVP.AddRange(existObj.Select(x => x.Index));

            var addObj = new List<VPMCreateAbs>();

            if (newObj.Any())
            {
                switch (Program.Season)
                {
                    case 9:
                        addObj.AddRange(newObj.Select(x => new VPMCreateS9Dto
                        {
                            Number = (ushort)(x.Index | 0x8000),
                            Position = x.Position,
                            TPosition = x.TPosition,
                            Type = x.Info.Monster,
                            ViewSkillState = Array.Empty<byte>(),
                            Path = (byte)(x.Direction << 4),
                            PentagramMainAttribute = x.Element,
                            Level = x.Level,
                            Life = (uint)x.Life,
                            MaxLife = (uint)x.MaxLife,
                        }));
                        break;
                    default:
                        addObj.AddRange(newObj.Select(x => new VPMCreateDto
                        {
                            Number = (ushort)(x.Index | 0x8000),
                            Position = x.Position,
                            TPosition = x.TPosition,
                            Type = x.Info.Monster,
                            ViewSkillState = Array.Empty<byte>(),
                            Path = (byte)(x.Direction << 4),
                        }));
                        break;
                }
            }
                

            if (existObj.Any())
            {
                switch (Program.Season)
                {
                    case 9:
                        addObj.AddRange(existObj.Select(x => new VPMCreateS9Dto
                        {
                            Number = x.Index,
                            Position = x.Position,
                            TPosition = x.TPosition,
                            Type = x.Info.Monster,
                            ViewSkillState = Array.Empty<byte>(),
                            Path = (byte)(x.Direction << 4),
                            PentagramMainAttribute = x.Element,
                            Level = x.Level,
                            Life = (uint)x.Life,
                            MaxLife = (uint)x.MaxLife,
                        }));
                        break;
                    default:
                        addObj.AddRange(existObj.Select(x => new VPMCreateDto
                        {
                            Number = x.Index,
                            Position = x.Position,
                            TPosition = x.TPosition,
                            Type = x.Info.Monster,
                            ViewSkillState = Array.Empty<byte>(),
                            Path = (byte)(x.Direction << 4),
                        }));
                        break;
                }
            }

            var remObj = new List<VPDestroyDto>();
                remObj.AddRange(deadObj.Select(x => new VPDestroyDto(x.Index)));
                remObj.AddRange(lostObj.Select(x => new VPDestroyDto(x.Index)));
                remObj.AddRange(missing.Select(x => new VPDestroyDto(x)));

            if (remObj.Any())
            {
                switch(Program.Season)
                {
                    default:
                        await plr.Player.Session.SendAsync(new SViewPortDestroy(remObj.ToArray()));
                        break;
                }
            }

            if (addObj.Any())
            {
                var c = 0;
                while(c < addObj.Count)
                {
                    var send = addObj.Skip(c).Take(0xff);
                    c += 0xff;
                    switch (Program.Season)
                    {
                        case 9:
                            await plr.Player.Session.SendAsync(new SViewPortMonCreateS9 { ViewPort = send.Select(x => (VPMCreateS9Dto)x).ToArray() });
                            break;
                        default:
                            await plr.Player.Session.SendAsync(new SViewPortMonCreate { ViewPort = send.Select(x => (VPMCreateDto)x).ToArray() });
                            break;
                    }
                }                
            }
        }

        private static async void PlayerItemViewPort(MapInfo Map, Character plr)
        {
            var oldVP = plr.ItemsVP;
            var targetVP = Map.Items.ToList();
            var pos = plr.Position;
            pos.Offset(15, 15);

            var playerVP = (from obj in targetVP
                           let rect = new Rectangle(obj.Position, new Size(30, 30))
                           where rect.Contains(pos)
                           select obj).ToList();

            if (!playerVP.Any())
            {
                oldVP.Clear();
                return;
            }

            var newObj = (from obj in playerVP
                          where obj.State == ItemState.Creating
                          select obj).ToList();

            var existObj = (from obj in playerVP
                            where !oldVP.Contains(obj.Index) && obj.State == ItemState.Created
                            select obj).ToList();

            var existObjVP = (from obj in playerVP
                              where oldVP.Contains(obj.Index) && obj.State == ItemState.Created
                              select obj).ToList();

            var deadObj = (from obj in playerVP
                           where obj.State == ItemState.Deleting
                           select new VPDestroyDto(obj.Index)).ToList();

            var lostObj = (from obj in targetVP
                           let rect = new Rectangle(obj.Position, new Size(30, 30))
                           where !rect.Contains(pos) && oldVP.Contains(obj.Index) && obj.State == ItemState.Created
                           select new VPDestroyDto(obj.Index)).ToList();

            // Update the old player VP
            oldVP.AddRange(newObj.Select(x => x.Index));
            oldVP.AddRange(existObj.Select(x => x.Index));

            foreach (var it in deadObj)
                oldVP.Remove(it.Number.ShufleEnding());
            foreach (var it in lostObj)
                oldVP.Remove(it.Number.ShufleEnding());

            var addItem = new List<VPICreateDto>();
            addItem.AddRange(newObj.Select(x => new VPICreateDto { ItemInfo = x.Item.GetBytes(), wzNumber = ((ushort)(x.Index | 0x8000)).ShufleEnding(), X = (byte)x.Position.X, Y = (byte)x.Position.Y }));
            addItem.AddRange(existObj.Select(x => new VPICreateDto { ItemInfo = x.Item.GetBytes(), wzNumber = ((ushort)(x.Index | 0x0000)).ShufleEnding(), X = (byte)x.Position.X, Y = (byte)x.Position.Y }));

            if (addItem.Any())
            {
                // fix VP
                addItem.AddRange(existObjVP.Select(x => new VPICreateDto { ItemInfo = x.Item.GetBytes(), wzNumber = ((ushort)(x.Index | 0x0000)).ShufleEnding(), X = (byte)x.Position.X, Y = (byte)x.Position.Y }));
                await plr.Player.Session.SendAsync(new SViewPortItemCreate(addItem.ToArray()));
            }

            var remItem = new List<VPDestroyDto>();
            remItem.AddRange(deadObj);
            remItem.AddRange(lostObj);

            if (remItem.Any())
                await plr.Player.Session.SendAsync(new SViewPortItemDestroy { ViewPort = remItem.ToArray() });
        }

        private static void WorkerEvents()
        {
            while (true)
            {
                try
                {
                    Program.EventManager.Update();
                    Thread.Sleep(1000);
                }
                catch(Exception e)
                {
                    Logger.Error(e, DateTime.Now.ToString());
                }
            }
        }

        private static async void WorkerIA()
        {
            while (true)
            {
                Marlon.Run();
                Program.GoldenInvasionManager.Update();
                foreach (var map in ResourceCache.Instance.GetMaps().Values)
                {
                    try
                    {
                        await map.WeatherUpdate();
                    }
                    catch (Exception)
                    { }
                    lock(map.Monsters)
                    foreach (var obj in map.Monsters)
                    {
                        if (obj.State != ObjectState.Live)
                            continue;

                        obj.Update();
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static async void WorkerSavePlayers()
        {
            using (db = new GameContext())
            {
                while (true)
                {
                    try
                    {
                        Logger.Information("Saving players");
                            foreach (var plr in Program.server.Clients.Where(x => x.Player != null).Select(x => x.Player))
                            {
                                var plrLog = Logger
                                        .ForAccount(plr.Session);

                                plrLog.Information("Saving for {0}", plr.Character?.Name ?? "UNK");

                                try
                                {
                                    await plr.Save(db);
                                    plr.Character?.MuHelper.Update();
                                }catch(Exception ex)
                                {
                                    plrLog.Error(ex, "Player Save:");
                                }
                            }
                            db.SaveChanges();
                        Logger.Information("Saved players");
                            var maps = ResourceCache.Instance.GetMaps();
                            foreach(var map in maps)
                            {
                                lock (map.Value.Items)
                                {
                                    using (var transaction = db.Database.BeginTransaction())
                                    {
                                        var fordel = map.Value.Items.Where(x => x.State == ItemState.Deleted).ToList();
                                        fordel.ForEach(x =>
                                        {
                                            map.Value.Items.Remove(x);
                                            x.Item.Delete(db);
                                        });

                                        try
                                        {
                                            db.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Error(ex, "Map " + map.Key + "Save:");
                                        }
                                        transaction.Commit();
                                    }
                                }
                            }
                        Thread.Sleep(60000);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, DateTime.Now.ToString());
                    }
                }

            }
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception("Already Initialized");

            Instance = new SubSystem();

            Instance._workerDelayed.Start();
            Instance._workerViewPort.Start();
            Instance._workerEvents.Start();
            Instance._workerSavePlayers.Start();
            Instance._workerIA.Start();
        }
    }
}
