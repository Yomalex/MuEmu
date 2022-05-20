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
            Hunting = new HuntingDto();
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
            using (var db = new GameContext())
            {
                Hunting = (from hr in db.HuntingRecords
                          where 
                          hr.CharacterId == Character.Id && 
                          hr.DateTime.Date == DateTime.Now.Date &&
                          hr.Map == (ushort)Character.MapID
                           select hr).SingleOrDefault();

                if(Hunting == null)
                {
                    Hunting = new HuntingDto();
                    Hunting.CharacterId = Character.Id;
                    Hunting.Map = (ushort)Character.MapID;
                    Hunting.Level = Character.Level;
                    Hunting.DateTime = DateTime.Now;
                }
            }            
        }

        internal void Save()
        {
            using(var db = new GameContext())
            {
                Hunting.Duration = (int)(DateTime.Now - Hunting.DateTime).TotalSeconds;

                if (Hunting.Id == 0)
                    db.HuntingRecords.Add(Hunting);
                else
                    db.HuntingRecords.Update(Hunting);

                db.SaveChanges();
            }
        }

        internal Dictionary<int, HuntingDto> GetRecordList(Maps map)
        {
            var id = 1;
            using (var db = new GameContext())
            {
                return db.HuntingRecords
                    .Where(x => x.CharacterId == Character.Id && x.Map == (ushort)map)
                    .ToDictionary(x => id++);
            }
        }
    }
}
