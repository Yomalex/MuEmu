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
            // gObjSetCharacter
            await session.SendAsync(new SWeather { Weather = 0x10 });

            // Event Map State
            await session.SendAsync(new SEventState(MapEvents.GoldenInvasion, false));

            await session.SendAsync(new SCheckSum { Key = session.Player.CheckSum.GetKey() });

            await session.SendAsync(new SCashPoints { CashPoints = 0 });

            // CashItem Effects

            // gObjSetCharacter END

            //QuestInfoSave

            // CharInfo Send
            await session.SendAsync(new SCharacterMapJoin2
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

            //GCItemListSend
            var it = new Item(0, 0, new { Luck = true });

            await session.SendAsync(new SInventory
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

            //GCMagicListMultiSend
            await session.SendAsync(new SSpells(0, new Data.SpellDto[] { new Data.SpellDto { Index = 0, Spell = 17 } }));

            //CGRequestQuestInfo
            await session.SendAsync(new SQuestInfo { Count = 0, State = Array.Empty<byte>() });

            //DGGuildMemberInfoRequest?

            //FriendListRequest
            await session.SendAsync(new SFriends { MemoCount = 1, Friends = new Data.FriendDto[] { new Data.FriendDto { Name = "Yomalex2".GetBytes() } } });

            //GCMapEventStateSend?

            //SkillChangeUse
            await session.SendAsync(new SViewPortCreate
            {
                ViewPort = new Data.VPCreateDto[]
                {
                    new Data.VPCreateDto
                    {
                        CharSet = Array.Empty<byte>(),
                        DirAndPkLevel = 0,
                        Name = "Yomalex",
                        Number = session.Player.Account.ID,
                        Position = new Point(120,120),
                        TPosition = new Point(120,120),
                        ViewSkillState = Array.Empty<byte>(),
                    }
                }
            });
            
            await session.SendAsync(new SKillCount { KillCount = 1 });
            //SkillChangeUse END

            //GCReFillSend
            //GCManaSend

            //GCServerMsgStringSend
            await session.SendAsync(new SNotice
            {
                Notice = "Welcome!"
            });

            //SendTWindow
            await session.SendAsync(new SNewQuestInfo { QuestList = new Data.NewQuestInfoDto[] { new Data.NewQuestInfoDto { Quest = 0, Number = 0 } } });

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
