using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Network;

namespace WebZen.Util
{
    public class CommandHandler<TSession>
        where TSession : WZClient
    {
        private List<Command<TSession>> _commands;

        public CommandHandler()
        {
            _commands = new List<Command<TSession>>();
        }

        public CommandHandler<TSession> AddCommand(Command<TSession> command)
        {
            _commands.Add(command);
            return this;
        }

        public bool ProcessCommands(TSession source, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach(var cmd in _commands)
            {
                if(cmd.Process(source, text))
                {
                    return true;
                }
            }
            return false;
        }

        public Command<TSession> FindCommand(TSession source, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            foreach (var cmd in _commands)
            {
                var res = cmd.Find(source, text);

                if (res != null)
                {
                    return res;
                }
            }

            return null;
        }

        public List<Command<TSession>> GetCommandList()
        {
            var a = (from c in _commands
                    where !c.IsPartial
                    select c).ToList();

            var b = _commands
                .Where(x => x.IsPartial)
                .SelectMany(x => x.GetCommandList());

            a.AddRange(b);

            return a;
        }
    }
}
