using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using MuEmu.Resources;
using System.Drawing;
using MuEmu.Network.Game;
using MuEmu.Network.Data;

namespace MuEmu
{
    public class DelayedMessage
    {
        public Player Player;
        public DateTimeOffset Time;
        public object Message;
    }

    public class SubSystem
    {
        private Thread _workerDelayed;
        private Thread _workerViewPort;
        private List<DelayedMessage> _delayedMessages;
        public static SubSystem Instance { get; set; }

        public SubSystem()
        {
            _delayedMessages = new List<DelayedMessage>();
            _workerDelayed = new Thread(WorkerDelayed);
            _workerViewPort = new Thread(WorkerViewPort);
        }

        public void AddDelayedMessage(Player plr, TimeSpan time, object message)
        {
            _delayedMessages.Add(new DelayedMessage { Player = plr, Message = message, Time = DateTimeOffset.Now.Add(time) });
        }

        private static async void WorkerDelayed()
        {
            while(true)
            {
                var toSend = (from msg in Instance._delayedMessages
                             where DateTimeOffset.Now >= msg.Time
                             select msg).ToList();

                foreach(var msg in toSend)
                {
                    await msg.Player.Session.SendAsync(msg.Message);
                }

                Instance._delayedMessages = (from msg in Instance._delayedMessages
                                             where DateTimeOffset.Now < msg.Time
                                             select msg).ToList();

                Thread.Sleep(100);
            }
        }

        private static async void WorkerViewPort()
        {
            while(true)
            {
                try
                {
                    foreach (var map in ResourceCache.Instance.GetMaps())
                    {
                        foreach (var @char in map.Value.Players)
                        {
                            var pos = @char.Position;
                            pos.Offset(15, 15);

                            var Monsters = from monst in map.Value.Monsters
                                           let rect = new Rectangle(monst.Position, new Size(30, 30))
                                           where rect.Contains(pos) && monst.Life > 0
                                           select new VPMCreateDto
                                           {
                                               Number = monst.Index,
                                               Position = monst.Position,
                                               TPosition = monst.Position,
                                               Type = monst.Info.Monster,
                                               ViewSkillState = Array.Empty<byte>(),
                                               Path = (byte)(monst.Direction << 4)
                                           };

                            var MonstersID = Monsters.Select(x => (ushort)x.Number);

                            var add = from monstID in MonstersID.Except(@char.MonstersVP)
                                      from monst in Monsters
                                      where monst.Number == monstID
                                      select monst;

                            var remove = @char.MonstersVP.Except(MonstersID).Select(x => new VPDestroyDto(x));
                            @char.MonstersVP = MonstersID;

                            if (remove.Count() > 0)
                                await @char.Player.Session.SendAsync(new SViewPortDestroy
                                {
                                    ViewPort = remove.ToArray()
                                });

                            if (add.Count() > 0)
                                await @char.Player.Session.SendAsync(new SViewPortMonCreate
                                {
                                    ViewPort = add.ToArray()
                                });

                            @char.Health += @char.BaseInfo.Attributes.LevelLife * @char.Level;
                            @char.Mana += @char.BaseInfo.Attributes.LevelMana * @char.Level;
                        }
                    }
                    Thread.Sleep(1000);
                }catch(Exception)
                {

                }
            }
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new Exception("Already Initialized");

            Instance = new SubSystem();

            Instance._workerDelayed.Start();
            Instance._workerViewPort.Start();
        }
    }
}
