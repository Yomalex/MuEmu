using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Network;
using WebZen.Util;

namespace MuEmu.Network.Game
{
    public class GameServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GameServices));

        [MessageHandler(typeof(CCheckSum))]
        public void CCheckSum(GSSession session, CCheckSum message)
        {
            //session.Player.CheckSum.IsValid(message.Key);
            Logger
                .ForAccount(session)
                .Debug("Key {0:X4}", message.Key);
        }

        [MessageHandler(typeof(CClientMessage))]
        public void CClientMessage(GSSession session, CClientMessage message)
        {
            Logger
                .ForAccount(session)
                .Information("Client Hack Check {0}", message.Flag);
        }

        [MessageHandler(typeof(CAction))]
        public void CAction(GSSession session, CAction message)
        {
            session.Player.Character.Direction = message.Dir;
            session.SendAsync(new SAction((ushort)session.Player.Account.ID, message.Dir, message.ActionNumber, message.Target));
        }

        [MessageHandler(typeof(CMove))]
        public void CMove(GSSession session, CMove message)
        {
            var dirs = new List<Point>
            {
                new Point(-1,-1),
                new Point(0, -1),
                new Point(1, -1),

                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1),

                new Point(-1, 1),
                new Point(-1, 0)
            };

            var @char = session.Player.Character;
            var count = message.Path[0] & 0x0F;
            var solvedPath = new List<Point>();
            var Cpos = new Point(message.X, message.Y);
            solvedPath.Add(Cpos);

            var valid = true;

            for (int i = 1; i <= count; i++)
            {
                var a = (message.Path[(i + 1) / 2] >> (((i % 2) == 1) ? 4 : 0)) & 0x0F;
                Cpos.Offset(dirs[a]);
                solvedPath.Add(Cpos);
                //Logger.Debug("Path solved [{0}] X:{1} Y:{2}", i, Cpos.X, Cpos.Y);
                var att = @char.Map.GetAttributes(Cpos);
                if (att.Where(y => y == Resources.Map.MapAttributes.NoWalk || y == Resources.Map.MapAttributes.Hide).Count() > 0)
                {
                    valid = false;
                }
            }

            if (!valid)
            {
                session.SendAsync(new SPositionSet { Number = (ushort)session.Player.Account.ID.ShufleEnding(), X = (byte)@char.Position.X, Y = (byte)@char.Position.Y });
                Logger
                    .ForAccount(session)
                    .Error("Invalid path");
                return;
            }

            @char.Position = Cpos;

            session.SendAsync(new SMove((ushort)session.Player.Account.ID, (byte)Cpos.X, (byte)Cpos.Y, message.Path[0]));
        }

        [MessageHandler(typeof(CChatNickname))]
        public void CChatNickname(GSSession session, CChatNickname message)
        {
            Logger
                .ForAccount(session)
                .Information("Chat [" + message.Character.MakeString() + "] {0}", message.Message.MakeString());

        }

        [MessageHandler(typeof(CNewQuestInfo))]
        public void CNewQuestInfo(GSSession session, CNewQuestInfo message)
        {
            Logger
                .ForAccount(session)
                .Information("Quest S5 {0}", message.Quest);
            session.SendAsync(message);
        }

        [MessageHandler(typeof(CClinetClose))]
        public void CClinetClose(GSSession session, CClinetClose message)
        {
            Logger
                .ForAccount(session)
                .Information("User request {0}", message.Type);
            session.SendAsync(message);
        }

        [MessageHandler(typeof(CMoveItem))]
        public void CMoveItem(GSSession session, CMoveItem message)
        {
            Logger.Debug("Move item {0} {1} to {2} {3}", message.sFlag, message.Source, message.tFlag, message.Dest);
            session.SendAsync(new SMoveItem
            {
                ItemInfo = message.ItemInfo,
                Position = message.Dest,
                Result = (byte)message.tFlag
            });
        }

        // lacting
        [MessageHandler(typeof(CUseItem))]
        public void CUseItem(GSSession session, CUseItem message)
        {

        }

        [MessageHandler(typeof(CEventEnterCount))]
        public void CEventEnterCount(GSSession session, CEventEnterCount message)
        {
            session.SendAsync(new SEventEnterCount { Type = message.Type });
        }
    }
}
