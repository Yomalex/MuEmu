using MuEmu.Network.CashShop;
using MuEmu.Network.Game;
using MuEmu.Network.Global;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;

namespace MuEmu.Network.Auth
{
    public class AuthServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthServices));

        [MessageHandler(typeof(CIDAndPass))]
        public async Task CIDAndPass(GSSession session, CIDAndPass message)
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

            await session.SendAsync(new SLoginResult(LoginResult.Ok));
        }

        [MessageHandler(typeof(CCharacterList))]
        public async Task CCharacterList(GSSession session, CCharacterList listReq)
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
            await session.SendAsync(chars);
        }

        [MessageHandler(typeof(CCharacterMapJoin))]
        public async Task CCharacterMapJoin(GSSession session, CCharacterMapJoin Character)
        {
            await session.SendAsync(new SCharacterMapJoin { Name = Character.Name, Valid = 0 });
        }

        [MessageHandler(typeof(CCharacterMapJoin2))]
        public async Task CCharacterMapJoin2(GSSession session, CCharacterMapJoin2 Character)
        {
            await session.SendAsync(new SEventState(MapEvents.GoldenInvasion, false));

            await session.SendAsync(new SCheckSum { Key = session.Player.CheckSum.GetKey(), Padding = 0xff });

            await session.SendAsync(new SCashPoints { CashPoints = 0 });

            session.Player.Character = new Character(session.Player, new MU.DataBase.CharacterDto
            {
                Index = 6,
                Name = "Yomalex",
                Class = (byte)HeroClass.Summoner,
                Level = 1,
                Experience = 0,
                Str = 21,
                Agility = 21,
                Vitality = 18,
                Energy = 23,
                Command = 0,
                Life = 10,
                Mana = 10,
                MaxLife = 100,
                MaxMana = 100,
                Map = 51,
                X = 53,
                Y = 225
            });
            var @char = session.Player.Character;
            //FriendListRequest
            await session.SendAsync(new SFriends { MemoCount = 0, Friends = new Data.FriendDto[] { new Data.FriendDto { Name = "Yomalex2".GetBytes() } } });
            
            await session.SendAsync(new SKillCount { KillCount = 1 });
            await session.SendAsync(new SNotice
            {
                Notice = $"Bienvenido {@char.Name} a mu desertor"
            });

            await session.SendAsync(new SSkillKey());
            session.Player.Status = LoginStatus.Playing;
        }

        [MessageHandler(typeof(CCharacterCreate))]
        public async Task CCharacterCreate(GSSession session, CCharacterCreate message)
        {
            await session.SendAsync(new SCharacterCreate
            {
                Name = message.Name,
                Level = 1,
                Position = 1,
                Class = message.Class,
                Result = 1
            });
        }

        [MessageHandler(typeof(CCharacterDelete))]
        public async Task CCharacterDelete(GSSession session, CCharacterDelete message)
        {
            await session.SendAsync(new SCharacterDelete());
        }
    }
}
