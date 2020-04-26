using MuEmu.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu.Util
{
    public static class SendExtensions
    {
        public static async Task SendAsync(this IEnumerable<GSSession> array, object message)
        {
            foreach(var client in array)
                await client.SendAsync(message);
        }
    }
}
