using MU.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuEmu
{
    public class Quests
    {
        private List<Quest> _quests;
        public Quests(Character @char, CharacterDto characterDto)
        {
            _quests = new List<Quest>();
        }
    }
}
