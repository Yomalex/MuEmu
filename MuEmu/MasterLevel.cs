using MU.DataBase;
using MuEmu.Entity;
using MU.Network.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using MU.Resources;

namespace MuEmu
{
    public class MasterLevel
    {
        private bool _needSave;
        private ushort _level;
        private ulong _experience;
        private ushort _points;
        private bool _new;

        public ushort Level
        {
            get => _level;
            set
            {
                _level = value;
                _needSave = true;
            }
        }
        public ulong Experience
        {
            get => _experience; set
            {
                _experience = value;
                _needSave = true;
            }
        }
        public ulong NextExperience => GetExperienceFromLevel((ushort)(Level + Character.Level-1));
        public ushort Points
        {
            get => _points; set
            {
                _needSave = true;
                _points = value;
            }
        }
        public Character Character { get; private set; }
        public MasterLevel(Character @char, CharacterDto @charDto)
        {
            Character = @char;
            _new = @charDto.MasterInfo == null;
            Level = @charDto.MasterInfo?.Level ?? 1;
            Experience = @charDto.MasterInfo?.Experience ?? 0;
            Points = @charDto.MasterInfo?.Points ?? 0;
        }

        public void GetExperience(ulong exp)
        {
            if(!Character.MasterClass || Character.Level != 400)
            {
                return;
            }

            Experience += exp;
            var level = Level;
            while(Experience >= NextExperience)
            {
                Level++;
            }

            if(level != Level)
            {
                var LevelAdd = Level - level;
                var levelPoint = Character.BaseClass == HeroClass.MagicGladiator || Character.BaseClass == HeroClass.DarkLord ? 7 : 5;

                Points += (ushort)(levelPoint*LevelAdd);
                if (Points > 200)
                    Points = 200;

                Character.CalcStats();
                Character.Player.Session
                    .SendAsync(new SMasterLevelUp(Level, (ushort)LevelAdd, Points, maxPoints:(ushort)200, (ushort)Character.MaxHealth, (ushort)Character.MaxShield, (ushort)Character.MaxMana, (ushort)Character.MaxStamina)).Wait();

                Character.Player.Session.SendAsync(new SEffect(Character.Index, ClientEffect.LevelUp)).Wait();
            }
        }

        private ulong GetExperienceFromLevel(ushort level)
        {
            var exp = (((level + 9ul) * level) * level) * 10ul + ((level>255)?(((((ulong)(level - 255) + 9ul) * (level - 255ul)) * (level - 255ul)) * 1000ul):0ul);
            if (level >= 400)
            {
                exp -= 3892250000;
                exp /= 2;
            }
            if (level > 600)
            {
                var Level3 = (double)((level - 600) * (level - 600));
                exp = (ulong)(exp * (1 + (Level3 * 1.2) / 100000.0));
            }
            return exp;
        }

        public void SendInfo()
        {
            if (Character.MasterClass)
            {
                Character.Player.Session.SendAsync(new SMasterInfo((ushort)(Level + Character.Level - 1), Character.Level >= 400 ? Experience : Character.Experience, NextExperience, Points, (ushort)Character.MaxHealth, (ushort)Character.MaxShield, (ushort)Character.MaxMana, (ushort)Character.MaxStamina)).Wait();
            }
        }

        public void Save(GameContext db)
        {
            if (!_needSave || !Character.MasterClass)
                return;

            if (_new)
            {
                db.MasterLevel.Add(new MasterInfoDto
                {
                    MasterInfoId = Character.Id,
                    Experience = Experience,
                    Level = Level,
                    Points = Points,
                });
                _new = false;
                return;
            }

            db.MasterLevel.Update(new MasterInfoDto
            {
                MasterInfoId = Character.Id,
                Experience = Experience,
                Level = Level,
                Points = Points,
            });
        }
    }
}
