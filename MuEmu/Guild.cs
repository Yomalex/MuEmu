using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MuEmu.Network.Guild;

namespace MuEmu
{
    public class GuildManager
    {
        public static GuildManager Instance { get; set; }

        public List<Guild> Guilds { get; set; }

        public static void Initialize()
        {
            if (GuildManager.Instance != null)
                return;

            Instance = new GuildManager();
        }

        public void CreateGuild()
        {

        }

        public void AddPlayer(Player plr)
        {
            var Guild = (from g in Guilds
                        from m in g.Members
                        where m.Name == plr.Character.Name
                        select g).FirstOrDefault();

            Guild?.Add(plr);
        }
        public void RemovePlayer(Player plr)
        {
            var Guild = (from g in Guilds
                         from m in g.Members
                         where m.Name == plr.Character.Name
                         select g).FirstOrDefault();

            Guild?.Remove(plr);
        }
    }

    public class Guild
    {
        public GuildManager Guilds => GuildManager.Instance;

        public int Index { get; set; }

        public GuildMember Master { get; set; }
        public List<GuildMember> Members { get; set; }

        public List<GuildMember> ActiveMembers => Members.Where(x => x.Player != null).ToList();

        public void Add(Player plr)
        {
            var memb = Members.Where(x => x.Name == plr.Character.Name).FirstOrDefault();
            if(memb == null)
            {
                Members.Add(new GuildMember(plr));
            }else
            {
                memb.Player = plr;
            }

            plr.Session.SendAsync(new SGuildViewPort
            {
                Guilds = new Network.Data.GuildViewPortDto[] {
                    new Network.Data.GuildViewPortDto {
                        ID = Index,
                        Type = 0,
                        Status = 0,
                        RelationShip = 0,
                        Number = plr.Session.ID
                    } }
            });
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
        public Player Player { get; set; }

        public string Name { get; }

        public GuildMember()
        {

        }

        public GuildMember(Player plr)
        {

        }
    }
}
