using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Network;

namespace WebZen.Util
{
    public class CommandEventArgs : EventArgs
    {
        public string Argument { get; set; }
    }
    public class Command<T>
    {
        private Predicate<T> _autority;
        private string _command;
        private List<Command<T>> _subCommands;
        private bool _partial;

        public event EventHandler<CommandEventArgs> OnCommand;

        public Command(string cmd, EventHandler<CommandEventArgs> _event = null, Predicate<T> autority = null)
        {
            if(string.IsNullOrEmpty(cmd))
            {
                throw new ArgumentNullException("cmd");
            }

            _autority = autority ?? (x => true);
            _subCommands = new List<Command<T>>();
            OnCommand = _event;
            _command = cmd;
            _partial = false;
        }

        public Command<T> AddCommand(Command<T> cmd)
        {
            _subCommands.Add(cmd);
            return this;
        }

        public bool Process(T client, string text)
        {
            var splited = text.Split(" ");
            var cmd = splited.First();
            var args = string.Join(" ", splited.Skip(1));

            if (_partial)
            {
                var substring = cmd.Substring(0, _command.Length);
                if (!substring.Equals(_command, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                args = text.Substring(_command.Length);
            }
            else
            {
                if (!cmd.Equals(_command, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if (client != null && !_autority(client))
                throw new Exception("No tienes acceso suficiente");

            if (_subCommands.Count == 0)
                OnCommand(client, new CommandEventArgs { Argument = args });
            else
            {
                foreach (var scmd in _subCommands)
                {
                    if (scmd.Process(client, args))
                    {
                        return true;
                    }
                }
            }

            return true;
        }

        public Command<T> SetPartial()
        {
            _partial = true;
            return this;
        }
    }
}
