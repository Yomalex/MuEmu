using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MU.Network.Guild;
using MuEmu.Network.Data;
using Serilog;
using Serilog.Core;
using MuEmu.Entity;
using MU.DataBase;
using MU.Network.Game;
using MuEmu.Util;
using MuEmu.Monsters;
using MU.Resources;
using WebZen.Util;
using MU.Network;

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
                foreach(var guild in game.Guilds.ToList())
                {
                    //guild.MembersInfo = game.GuildMembers.Where(x => x.GuildId == guild.GuildId).ToList();
                    Guilds.Add(guild.GuildId, new Guild(guild));
                }
                // Solve references
                foreach(var guild in game.Guilds.ToList())
                {
                    if (guild.AllianceId != null)
                    {
                        var masterAlliance = Guilds[(int)guild.AllianceId];
                        masterAlliance.Union.Add(Guilds[guild.GuildId]);
                        Guilds[guild.GuildId].Union = masterAlliance.Union;
                    }
                    for(var i =0; i < 5; i++)
                    {
                        var r = guild.Get("Rival" + (i + 1)) as int?;
                        if (r != null)
                        {
                            Guilds[guild.GuildId].Rival.Add(Guilds[(int)r]);
                        }
                    }
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
                _=master.Session.SendAsync(new SGuildCreateResult { Result = 0, GuildType = Type });
                Logger.Error("Guild name in use:{0} Master:{1}", name, master.Character.Name);
                return;
            }

            var g = new Guild(name, Mark, Type);
            Instance.Guilds.Add(g.Index,g);
            master.Session.SendAsync(new SGuildCreateResult { Result = 1, GuildType = Type }).Wait();
            g.Add(master, GuildStatus.GuildMaster);
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

        public static void NPCTalk(Player plr, Monster npc)
        {
            if(plr.Character.Guild != null)
            {
                plr.Session.SendAsync(new SChatTarget(npc.Index, ""))
                    .Wait();
                return;
            }
            plr.Session.SendAsync(new SGuildMasterQuestion())
                .Wait();
        }

        public static void SendList(Player plr)
        {
            var guild = Instance.Guilds.Values.Where(x => x.Members.Any(y => y.Player == plr)).FirstOrDefault();
            object pMsg;

            if (guild == null)
            {
                pMsg = VersionSelector.CreateMessage<SGuildList>((byte)0);
                
                plr.Session.SendAsync(pMsg).Wait();
                return;
            }

            var members = guild.Members
                .OrderBy(x => x.Name)
                .OrderByDescending(x => x.Rank)
                .Select((x,i) => new GuildListDto
            {
                Name = x.Name,
                ConnectAServer = (byte)(x.Server != 0xff?(0x80| x.Server): 0x00),
                Number = (byte)i,
                btGuildStatus = x.Rank,
            });

            pMsg = VersionSelector.CreateMessage<SGuildList>((byte)1, guild.Score, guild.TotalScore, members.ToList(), guild.Rival.Select(x => x.Name).ToList());
            
            plr.Session.SendAsync(pMsg).Wait();
        }

        internal static Guild Get(string v)
        {
            return Instance.Guilds.First(x => x.Value.Name == v).Value;
        }
    }

    public class Guild
    {
        public GuildManager Guilds => GuildManager.Instance;

        public int Index { get; private set; }

        public string Name { get; private set; }
        public byte[] Mark { get; private set; }
        public byte Type { get; private set; }

        public List<Guild> Union { get; set; } = new List<Guild>();
        public List<Guild> Rival { get; set; } = new List<Guild>();

        public GuildMember Master => Members.First(x => x.Rank == GuildStatus.GuildMaster);
        public GuildMember Assistant => Members.FirstOrDefault(x => x.Rank == GuildStatus.Assistant);
        public IEnumerable<GuildMember> BattleMasters => Members.Where(x => x.Rank == GuildStatus.BattleMaster);
        public List<GuildMember> Members { get; set; }

        public List<GuildMember> ActiveMembers => Members.Where(x => x.Server != 0xff).ToList();

        public byte Score { get; internal set; }
        public int TotalScore { get; internal set; }
        public bool IsUnionMaster => (Union.Count > 1) && (Union[0] == this);

        public Guild(string name, byte[] mark, byte type)
        {
            Members = new List<GuildMember>();
            Name = name;
            Mark = mark;

            using (var game = new GameContext())
            {
                var guild = new GuildDto
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
            Members = guildDto.MembersInfo.Select(x => new GuildMember(this, x.Memb.Name, (GuildStatus)x.Rank) { Server = 0xff }).ToList();
            Type = guildDto.GuildType;
            
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

            memb.Server = (byte)Program.ServerCode;
            plr.Character.Guild = this;
            memb.ViewPort();

            return memb;
        }

        public GuildMember Find(string name)
        {
            return Members.FirstOrDefault(x => x.Name == name);
        }

        public void ConnectMember(Player plr)
        {
            var conMemb = Find(plr.Character.Name);
            conMemb.Player = plr;
            conMemb.ViewPort();
            conMemb.Server = (byte)Program.ServerCode;
            plr.Character.Guild = this;

            var notice = new SNotice(NoticeType.Guild, $"Welcome back {plr.Character.Name}");

            ActiveMembers
                .Where(x => x.Player != null)
                .Select(x => x.Player.Session)
                .SendAsync(notice)
                .Wait();
        }

        public void Remove(Player plr)
        {
            var memb = Members
                .Where(x => x.Name == plr.Character.Name)
                .FirstOrDefault();

            Remove(memb);
        }

        public void Remove(GuildMember memb)
        {
            if (memb == null)
                throw new Exception("GUILD: Try to remove an Invalid Member");

            Members.Remove(memb);
            using (var game = new GameContext())
            {
                var charId = (from row in game.Characters where row.Name == memb.Name select row.CharacterId).FirstOrDefault();
                var dto = (from row in game.GuildMembers where row.MembId == charId select row).FirstOrDefault();

                game.Remove(dto);
                game.SaveChanges();
            }
        }

        public GuildRelation GetRelation(Guild guild)
        {
            if (Union.Contains(guild))
                return GuildRelation.Union;

            if (Rival.Contains(guild))
                return GuildRelation.Rival;

            return GuildRelation.None;
        }

        internal bool CanAdd()
        {
            var max = Master.Player.Character.Level / 10;
            if(Master.Player.Character.BaseClass == HeroClass.DarkLord)
            {
                max += Master.Player.Character.CommandTotal / 10;
            }

            if(max > 100)
            {
                max = 100;
            }

            return max > Members.Count();
        }

        internal void ChangeRelation(Guild guild, GuildUnionRequestType requestType, GuildRelationShipType relationShipType)
        {
            switch(requestType)
            {
                case GuildUnionRequestType.Join:
                    switch(relationShipType)
                    {
                        case GuildRelationShipType.Union:
                            if (Union.Count == 0)
                                Union.Add(this);

                            Union.Add(guild);
                            guild.Union = Union;
                            break;
                        case GuildRelationShipType.Rivals:
                            Rival.Add(guild);
                            guild.Rival.Add(this);
                            break;
                    }
                    break;
                case GuildUnionRequestType.BreakOff:
                    switch (relationShipType)
                    {
                        case GuildRelationShipType.Union:
                            if (this == guild)
                            {
                                Union[0].ChangeRelation(this, requestType, relationShipType);
                                return;
                            }
                            Union.Remove(guild);
                            if (Union.Count == 1)
                            {
                                Union.Remove(this);
                            }

                            guild.Union = new List<Guild>();
                            break;
                        case GuildRelationShipType.Rivals:
                            Rival.Remove(guild);
                            guild.Rival.Remove(this);
                            break;
                    }
                    break;
            }

            using (var game = new GameContext())
            {
                var guildDtoA = (from g in game.Guilds
                                 where g.GuildId == Index
                                 select g).Single();

                var guildDtoB = (from g in game.Guilds
                                 where g.GuildId == guild.Index
                                 select g).Single();

                guildDtoA.AllianceId = Union.FirstOrDefault()?.Index ?? null;
                guildDtoB.AllianceId = Union.FirstOrDefault()?.Index ?? null;
                for(var i = 0; i < 5; i++)
                {
                    var rivA = Rival.Count > i ? (int?)Rival[i].Index : null;
                    var rivB = guild.Rival.Count > i ? (int?)guild.Rival[i].Index : null;
                    guildDtoA.Set("Rival" + (i + 1), rivA);
                    guildDtoB.Set("Rival" + (i + 1), rivB);
                }

                game.Update(guildDtoA);
                game.Update(guildDtoB);
                game.SaveChanges();
            }
        }
    }

    public class GuildMember
    {
        public Guild Guild { get; private set; }

        public Player Player { get; set; }

        public string Name { get; }

        public byte Server { get; set; }

        public GuildStatus Rank { get; private set; }

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
                    Rank = (int)rank,
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

        public void ViewPort()
        {
            var vp = new SGuildViewPort
            {
                Guilds = new GuildViewPortDto[] { new GuildViewPortDto {
                    ID = Guild.Index,
                    Number = (ushort)(Player.ID /*| (Rank == GuildStatus.GuildMaster ? 0x80 : 0x00)*/),
                    RelationShip = GuildRelation.None,
                    CastleState = 0,
                    Status = Rank,
                    Type = Guild.Type,
                } }
            };

            Player.SendV2Message(vp);
            Player.Session.SendAsync(vp).Wait();
        }

        public void UpdateRank(GuildStatus newStatus)
        {
            if (Rank == newStatus)
                return;

            using (var game = new GameContext())
            {
                var charId = (from row in game.Characters where row.Name == Name select row.CharacterId).FirstOrDefault();
                var guildMember = (from row in game.GuildMembers where row.MembId == charId select row).FirstOrDefault();
                guildMember.Rank = (int)Rank;
                game.Update(guildMember);
                game.SaveChanges();
            }

            Rank = newStatus;
        }
    }
}
