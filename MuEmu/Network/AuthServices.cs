using MU.Resources;
using MuEmu.Entity;
using MuEmu.Events;
using MU.Network.Event;
using MU.Network.Game;
using MU.Network.GensSystem;
using MU.Network.Global;
using MU.Network.Pentagrama;
using MuEmu.Network.UBFSystem;
using MuEmu.Resources;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebZen.Handlers;
using WebZen.Util;
using MU.Network.Auth;
using MuEmu.Network.ConnectServer;
using System.Security.Cryptography;
using MU.Network;

namespace MuEmu.Network
{
    public class AuthServices : MessageHandler
    {
        public static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthServices));


        [MessageHandler(typeof(CIDAndPassS12))]
        public async Task CIDAndPassS12(GSSession session, CIDAndPassS12 message)
        {
            await CIDAndPass(session, new CIDAndPass
            {
                btAccount = message.btAccount,
                ClientSerial = message.ClientSerial,
                ClientVersion = message.ClientVersion,
                btPassword = message.btPassword,
                TickCount = message.TickCount
            });
        }

        [MessageHandler(typeof(CIDAndPass))]
        public async Task CIDAndPass(GSSession session, CIDAndPass message)
        {
            BuxDecode.Decode(message.btAccount);
            BuxDecode.Decode(message.btPassword);

            if(session.TryLoginCount>2)
            {
                await session.SendAsync(new SLoginResult(LoginResult.ConnectionClosed3Fail));
                return;
            }

            if(Program.server.ClientVersion != message.ClientVersion)
            {
                Logger.Error("Bad client version {0} != {1}", Program.server.ClientVersion, message.ClientVersion);
                await session.SendAsync(new SLoginResult(LoginResult.OldVersion));
                session.Disconnect();
                return;
            }

            if(Program.server.ClientSerial != message.ClientSerial)
            {
                Logger.Error("Bad client serial {0} != {1}", Program.server.ClientSerial, message.ClientSerial);
                await session.SendAsync(new SLoginResult(LoginResult.OldVersion));
                session.Disconnect();
                return;
            }

            if(string.IsNullOrWhiteSpace(message.Account) || string.IsNullOrWhiteSpace(message.Password))
            {
                await session.SendAsync(new SLoginResult(LoginResult.AccountError));
                return;
            }    

            using (var db = new GameContext())
            {
                var acc = (from account in db.Accounts
                          where account.Account.ToLower() == message.Account.ToLower()
                          select account)
                          .SingleOrDefault();

                if(acc == null)
                {
                    Logger.Information("Account {0} Don't exists", message.Account);
                    if (!Program.AutoRegistre)
                    {
                        await session.SendAsync(new SLoginResult(LoginResult.AccountError));
                        return;
                    }else
                    {
                        acc = new MU.DataBase.AccountDto
                        {
                            Account = message.Account,
                            //Password = Convert.ToBase64String(hash),
                            //Salt = Convert.ToBase64String(newSalt),
                            Characters = new List<MU.DataBase.CharacterDto>(),
                            VaultCount = 1,
                            VaultMoney = 0,
                            LastConnection = DateTime.Now,
                            IsConnected = false,
                            ServerCode = 0,
                        };

                        string Salt = "";
                        acc.Password = GetHashPassword(message.Password, ref Salt);
                        acc.Salt = Salt;
                        db.Accounts.Add(acc);
                        db.SaveChanges();
                        Logger.Information("Account Created");
                    }
                }


                var salt = acc.Salt;
                if (string.IsNullOrEmpty(salt))
                {
                    acc.Password = GetHashPassword(acc.Password, ref salt);
                    acc.Salt = salt;
                }

                if (acc.Password != GetHashPassword(message.Password, ref salt))
                {
                    await session.SendAsync(new SLoginResult(LoginResult.PasswordError));
                    session.TryLoginCount++;
                    return;
                }

                if (acc.IsConnected == true)
                {
                    await session.SendAsync(new SLoginResult(LoginResult.IsConnected));
                    return;
                }

                session.PreviousCode = 0xffff;
                acc.ServerCode = Program.ServerCode;
                acc.IsConnected = true;
                acc.LastConnection = DateTime.Now;
                db.Accounts.Update(acc);
                db.SaveChanges();

                session.Player.SetAccount(acc);
            }
            
            await session.SendAsync(new SLoginResult(LoginResult.Ok));
        }

        internal string GetHashPassword(string pwd, ref string salt)
        {
            if (string.IsNullOrEmpty(salt))
            {
                var newSalt = new byte[24];
                using (var csprng = new RNGCryptoServiceProvider())
                {
                    csprng.GetBytes(newSalt);
                }

                salt = Convert.ToBase64String(newSalt);
            }

            var hash = new Rfc2898DeriveBytes(pwd, Convert.FromBase64String(salt), 24000).GetBytes(24);
            return Convert.ToBase64String(hash);
        }

        [MessageHandler(typeof(CCharacterList))]
        public async Task CCharacterList(GSSession session)
        {
            using (var db = new GameContext())
            {
                var acc = session.Player.Account;

                byte y = 0;
                acc.Characters = (from @char in db.Characters
                                  where @char.AccountId == acc.ID
                                  select @char).ToDictionary(x => y++);

                var resetList = new SResetCharList();

                byte mClass = 0;
                var maxLevel = acc.Characters.Any()?acc.Characters.Max(x => x.Value.Level):1;

                if (maxLevel > 250)
                    mClass = 4;
                else if (maxLevel > 220)
                    mClass = 3;
                else if (maxLevel > 210)
                    mClass = 2;
                else if(maxLevel > 200)
                    mClass = 1;

                var charList = VersionSelector.CreateMessage<SCharacterList>(mClass, (byte)0, (byte)5, (byte)3) as CharList;

                foreach (var @char in acc.Characters)
                {
                    @char.Value.Items = (from item in db.Items
                                         where item.CharacterId == @char.Value.CharacterId
                                         select item).ToList();

                    charList.AddChar(
                        @char.Key,
                        @char.Value,
                        Inventory.GetCharset((HeroClass)@char.Value.Class, new Inventory(null, @char.Value), 0),
                        GuildManager.Instance.FindCharacter(@char.Value.Name)?.Rank ?? GuildStatus.NoMember);
                }

                if(Program.Season == ServerSeason.Season9Eng)
                    await session.SendAsync(resetList);

                await session.SendAsync(charList);

                await session.SendAsync(new SEnableCreation { 
                    EnableCreation = EnableClassCreation.Summoner | EnableClassCreation.RageFighter | EnableClassCreation.MagicGladiator | EnableClassCreation.GrowLancer | EnableClassCreation.DarkLord 
                });
            }
        }

        [MessageHandler(typeof(CCharacterMapJoin))]
        public async Task CCharacterMapJoin(GSSession session, CCharacterMapJoin Character)
        {
            var valid = session.Player.Account.Characters.Any(x => x.Value.Name == Character.Name);
            Logger.ForAccount(session)
                .Information("Try to join with {0}", Character.Name);
            if(Program.Season >= ServerSeason.Season17Kor)
            {
                await session.SendAsync(new SCharacterMapJoin { Name = Character.Name, Valid = (byte)(valid ? 0 : 1) });
            }else
            {
                await session.SendAsync(new SCharacterMapJoin { Name = Character.Name, Valid = (byte)(valid ? 0 : 1) });
            }
        }

        [MessageHandler(typeof(CServerList))]
        public async Task CServerList(GSSession session)
        {
            //Program.client.
            Program.client.SendAsync(new SCServerList());
            await session.SendAsync(new SServerList
            {
                List = Program.ServerList?.ToArray()??Array.Empty<MU.Network.Auth.ServerDto>()
            });
            //type:1nopvp,2pvp
        }

        [MessageHandler(typeof(CCharacterMapJoin2))]
        public async Task CCharacterMapJoin2(GSSession session, CCharacterMapJoin2 Character)
        {
            Logger.ForAccount(session)
                .Information("Join with {0}", Character.Name);
            var charDto = session.Player.Account.Characters
                .Select(x => x.Value)
                .FirstOrDefault(x => x.Name == Character.Name);

            if(!MapServerManager.CheckMapServerMove(session, (Maps)charDto.Map))
                return;

            using (var db = new GameContext())
            {
                charDto.Items = (from it in db.Items
                                 where it.CharacterId == charDto.CharacterId
                                 select it).ToList();

                charDto.Spells = (from spell in db.Spells
                                  where spell.CharacterId == charDto.CharacterId
                                  select spell).ToList();

                charDto.Quests = (from quest in db.Quests
                                   where quest.CharacterId == charDto.CharacterId
                                   select quest).ToList();

                charDto.QuestEX = (from quest in db.QuestsEX
                                  where quest.CharacterId == charDto.CharacterId
                                  select quest).ToList();

                charDto.SkillKey = (from config in db.Config
                                    where config.SkillKeyId == charDto.CharacterId
                                    select config).FirstOrDefault();

                charDto.Favorites = (from config in db.Favorites
                                    where config.CharacterId == charDto.CharacterId
                                    select config).FirstOrDefault();

                charDto.Friends = (from friend in db.Friends
                                 where (friend.FriendId == charDto.CharacterId || friend.CharacterId == charDto.CharacterId) && friend.State == 1
                                 select friend).ToList();

                charDto.Memos = (from letter in db.Letters
                            where letter.CharacterId == charDto.CharacterId
                            select letter).ToList();

                charDto.MasterInfo = (from mi in db.MasterLevel
                                     where mi.MasterInfoId == charDto.CharacterId
                                     select mi).FirstOrDefault();

                charDto.GremoryCases = (from gc in db.GremoryCase
                                        where gc.CharacterId == charDto.CharacterId && gc.AccountId == charDto.AccountId && (gc.Inventory == (byte)GremoryStorage.Character || gc.Inventory == (byte)GremoryStorage.Server)
                                        select gc).ToList();
            }

            if (@charDto == null)
                return;

            await session.SendAsync(new SCheckSum { Key = session.Player.CheckSum.GetKey(), Padding = 0xff });

            session.Player.Character = new Character(session.Player, @charDto);
            var @char = session.Player.Character;

            await session.SendAsync(new SPeriodItemCount());
            
            await session.SendAsync(new SKillCount { KillCount = 1 });
            
            if (charDto.SkillKey != null)
            {
                var skillKey = new SSkillKey
                {
                    SkillKey = charDto.SkillKey.SkillKey,
                    ChatWindow = charDto.SkillKey.ChatWindow,
                    E_Key = charDto.SkillKey.EkeyDefine,
                    GameOption = charDto.SkillKey.GameOption,
                    Q_Key = charDto.SkillKey.QkeyDefine,
                    R_Key = charDto.SkillKey.RkeyDefine,
                    W_Key = charDto.SkillKey.WkeyDefine,
                };
                if(skillKey.SkillKey == null)
                {
                    skillKey.SkillKey = Array.Empty<byte>();
                }
                await session.SendAsync(skillKey);
            }
            if(charDto.Favorites != null)
            {
                var fav = new CFavoritesList
                {
                    Region = new int[]
                    {
                        charDto.Favorites.Fav01,
                        charDto.Favorites.Fav02,
                        charDto.Favorites.Fav03,
                        charDto.Favorites.Fav04,
                        charDto.Favorites.Fav05,
                    }
                };

                await session.SendAsync(fav);
            }
            session.Player.Status = LoginStatus.Playing;

            GuildManager.Instance.AddPlayer(session.Player);

            //await session.SendAsync(new SNewQuestInfo());
            session.Player.Character.Inventory.SendJewelsInfo();

            await session.SendAsync(new SUBFInfo { Result = 1 });
            await session.SendAsync(new SMapMoveCheckSum { key = 0x0010 });

            if(Program.GlobalEventsManager.AnyEvent)
                await session.SendAsync(new SSendBanner { Type = BannerType.EvenInven });

            var bannerList = Program.EventManager.GetEvents()
                .Where(x => x.CurrentState != EventState.None && x.GetBanner() != (BannerType)0xff)
                .Select(x => x.GetBanner())
                .ToList();

            foreach( var banner in bannerList)
                await session.SendAsync(new SSendBanner { Type = banner });

            //ConnectServer dataSend
            Program.client.SendAsync(new SCAdd { Server = (byte)Program.ServerCode, btName = @charDto.Name.GetBytes() });

            if((@char.CtlCode & ControlCode.GameMaster) == ControlCode.GameMaster)
            {
                @char.Spells.SetBuff(SkillStates.GameMaster, TimeSpan.FromDays(100));
            }

            @char.DataLoaded = true;
        }

        [MessageHandler(typeof(CCharacterCreate))]
        public async Task CCharacterCreate(GSSession session, CCharacterCreate message)
        {
            var log = Logger.ForAccount(session);

            using (var db = new GameContext())
            {
                var exists = (from @char in db.Characters
                              where @char.Name.ToLower() == message.Name.ToLower()
                              select @char).Any();

                if(exists)
                {
                    log.Information("Character name {0} is in use", message.Name);
                    await session.SendAsync(new SCharacterCreate(0));
                    return;
                }

                log.Information("Creating character {0} class:{1}", message.Name, message.Class);

                var defaultChar = ResourceCache.Instance.GetDefChar()[message.Class];

                var gate = ResourceCache.Instance.GetGates()
                    .Where(s => s.Value.Map == defaultChar.Map && s.Value.GateType == GateType.Warp)
                    .Select(s => s.Value)
                    .FirstOrDefault();

                var rand = new Random();
                var x = (byte)rand.Next(gate?.Door.Left ?? 0, gate?.Door.Right ?? 126);
                var y = (byte)rand.Next(gate?.Door.Top ?? 0, gate?.Door.Bottom ?? 126);

                var newChar = new MU.DataBase.CharacterDto
                {
                    AccountId = session.Player.Account.ID,
                    Class = (byte)message.Class,
                    Experience = 0,
                    GuildId = null,
                    Level = defaultChar.Level,
                    LevelUpPoints = 0,
                    Name = message.Name,
                    Quests = new List<MU.DataBase.QuestDto>(),
                    Items = new List<MU.DataBase.ItemDto>(),
                    // Map
                    Map = (byte)defaultChar.Map,
                    X = x,
                    Y = y,
                    // Stats
                    Str = (ushort)defaultChar.Stats.Str,
                    Agility = (ushort)defaultChar.Stats.Agi,
                    Vitality = (ushort)defaultChar.Stats.Vit,
                    Energy = (ushort)defaultChar.Stats.Ene,
                    Command = (ushort)defaultChar.Stats.Cmd,
                    CtlCode = 0,
                    Life = (ushort)defaultChar.Attributes.Life,
                    Mana = (ushort)defaultChar.Attributes.Mana,
                    MaxLife = (ushort)defaultChar.Attributes.Life,
                    MaxMana = (ushort)defaultChar.Attributes.Mana,
                    Resets = 0
                };

                db.Characters.Add(newChar);
                db.SaveChanges();

                var position = (byte)session.Player.Account.Characters.Count();

                session.Player.Account.Characters.Add(position, newChar);

                var items = defaultChar.Equipament.Select(eq => new MU.DataBase.ItemDto
                {
                    AccountId = session.Player.Account.ID,
                    CharacterId = newChar.CharacterId,
                    VaultId = eq.Key >= 12 ? 12 : 0,
                    SlotId = eq.Key,
                    DateCreation = DateTime.Now,
                    Durability = eq.Value.Durability,
                    HarmonyOption = eq.Value.Harmony.Option,
                    Luck = eq.Value.Luck,
                    Number = eq.Value.Number,
                    Option = eq.Value.Option28,
                    OptionExe = (byte)eq.Value.OptionExe,
                    Plus = eq.Value.Plus,
                    Skill = eq.Value.Skill,
                    SocketOptions = string.Join(",", eq.Value.Slots.Select(s => s.ToString())),
                });

                db.Items.AddRange(items.ToArray());
                db.SaveChanges();

                await session.SendAsync(new SCharacterCreate(1, 
                    message.Name,
                    position,
                    newChar.Level, 
                    Array.Empty<byte>(),
                    Character.GetClientClass(message.Class)
                    ));
            }
        }

        [MessageHandler(typeof(CCharacterDelete))]
        public async Task CCharacterDelete(GSSession session, CCharacterDelete message)
        {
            var result = CharacterDeleteResult.Error;
            using (var db = new GameContext())
            {
                var @char = db.Characters.FirstOrDefault(x => x.Name == message.Name);
                if (@char != null)
                {
                    if (@char.GuildId == null)
                    {
                        db.Characters.Remove(@char);
                        db.SaveChanges();

                        var pk = session.Player.Account.Characters.FirstOrDefault(x => x.Value.Name == message.Name);
                        session.Player.Account.Characters.Remove(pk.Key);
                        result = CharacterDeleteResult.Success;
                    }
                }
            }
            await session.SendAsync(new SCharacterDelete { Result = result });
        }

        [MessageHandler(typeof(SSkillKey))]
        public void CSkillKey(GSSession session, SSkillKey message)
        {
            if (session.Player == null || session.Player.Character == null)
                return;

            using (var db = new GameContext())
            {
                var res = db.Config.FirstOrDefault(x => x.SkillKeyId == session.Player.Character.Id);
                if(res == null)
                {
                    db.Config.Add(new MU.DataBase.SkillKeyDto
                    {
                        SkillKeyId = session.Player.Character.Id,
                        SkillKey = message.SkillKey,
                        QkeyDefine = message.Q_Key,
                        EkeyDefine = message.E_Key,
                        WkeyDefine = message.W_Key,
                        GameOption = message.GameOption,
                        ChatWindow = message.ChatWindow,
                        RkeyDefine = message.R_Key,
                        //                    QWERLevelDefine = message.q
                    });
                }
                else
                {
                    //res.SkillKeyId = session.Player.Character.Id;
                    res.SkillKey = message.SkillKey;
                    res.QkeyDefine = message.Q_Key;
                    res.EkeyDefine = message.E_Key;
                    res.WkeyDefine = message.W_Key;
                    res.GameOption = message.GameOption;
                    res.ChatWindow = message.ChatWindow;
                    res.RkeyDefine = message.R_Key;
                    db.Config.Update(res);
                }

                db.SaveChanges();
            }
        }

        [MessageHandler(typeof(CServerMove))]
        public async Task CServerMove(GSSession session, CServerMove message)
        {
            Logger.ForAccount(session).Information("Server move recv");
            BuxDecode.Decode(message.btAccount);

            if (Program.server.ClientVersion != message.ClientVersion)
            {
                Logger.Error("Bad client version {0} != {1}", Program.server.ClientVersion, message.ClientVersion);
                await session.SendAsync(new SLoginResult(LoginResult.OldVersion));
                session.Disconnect();
                return;
            }

            if (Program.server.ClientSerial != message.ClientSerial)
            {
                Logger.Error("Bad client serial {0} != {1}", Program.server.ClientSerial, message.ClientSerial);
                await session.SendAsync(new SLoginResult(LoginResult.OldVersion));
                session.Disconnect();
                return;
            }

            using (var db = new GameContext())
            {
                var acc = (from account in db.Accounts
                           where string.Equals(account.Account, message.Account, StringComparison.InvariantCultureIgnoreCase)
                           select account)
                          .FirstOrDefault();

                var token = $"{message.AuthCode1:X8}{message.AuthCode2:X8}{message.AuthCode3:X8}{message.AuthCode4:X8}";

                if (acc.AuthToken != token)
                {
                    await session.SendAsync(new SLoginResult(LoginResult.ConnectionError));
                    return;
                }

                session.PreviousCode = (ushort)acc.ServerCode;
                acc.ServerCode = Program.ServerCode;
                acc.IsConnected = true;
                acc.LastConnection = DateTime.Now;
                db.Accounts.Update(acc);
                db.SaveChanges();

                byte y = 0;
                session.Player.SetAccount(acc);
                var _acc = session.Player.Account;
                _acc.Characters = (from @char in db.Characters
                                  where @char.AccountId == acc.AccountId
                                  select @char).ToDictionary(x => y++);

                foreach (var @char in _acc.Characters)
                {
                    @char.Value.Items = (from item in db.Items
                                         where item.CharacterId == @char.Value.CharacterId
                                         select item).ToList();
                }
            }

            await CCharacterMapJoin2(session, new CCharacterMapJoin2 { Name = message.Character });
        }
    }
}
