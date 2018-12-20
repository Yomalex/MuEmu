using System;
using System.Collections.Generic;
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

        public CommandHandler<TSession> ProcessCommands(TSession source, string text)
        {
            foreach(var cmd in _commands)
            {
                if(cmd.Process(source, text))
                {
                    return this;
                }
            }
            return this;
        }
    }
}
