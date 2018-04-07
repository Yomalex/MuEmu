using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebZen.Network;

namespace WebZen.Handlers
{
    public class MessageHandler : IMessageHandler
    {
        public virtual async Task<bool> OnMessageReceived(WZClient session, object message)
        {
            foreach (var m in GetType().GetMethods())
            {
                foreach (var a in m.GetCustomAttributes(false))
                {
                    if(a.GetType() == typeof(MessageHandlerAttribute))
                    {
                        var mh = a as MessageHandlerAttribute;
                        if (mh._type == message.GetType())
                        {
                            m.Invoke(this, new object[] { session, message });
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public interface IMessageHandler
    {
        Task<bool> OnMessageReceived(WZClient session, object message);
    }
}
