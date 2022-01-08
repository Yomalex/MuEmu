using MU.Network.Game;
using MU.Resources;
using MU.Resources.XML;
using MuEmu.Resources;
using MuEmu.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu.Monsters
{
    internal class PatternManager
    {
        private List<List<MonsterSpell>> _patterns = new List<List<MonsterSpell>>();
        private DateTime _nextSkill = DateTime.MinValue;
        public PatternManager(Monster monster, string pattern)
        {
            Monster = monster;
            Monster.CanGoToBattleState = false;
            var xml = ResourceLoader.XmlLoader<PatternsDto>(pattern);
            foreach(var x in xml.Pattern.OrderBy(x => x.Number))
            {
                _patterns.Add(x.Skill.ToList());
            }
        }

        public Monster Monster { get; }
        public event EventHandler<UseSkillEventArgs> UseSkill;

        public void Update()
        {
            if (Monster.Active == false || _nextSkill > DateTime.Now)
                return;

            var currPatter = (int)((1.0f - Monster.Life / Monster.MaxLife) * _patterns.Count);
            var patter = _patterns[currPatter];
            var randomSkill = Program.RandomProvider(patter.Count);
            var skill = patter[randomSkill];

            UseSkill?.Invoke(Monster, new UseSkillEventArgs { Spell = skill });
            _ = Monster.ViewPort.Select(x => x.Session).SendAsync(new SMonsterSkillS9Eng
            {
                MonsterSkillNumber = (ushort)skill,
                ObjIndex = Monster.Index,
                TargetObjIndex = Monster.Target?.ID ?? 0xffff
            });

            var maxDelay = (int)MathF.Max(10000.0f * Monster.Life / Monster.MaxLife, 3000.0f);
            var delay = Program.RandomProvider(maxDelay, 300);
            _nextSkill = DateTime.Now.AddMilliseconds(delay);
        }
    }

    public class UseSkillEventArgs
    {
        public MonsterSpell Spell { get; set; }
    }
}
