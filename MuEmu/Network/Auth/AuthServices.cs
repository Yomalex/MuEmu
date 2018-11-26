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
            //await session.SendAsync(new SWeather { Weather = 0x10 });

            // Event Map State
            await session.SendAsync(new SEventState(MapEvents.GoldenInvasion, false));

            await session.SendAsync(new SCheckSum { Key = session.Player.CheckSum.GetKey(), Padding = 0xff });

            await session.SendAsync(new SCashPoints { CashPoints = 0 });

            // CashItem Effects

            // gObjSetCharacter END

            //QuestInfoSave

            // CharInfo Send
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
            //await session.SendAsync(new SCharacterMapJoin2
            //{
            //    Map = Maps.Elbeland,
            //    MapX = 53,
            //    MapY = 225,
            //    Direccion = 4,
            //    Experience = 0,
            //    NextExperience = 100u.ShufleEnding(),
            //    Life = 66,
            //    MaxLife = 66,
            //    Mana = 46,
            //    MaxMana = 46,
            //    MaxAddPoints = 100,
            //    LevelUpPoints = 0,
            //    MaxMinusPoints = 100,
            //    MaxStamina = 11,
            //    Stamina = 11,
            //    Shield = 101,
            //    MaxShield = 101,
            //    PKLevel = 3,
            //    Str = 21,
            //    Agi = 21,
            //    Vit = 18,
            //    Ene = 23,
            //});
            //await Task.Delay(250);
            //session.Send(new byte[] {
            //    0xC3, 0x65, 0xA3, 0x34, 0x11, 0xD2, 0x17, 0x9A, 0x05, 0x30, 0x14, 0xDA, 0xEF, 0x45, 0x1B, 0x0D,
            //    0x0F, 0x12, 0x52, 0x02, 0xD4, 0xD8, 0xCD, 0xF8, 0x45, 0x1B, 0x0D, 0x0F, 0x19, 0x16, 0x70, 0x05,
            //    0xC4, 0xA9, 0x9C, 0x12, 0xFB, 0x13, 0x90, 0x41, 0xB2, 0xB4, 0x2B, 0xB9, 0xC8, 0xFD, 0x25, 0x56,
            //    0x08, 0x0D, 0x82, 0x15, 0x62, 0xA6, 0x05, 0xCD, 0xF8, 0x20, 0x1A, 0x13, 0xD1, 0x45, 0x17, 0x62,
            //    0x3D, 0xDE, 0xD0, 0xE5, 0x45, 0x1B, 0x0D, 0x0F, 0x17, 0xD7, 0x33, 0xB5, 0x96, 0xCE, 0xFB, 0x61,
            //    0xDB, 0x1E, 0x4B, 0xC4, 0xDC, 0xB0, 0xF0, 0x0D, 0xCD, 0xF8, 0x2F, 0xFA, 0x28, 0x6F, 0x46, 0x32,
            //    0x02, 0x11, 0x6D, 0xE5, 0xDB/*, 0xC4, 0x00, 0x0E, 0xB7, 0xE2, 0x30, 0x12, 0x54, 0x54, 0x91, 0xCB,
            //    0xB0, 0x23, 0x1A*/ });

            //GCItemListSend
            //var it = new Item(0, 0, new { Luck = true });

            //await session.SendAsync(new SInventory
            //{
            //    Inventory = new Data.InventoryDto[]
            //    {
            //        new Data.InventoryDto
            //        {
            //            Index = 0,
            //            Item = it.GetBytes()
            //        }
            //    }
            //});
            //session._outSerial = 1;
            //await session.SendAsync(new SInventory());

            //GCMagicListMultiSend
            //await session.SendAsync(new SSpells(0, new Data.SpellDto[] { new Data.SpellDto { Index = 0, Spell = 17 } }));

            //CGRequestQuestInfo
            //await session.SendAsync(new SQuestInfo { Count = 0, State = Array.Empty<byte>() });

            //DGGuildMemberInfoRequest?

            //FriendListRequest
            await session.SendAsync(new SFriends { MemoCount = 0, Friends = new Data.FriendDto[] { new Data.FriendDto { Name = "Yomalex2".GetBytes() } } });
            //await Task.Delay(250);

            //GCMapEventStateSend?

            //SkillChangeUse
            //await session.SendAsync(new SViewPortCreate
            //{
            //    ViewPort = new Data.VPCreateDto[]
            //    {
            //        new Data.VPCreateDto
            //        {
            //            CharSet = Array.Empty<byte>(),
            //            DirAndPkLevel = 0,
            //            Name = @char.Name,
            //            Number = session.Player.Account.ID,
            //            Position = @char.Position,
            //            TPosition = @char.Position,
            //            ViewSkillState = Array.Empty<byte>(),
            //        }
            //    }
            //});

            await session.SendAsync(new SKillCount { KillCount = 1 });
            //SkillChangeUse END

            //GCReFillSend
            //GCManaSend

            //GCServerMsgStringSend
            await session.SendAsync(new SNotice
            {
                Notice = $"Bienvenido {@char.Name} a mu desertor"
            });

            await session.SendAsync(new SSkillKey());

            //SendTWindow
            //await session.SendAsync(new SNewQuestInfo { QuestList = new Data.NewQuestInfoDto[] { new Data.NewQuestInfoDto { Quest = 0, Number = 0 } } });
            //await Task.Delay(250);

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
