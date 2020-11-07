using MU.DataBase;
using MuEmu.Entity;
using MuEmu.Network.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuEmu
{
    public class Friends
    {
        public static bool AddSet;
        private Character character;
        private List<Network.Data.FriendDto> _friends;
        private List<MemoDto> _memos;

        public Friends(Character character, CharacterDto characterDto)
        {
            this.character = character;
            var ids = characterDto.Friends.Select(
                x => x.FriendId == characterDto.CharacterId ? x.CharacterId : x.FriendId
                );

            using (var db = new GameContext())
            {
                var chars = ids.Select(x => db.Characters.First(z => z.CharacterId == x));
                var accs = chars.Select(x => db.Accounts.First(z => z.AccountId == x.AccountId));
                var info = from c in chars
                           from a in accs
                           where c.AccountId == a.AccountId
                           select new { c.CharacterId, c.Name, a.ServerCode, a.IsConnected };

                _friends = info.Select(x =>
                new Network.Data.FriendDto
                {
                    Name = x.Name,
                    Server = (byte)(x.IsConnected ? x.ServerCode : 255)
                }).ToList();

                var result = from f in db.Friends
                             from c in db.Characters
                             where c.CharacterId == f.FriendId && c.CharacterId == character.Id
                             select new SFriendAddSin { Name = c.Name };

                foreach (var msg in result)
                    character.Player.Session.SendAsync(msg).Wait();
            }

            _memos = characterDto.Memos;

            character.Player.Session.SendAsync(new SFriends {
                Friends = _friends.ToArray(),
                MailTotal = 50,
                MemoCount = (byte)_memos.Count(),
            }).Wait();
        }

        public async void ConnectFriend(string name, byte server)
        {
            var result = _friends.FirstOrDefault(x => x.Name == name);
            if (result == null)
                return;

            result.Server = server;

            character.Player.Session.SendAsync(new SFriends
            {
                Friends = _friends.ToArray(),
                MailTotal = 50,
                MemoCount = (byte)_memos.Count(),
            }).Wait();

            await character.Player.Session.SendAsync(new SNotice(NoticeType.Blue, name + " is now Online"));
        }

        public async void DisconnectFriend(string name)
        {
            var result = _friends.FirstOrDefault(x => x.Name == name);
            if (result == null)
                return;

            result.Server = 255;

            character.Player.Session.SendAsync(new SFriends
            {
                Friends = _friends.ToArray(),
                MailTotal = 50,
                MemoCount = (byte)_memos.Count(),
            }).Wait();

            await character.Player.Session.SendAsync(new SNotice(NoticeType.Blue, name + " is now Offline"));
        }

        internal void AddFriend(string name)
        {
            using (var db = new GameContext())
            {
                var @char = db.Characters.First(x => x.Name == name);
                @char.Account = db.Accounts.First(x => x.AccountId == @char.AccountId);

                if (@char.CharacterId == character.Id)
                {
                    character.Player.Session.SendAsync(new SFriendAddReq
                    {
                        Name = @char.Name,
                        Result = 5,
                        State = 255
                    }).Wait();
                    return;
                }

                var res = from f in db.Friends
                          where (f.CharacterId == character.Id || f.FriendId == character.Id) && (f.CharacterId == @char.CharacterId || f.FriendId == @char.CharacterId)
                          select f;

                if (res.Count() != 0)
                {
                    character.Player.Session.SendAsync(new SFriendAddReq
                    {
                        Name = @char.Name,
                        Result = 5,
                        State = 255
                    }).Wait();
                    return;
                }

                db.Friends.Add(new MU.DataBase.FriendDto
                {
                    CharacterId = character.Id,
                    FriendId = @char.CharacterId,
                    State = 0,
                });

                db.SaveChanges();

                character.Player.Session.SendAsync(new SFriendAddReq
                {
                    Name = @char.Name,
                    Result = 1,
                    State = 255
                }).Wait();
            }
            AddSet = true;
        }

        internal void AcceptFriend(string name, byte Result)
        {
            using (var db = new GameContext())
            {
                var @char = db.Characters.First(x => x.Name == name);
                var frdDto = db.Friends.First(x =>
                x.FriendId == character.Id && x.CharacterId == @char.CharacterId);

                if(Result == 1)
                {
                    frdDto.State = 1;
                }else
                {
                    db.Friends.Remove(frdDto);
                }

                db.SaveChanges();
                character.Player.Session.SendAsync(new SFriendAddReq
                {
                    Name = @char.Name,
                    Result = 1,
                    State = (byte)(@char.Account.IsConnected ? @char.Account.ServerCode : 255)
                }).Wait();

                if(@char.Account.IsConnected)
                {
                    var session = Program.server.Clients.FirstOrDefault(x => x.Player.Character.Name == @char.Name);
                    if(session != null)
                    {
                        session.SendAsync(new SFriendAddReq
                        {
                            Name = character.Name,
                            Result = 1,
                            State = (byte)Program.ServerCode
                        }).Wait();
                    }
                }
            }
        }

        public void Update()
        {
            if (!AddSet)
                return;

            using (var db = new GameContext())
            {
                var result = from f in db.Friends
                             from c in db.Characters
                             where c.CharacterId == f.FriendId && c.CharacterId == character.Id
                             select new SFriendAddSin { Name = c.Name };

                foreach (var msg in result)
                    character.Player.Session.SendAsync(result).Wait();
            }
        }
    }
}
