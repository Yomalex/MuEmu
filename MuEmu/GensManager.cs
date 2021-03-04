using MU.DataBase;
using MuEmu.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public enum Gens
    {
        None,
        Duprian,
        Vanert,
    }
    public class GensManager
    {
        private static Dictionary<Gens, int> _contribution;
        public static void Initialize()
        {
            _contribution = new Dictionary<Gens, int>();
            /*using(var db = new GameContext())
            {

            }*/
        }

        public GensManager(CharacterDto dto)
        {

        }

        public void NPCTalk(Player player)
        {

        }
    }
}
