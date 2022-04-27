using MU.DataBase;
using MU.Resources;
using MuEmu.Entity;
using MuEmu.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    internal class HuntingRecord
    {
        public HuntingDto Hunting { get; set; }
        public Character Character { get; }
        public bool Active { get; set; }

        public HuntingRecord(Character character, CharacterDto characterDto)
        {
            Character = character;
        }

        internal void GainExperience(long gain)
        {
            Hunting.Experience += gain;
        }

        internal void AttackPVM(float attack)
        {
            Hunting.AttackPVM += (long)attack;
        }

        internal void KilledMonster(Monster monster)
        {
            Hunting.KilledMonsters++;
        }

        internal void HealingUse(long addLife)
        {
            Hunting.HealingUse += addLife;
        }

        internal void ElementalAttackPVM(float dmg)
        {
            Hunting.ElementalAttackPVM += (long)dmg;
        }

        internal void Start()
        {
            Hunting = new HuntingDto();
            Hunting.CharacterId = Character.Id;
            Hunting.Character.Map = (ushort)Character.MapID;
            Hunting.Level = Character.Level;
            Hunting.DateTime = DateTime.Now;
        }

        internal void Save()
        {
            using(var db = new GameContext())
            {
                db.HuntingRecords.Add(Hunting);
                db.SaveChanges();
            }
            Hunting = null;
        }

        internal List<HuntingDto> GetRecordList(Maps map)
        {
            using (var db = new GameContext())
            {
                return db.HuntingRecords
                    .Where(x => x.CharacterId == Character.Id && x.Map == (ushort)map)
                    .ToList();
            }
        }
    }
}
