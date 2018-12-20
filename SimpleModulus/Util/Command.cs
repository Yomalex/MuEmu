using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebZen.Network;

namespace WebZen.Util
{
    public class Command<T>
    {
        private Predicate<T> _autority;
        private string _command;
        private List<Command<T>> _subCommands;

        public event EventHandler OnCommand;

        public Command(string cmd, EventHandler _event, Predicate<T> autority = null)
        {
            if(string.IsNullOrEmpty(cmd))
            {
                throw new ArgumentNullException("cmd");
            }

            _autority = autority ?? (x => true);
            _subCommands = new List<Command<T>>();
            OnCommand = _event;
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
            if (cmd.Equals(_command, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (!_autority(client))
                throw new Exception("No tienes acceso suficiente");

            if (_subCommands.Count == 0)
                OnCommand(client, new EventArgs());
            else
            {
                foreach (var scmd in _subCommands)
                {
                    if (scmd.Process(client, text))
                    {
                        return true;
                    }
                }
            }

            return true;
        }
    }
}
