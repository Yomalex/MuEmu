using MU.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WebZen.Network;
using BlubLib;

namespace MU.Network
{
    public class VersionSelector
    {
        private ServerSeason _activeSeason;
        private Dictionary<ushort, Type> _active;
        private Dictionary<ServerSeason, Dictionary<ushort, Type>> _types = new Dictionary<ServerSeason, Dictionary<ushort, Type>>();
        private Dictionary<Type, ushort> _opCodeLookUp = new Dictionary<Type, ushort>();

        private static VersionSelector s_instance;

        private VersionSelector(ServerSeason s) { _activeSeason = s; }

        public static void Initialize(ServerSeason s)
        {
            if(s_instance == null)
                s_instance = new VersionSelector(s);
        }

        /// <summary>
        /// Associate a class with a server version
        /// </summary>
        /// <typeparam name="_T">Class Type of message</typeparam>
        /// <param name="season">server version</param>
        /// <param name="opCode">The message operation code</param>
        public static void Register<_T>(ServerSeason season, Enum opCode)
            where _T : new()
        {
            if(!s_instance._types.ContainsKey(season))
                s_instance._types.Add(season, new Dictionary<ushort, Type>());

            var usOpCode = Convert.ToUInt16(opCode);

            s_instance._types[season].Add(usOpCode, typeof(_T));
            s_instance._opCodeLookUp.Add(typeof(_T), usOpCode);

            if (s_instance._active == null && season == s_instance._activeSeason)
                s_instance._active = s_instance._types[season];

        }

        /// <summary>
        /// Create a message version matched with current server version
        /// </summary>
        /// <typeparam name="_T">Class Type of message</typeparam>
        /// <param name="args">Constructor Args</param>
        /// <returns>Instance of message</returns>
        public static object CreateMessage<_T>(params object[] args)
            where _T : new()
        {
            var result = s_instance._opCodeLookUp[typeof(_T)];
            var type = s_instance._active[result];
            return Activator.CreateInstance(type, args);
        }
    }
}
