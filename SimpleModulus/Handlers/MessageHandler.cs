using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                            var isAsync = m.GetCustomAttributes().Where(x => x.GetType() == typeof(AsyncStateMachineAttribute)).Count() > 0;
                            var lArgs = new List<object>
                            {
                                session,
                                message
                            };
                            var args = lArgs.Take(m.GetParameters().Length).ToArray();
                            try
                            {
                                if (isAsync)
                                    await (Task)m.Invoke(this, args);
                                else
                                    m.Invoke(this, args);
                            }catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                                return false;
                            }
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
