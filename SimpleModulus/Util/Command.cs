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
        private Command<T> _parent;
        private Predicate<T> _autority;
        private string _command;
        private List<Command<T>> _subCommands;
        private bool _partial;
        private string _help;

        public event EventHandler<CommandEventArgs> OnCommand;

        public Command(string cmd, EventHandler<CommandEventArgs> _event = null, Predicate<T> autority = null, string help = "")
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
            _help = help;
        }

        public Command<T> AddCommand(Command<T> cmd)
        {
            _subCommands.Add(cmd);
            cmd._parent = this;
            return this;
        }

        public Command<T> Find(T client, string text)
        {
            var splited = text.Split(" ");
            var cmd = splited.First();
            var args = string.Join(" ", splited.Skip(1));
            if (_partial)
            {
                var substring = cmd.Substring(0, _command.Length);
                if (!substring.Equals(_command, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                args = text.Substring(_command.Length);
            }
            else
            {
                if (!cmd.Equals(_command, StringComparison.InvariantCultureIgnoreCase))
                    return null;
            }

            if (client != null && !_autority(client))
                return null;

            if (_subCommands.Count == 0 || string.IsNullOrWhiteSpace(args))
                return this;
            else
            {
                foreach (var scmd in _subCommands)
                {
                    var res = scmd.Find(client, args);
                    if (res == null)
                        continue;

                    return res;
                }
            }

            return null;
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

        public string Help()
        {
            var output = FullName();

            if (!_partial)
                output += ":" + _help;

            if(_subCommands.Count > 0)
            {
                output += "\n\tSubCommand List:\n\t\t";
                output += string.Join("\n\t\t", _subCommands.Select(x => x.FullName()));
            }

            return output;
        }

        public string FullName()
        {
            var output = "";
            if (_parent != null)
            {
                output = _parent.FullName();
                if (!_parent._partial)
                    output += " ";
            }

            output += _command;

            return output;
        }

        public string Name => _command;
        public bool IsPartial => _partial;

        public List<Command<T>> GetCommandList()
        {
            var a = (from c in _subCommands
                     where !c.IsPartial
                     select c).ToList();

            var b = _subCommands
                .Where(x => x.IsPartial)
                .SelectMany(x => x.GetCommandList());

            a.AddRange(b);
            return a;
        }
    }
}
