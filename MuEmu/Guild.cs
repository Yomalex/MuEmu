using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network.Guild;
using MuEmu.Network.Data;
using Serilog;
using Serilog.Core;
using MuEmu.Entity;
using MU.DataBase;
using MuEmu.Network.Game;

namespace MuEmu
{
    public class GuildManager
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GuildManager));
        public static GuildManager Instance { get; set; }

        public Dictionary<int,Guild> Guilds { get; set; }

        GuildManager()
        {
            Guilds = new Dictionary<int, Guild>();
            using (var game = new GameContext())
            {
                foreach(var guild in game.Guilds)
                {
                    Guilds.Add(guild.GuildId, new Guild(guild));
                }
            }
        }

        public static void Initialize()
        {
            if (Instance != null)
                return;

            Instance = new GuildManager();

            Logger.Information("Initializing Guild System");
        }

        public static void CreateGuild(Player master, string name, byte[] Mark, byte Type)
        {
            name.Trim();
            //if (name.Length < 2 || name.Length >= 8)
            //{
            //    master.Session.SendAsync(new SGuildCreateResult { Result = 2 });
            //    Logger.Error("Guild name long or short (4-8):{0} Master:{1}", name, master.Character.Name);
            //    return;
            //}

            if (Instance.Guilds.Any( x => x.Value.Name.ToLower() == name.ToLower()))
            {
                master.Session.SendAsync(new SGuildCreateResult { Result = 0, GuildType = Type });
                Logger.Error("Guild name in use:{0} Master:{1}", name, master.Character.Name);
                return;
            }

            var g = new Guild(name, Mark, Type);
            Instance.Guilds.Add(g.Index,g);
            master.Session.SendAsync(new SGuildCreateResult { Result = 1, GuildType = Type });
            g.Add(master, 0);
            Logger.Information("New guild added:{0} Master:{1}", name, master.Character.Name);
        }

        public static Guild Get(int guildID)
        {
            if(Instance.Guilds.ContainsKey(guildID))
                return Instance.Guilds[guildID];

            return null;
        }

        public void AddPlayer(Player plr)
        {
            var Guild = (from g in Guilds.Values
                        from m in g.Members
                        where m.Name == plr.Character.Name
                        select g).FirstOrDefault();

            Guild?.ConnectMember(plr);
        }
        public void RemovePlayer(Player plr)
        {
            var Guild = (from g in Guilds.Values
                         from m in g.Members
                         where m.Name == plr.Character.Name
                         select g).FirstOrDefault();

            Guild?.Remove(plr);
        }

        public GuildMember FindCharacter(string @char)
        {
            foreach(var guild in Guilds)
            {
                var ret = guild.Value.Find(@char);
                if (ret != null)
                    return ret;
            }

            return null;
        }

        public static void NPCTalk(Player plr)
        {
            plr.Session.SendAsync(new SGuildMasterQuestion());
        }

        public static void SendList(Player plr)
        {
            var guild = Instance.Guilds.Values.Where(x => x.Members.Any(y => y.Player == plr)).FirstOrDefault();
            var pMsg = new SGuildList();
            if (guild == null)
            {
                Logger.Error("NO GUILD");
                plr.Session.SendAsync(pMsg);
                return;
            }

            pMsg.Result = 1;
            pMsg.Members = guild.ActiveMembers.Select(x => new GuildListDto
            {
                Name = x.Name,
                ConnectAServer = 0x80,
                Number = 0,
                btGuildStatus = 1,
            }).ToArray();
            pMsg.Count = (byte)pMsg.Members.Length;

            plr.Session.SendAsync(pMsg);
            Logger.Debug("Player List:{0}", string.Join(", ", guild.ActiveMembers.Select(x => x.Name)));
        }
    }

    public class Guild
    {
        public GuildManager Guilds => GuildManager.Instance;

        public int Index { get; private set; }

        public string Name { get; private set; }
        public byte[] Mark { get; private set; }
        public byte Type { get; private set; }
        public GuildMember Master => Members.First(x => x.Rank == 0);
        public List<GuildMember> Members { get; set; }

        public List<GuildMember> ActiveMembers => Members.Where(x => x.Player != null).ToList();

        public Guild(string name, byte[] mark, byte type)
        {
            Members = new List<GuildMember>();
            Name = name;
            Mark = mark;

            using (var game = new GameContext())
            {
                var guild = new MU.DataBase.GuildDto
                {
                    Name = name,
                    Mark = mark,
                    GuildType = type,
                };
                game.Add(guild);
                game.SaveChanges();

                Index = guild.GuildId;
            }
        }

        public Guild(GuildDto guildDto)
        {
            Index = guildDto.GuildId;
            Name = guildDto.Name;
            Mark = guildDto.Mark;
            Type = guildDto.GuildType;

            using (var game = new GameContext())
            {
                Members = (from memb in game.GuildMembers
                           join @char in game.Characters on memb.MembId equals @char.CharacterId
                           where memb.GuildId == Index
                           select new GuildMember(this, @char.Name, (GuildStatus)memb.Rank)).ToList();
            }
        }

        public GuildMember Add(Player plr, GuildStatus rank)
        {
            var memb = Members.Where(x => x.Name == plr.Character.Name).FirstOrDefault();
            if(memb == null)
            {
                memb = new GuildMember(this, plr, rank);
                Members.Add(memb);
            }else
            {
                memb.Player = plr;
            }

            var status = ((Master?.Player??plr) == plr ? 0x80 : 0x00);

            plr.Session.SendAsync(new SGuildViewPort
            {
                Guilds = new Network.Data.GuildViewPortDto[] {
                    new Network.Data.GuildViewPortDto {
                        ID = Index,
                        Type = Type,
                        Status = (byte)status,
                        RelationShip = 0,
                        Number = plr.Session.ID | status
                    } }
            });

            return memb;
        }

        public GuildMember Find(string name)
        {
            return Members.FirstOrDefault(x => x.Name == name);
        }

        public void ConnectMember(Player plr)
        {
            Members.First(x => x.Name == plr.Character.Name).Player = plr;

            var notice = new SNotice(NoticeType.Guild, $"Welcome back {plr.Character.Name}");

            foreach(var memb in Members.Where(x => x.Player != null))
            {
                memb.Player.Session.SendAsync(notice);
            }
        }

        public void Remove(Player plr)
        {
            var memb = Members
                .Where(x => x.Name == plr.Character.Name)
                .FirstOrDefault();

            if (memb == null)
                throw new Exception("GUILD: Try to remove an Invalid Member");
        }
    }

    public class GuildMember
    {
        public Guild Guild { get; private set; }

        public Player Player { get; set; }

        public string Name { get; }

        public GuildStatus Rank { get; }

        public GuildMember()
        {

        }

        public GuildMember(Guild guild, Player plr, GuildStatus rank)
        {
            Name = plr.Character.Name;
            Player = plr;
            Rank = rank;
            Guild = guild;

            using (var game = new GameContext())
            {
                var guildMember = new MU.DataBase.GuildMemberDto
                {
                    GuildId = guild.Index,
                    MembId = plr.Character.Id,
                    Rank = (byte)rank,
                };
                game.Add(guildMember);
                game.SaveChanges();
            }
        }

        public GuildMember(Guild guild, string name, GuildStatus rank)
        {
            Name = name;
            Rank = rank;
            Guild = guild;
        }
    }
}
