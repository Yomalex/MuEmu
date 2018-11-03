using BlubLib.Collections.Generic;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Network;

namespace CSEmu.Network
{
    public class FilteredMessageHandler<TSession> : MessageHandler
        where TSession : WZClient
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(FilteredMessageHandler<TSession>));
        private readonly IDictionary<Type, List<Predicate<TSession>>> _filter = new Dictionary<Type, List<Predicate<TSession>>>();
        private readonly IList<IMessageHandler> _messageHandlers = new List<IMessageHandler>();

        public override async Task<bool> OnMessageReceived(WZClient wzsession, object message)
        {
            List<Predicate<TSession>> predicates;
            _filter.TryGetValue(message.GetType(), out predicates);

            TSession session = (TSession)wzsession;

            if (predicates != null && predicates.Any(predicate => !predicate(session)))
            {
                Logger.Debug("Dropping message {messageName} from client {remoteAddress}", message.GetType().Name, session.ID);
                return false;
            }

            var handled = false;
            foreach (var messageHandler in _messageHandlers)
            {
                var result = await messageHandler.OnMessageReceived(wzsession, message);
                if (result)
                    handled = true;
            }

            return handled;
        }

        public FilteredMessageHandler<TSession> AddHandler(IMessageHandler handler)
        {
            _messageHandlers.Add(handler);
            return this;
        }

        public FilteredMessageHandler<TSession> RegisterRule<T>(params Predicate<TSession>[] predicates)
        {
            if (predicates == null)
                throw new ArgumentNullException(nameof(predicates));

            _filter.AddOrUpdate(typeof(T),
                new List<Predicate<TSession>>(predicates),
                (key, oldValue) =>
                {
                    oldValue.AddRange(predicates);
                    return oldValue;
                });
            return this;
        }

        public FilteredMessageHandler<TSession> RegisterRule<T>(Predicate<TSession> predicate)
        {
            _filter.AddOrUpdate(typeof(T),
                new List<Predicate<TSession>> { predicate },
                (key, oldValue) =>
                {
                    oldValue.Add(predicate);
                    return oldValue;
                });
            return this;
        }
    }
}