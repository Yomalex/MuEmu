using MU.DataBase;
using MU.Resources;
using MuEmu.Entity;
using MU.Network.Game;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using MU.Network.GensSystem;

namespace MuEmu
{
    public class Gens
    {
        private static Dictionary<GensType, int> _contribution;
        private static readonly Dictionary<int, int> _classSuperior = new Dictionary<int, int>
        {
            { 1, 1 },
            { 5, 2 },
            { 10, 3 },
            { 30, 4 },
            { 50, 5 },
            { 100, 6 },
            { 200, 7 },
            { 300, 8 },
            { 9999, 9 },
        };
        private static readonly Dictionary<int, int> _class = new Dictionary<int, int>
        {
            { 6000, 10 },
            { 3000, 11 },
            { 1500, 12 },
            { 500, 13 },
            { 0, 14 }
        };
        private Player player;
        public int Contribution { get; set; }
        public GensType Influence { get; internal set; }
        public int Ranking { get; internal set; }
        public int Class { get; internal set; }

        public static void Initialize()
        {
            _contribution = new Dictionary<GensType, int>();
            /*using(var db = new GameContext())
            {

            }*/
        }
        public static void Update()
        {
            
        }

        public Gens(Character @char, CharacterDto dto)
        {
            player = @char.Player;
            if(dto.Gens == null)
            {
                Influence = GensType.None;
                Class = 14;
                Ranking = 9999;
                Contribution = 0;
                return;
            }
            Influence = (GensType)dto.Gens.Influence;
            Ranking = dto.Gens.Ranking;
            Class = dto.Gens.Class;
            Contribution = dto.Gens.Contribution;
        }

        internal void SendMemberInfo()
        {
            var nextCP = 0;
            if (Contribution < 9999)
            {
                if (Class < 9)
                    Class = 14;

                nextCP = _class.First(x => x.Value == Class - 1).Key;
            }else
            {
                nextCP = 10000;
            }

            player.Session.SendAsync(new SGensSendInfoS9
            {
                Class = Class,
                ContributePoint = Contribution,
                Influence = Influence,
                NextContributePoint = nextCP,
                Ranking = Ranking
            }).Wait();
        }

        public void NPCTalk(ushort npc)
        {
            var obj = new SNPCDialog
            {
                Contribution = (uint)Contribution,
                NPC = npc,
            };

            player.Session.SendAsync(obj).Wait();
        }

        internal void Join(GensType influence)
        {
            Influence = influence;
            SendMemberInfo();
        }
        internal void Leave()
        {
            Influence = GensType.None;
            Contribution = 0;
            Ranking = 9999;
            SendMemberInfo();
        }
    }
}
