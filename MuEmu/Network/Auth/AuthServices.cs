using MuEmu.Network.Game;
using MuEmu.Network.Global;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network.Auth
{
    public class AuthServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthServices));

        [MessageHandler(typeof(CIDAndPass))]
        public void CIDAndPass(GSSession session, CIDAndPass message)
        {
            BuxDecode.Decode(message.btAccount);
            BuxDecode.Decode(message.btPassword);

            Logger.Debug("ID:{account} PW:{password} Client:{cl}#!{cs}", message.Account, message.Password, message.ClientVersion, message.ClientSerial);

            if(Program.server.ClientVersion != message.ClientVersion)
            {
                Logger.Error("Bad client version {0} != {1}", Program.server.ClientVersion, message.ClientVersion);
                session.Disconnect();
                return;
            }

            var acc = session.Player.Account;
            acc.ID = 1;
            acc.Nickname = message.Account;
            session.Player.Status = LoginStatus.Logged;

            session.SendAsync(new SLoginResult(LoginResult.Ok));
        }

        [MessageHandler(typeof(CCharacterList))]
        public void CCharacterList(GSSession session, CCharacterList listReq)
        {
            var chars = new SCharacterList(5, 0, new CharacterPreviewDto[] {
                new CharacterPreviewDto
                {
                    ID = 0,
                    Level = 1,
                    Name = "Yomalex".GetBytes(),
                    CharSet = new Data.CharsetDto{ Class = Character.GetClientClass(HeroClass.BlodySummoner), CharSet = Array.Empty<byte>() },
                    ControlCode = ControlCode.GameMaster,
                    GuildStatus = GuildStatus.NoMember
                }
            });
            session.SendAsync(chars);
        }

        [MessageHandler(typeof(CCharacterMapJoin))]
        public void CCharacterMapJoin(GSSession session, CCharacterMapJoin Character)
        {
            session.SendAsync(new SCharacterMapJoin { Name = Character.Name, Valid = 0 });
        }

        [MessageHandler(typeof(CCharacterMapJoin2))]
        public void CCharacterMapJoin2(GSSession session, CCharacterMapJoin2 Character)
        {
            session.SendAsync(new SWeather { Weather = 0x10 });

            // Event Map State
            session.SendAsync(new SEventState(MapEvents.GoldenInvasion, false));

            session.SendAsync(new SCheckSum { Key = session.Player.CheckSum.GetKey() });

            session.SendAsync(new SSpells(0, Array.Empty<Data.SpellDto>()));

            session.SendAsync(new SCharacterMapJoin2
            {
                Map = Maps.Lorencia,
                MapX = 120,
                MapY = 120,
                Experience = 0,
                NextExperience = 100u.ShufleEnding(),
                Life = 100,
                MaxLife = 100,
                Mana = 100,
                MaxMana = 100,
                Str = 32,
                Agi = 32,
                Vit = 32,
                Ene = 32,
            });

            var it = new Item(0, 0, new { Luck = true });

            session.SendAsync(new SInventory
            {
                Inventory = new Data.InventoryDto[]
                {
                    new Data.InventoryDto
                    {
                        Index = 0,
                        Item = it.GetBytes()
                    }
                }
            });

            session.SendAsync(new SEquipament { Number = (ushort)session.ID.ShufleEnding() });

            session.SendAsync(new SQuestInfo { Count = 0, State = Array.Empty<byte>() });

            session.SendAsync(new SFriends());


            // Skill Change Use

            session.SendAsync(new SViewPortCreate());

            session.SendAsync(new SViewPortMonCreate());

            session.SendAsync(new SKillCount());

            session.SendAsync(new SNotice
            {
                Notice = "Welcome!"
            });

            session.Player.Status = LoginStatus.Playing;
        }

        [MessageHandler(typeof(CCharacterCreate))]
        public void CCharacterCreate(GSSession session, CCharacterCreate message)
        {
            session.SendAsync(new SCharacterCreate
            {
                Name = message.Name,
                Level = 1,
                Position = 1,
                Class = message.Class,
                Result = 1
            });
        }

        [MessageHandler(typeof(CCharacterDelete))]
        public void CCharacterDelete(GSSession session, CCharacterDelete message)
        {
            session.SendAsync(new SCharacterDelete());
        }
    }
}
