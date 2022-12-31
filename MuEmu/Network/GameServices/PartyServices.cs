using MU.Network.Game;
using MU.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebZen.Handlers;

namespace MuEmu.Network.GameServices
{
    public partial class GameServices : MessageHandler
    {
        [MessageHandler(typeof(CPartyRequest))]
        public async Task CPartyRequest(GSSession session, CPartyRequest message)
        {
            var trg = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);
            if (trg == null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.PlayerOffline));
                return;
            }

            if (trg.Player.Character.Party != null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.InAnotherParty));
                return;
            }

            var party = session.Player.Character.Party;

            if ((party != null && party.Master != session.Player) || session.Player.Window != null || trg.Player.Window != null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.Fail));
                return;
            }

            if (Math.Abs(session.Player.Character.Level - trg.Player.Character.Level) > PartyManager.MaxLevelDiff)
            {
                await session.SendAsync(new SPartyResult(PartyResults.RestrictedLevel));
                return;
            }

            message.Number = (ushort)session.ID;
            await trg.SendAsync(message);
        }

        [MessageHandler(typeof(CPartyRequestResult))]
        public async Task CPartyRequestResult(GSSession session, CPartyRequestResult message)
        {
            var trg = Program.server.Clients.FirstOrDefault(x => x.ID == message.Number);
            if (trg == null)
            {
                await session.SendAsync(new SPartyResult(PartyResults.Fail));
                return;
            }
            PartyManager.CreateLink(trg.Player, session.Player);
        }

        [MessageHandler(typeof(CPartyList))]
        public async Task CPartyList(GSSession session)
        {
            var party = session.Player.Character.Party;
            PartyManager.SendAll(party);
        }

        [MessageHandler(typeof(CPartyDelUser))]
        public void CPartyDelUser(GSSession session, CPartyDelUser message)
        {
            var party = session.Player.Character.Party;
            if (party == null)
            {
                return;
            }

            var memb = party.Members.ElementAtOrDefault(message.Index);
            if (memb == null)
            {
                return;
            }

            if (memb != party.Master && memb != session.Player)
            {
                return;
            }

            PartyManager.Remove(memb);
        }

        [MessageHandler(typeof(CPartyMRegister))]
        public async Task CPartyMRegister(GSSession session, CPartyMRegister message)
        {
            var result = new SPartyMRegister();

            var plr = session.Player;
            var @char = plr.Character;
            if (@char.Party != null)
            {
                if (@char.Party.Master != plr)
                {
                    result.Result = -3;
                    await session.SendAsync(result);
                    return;
                }
            }

            if (PartyManager.ExistsMatching(plr))
            {
                result.Result = -2;
                await session.SendAsync(result);
                return;
            }

            PartyManager.CreateMatching(
                plr,
                message.Text,
                message.NeedPassword ? message.Password : "",
                message.AutAccept,
                message.MinLevel,
                message.MaxLevel,
                message.EnergyElf
                );

            await session.SendAsync(result);
        }

        [MessageHandler(typeof(CPartyMSearch))]
        public async Task CPartyMSearch(GSSession session, CPartyMSearch message)
        {
            var msg = new SPartyMSearch();
            msg.Page = message.Page;
            msg.Result = 0;

            var matchings = PartyManager.GetMatchings();

            msg.MaxPage = (uint)Math.Ceiling(matchings.Count / 6.0f);
            var matchingPage = matchings.Skip((int)((message.Page - 1) * 6)).Take(6);
            msg.List = matchingPage.Select(x => new PartyMSearchDto
            {
                MaxLevel = x.MaxLevel,
                MinLevel = x.MinLevel,
                Text = x.Text,
                Password = !string.IsNullOrEmpty(x.Password),
                Members = x.Player.Character.Party?.Members.Select(y => new PartyMSearchMemberDto
                {
                    Name = y.Character.Name,
                    Level = y.Character.Level,
                    Race = (ushort)y.Character.Class
                }).ToArray() ?? new PartyMSearchMemberDto[] { new PartyMSearchMemberDto { Name = x.Player.Character.Name, Level = x.Player.Character.Level } },
                Count = (byte)(x.Player.Character.Party?.Members.Count() ?? 1)
            }).ToArray();

            msg.Count = (uint)matchingPage.Count();
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CPartyMJoin))]
        public async Task CPartyMJoin(GSSession session, CPartyMJoin message)
        {
            var msg = new SPartyMJoin();

            if (PartyManager.ExistsMatching(session.Player))
            {
                msg.Result = -4;
                goto sendmsg;
            }

            if (session.Player.Character.Party != null)
            {
                msg.Result = -6;
                goto sendmsg;
            }

            var matchings = PartyManager.GetMatchings();
            PartyMatching matching = null;
            if (message.Random)
            {
                var pool = matchings.Where(x => x.CanJoin(session.Player.Character));
                if (pool.Any())
                {
                    var matchNum = Program.RandomProvider(pool.Count());
                    matching = pool.ElementAtOrDefault(matchNum);
                }
                if (matching == null)
                {
                    msg.Result = -3;
                    goto sendmsg;
                }
            }
            else
            {
                matching = matchings.FirstOrDefault(x => x.Player.Character.Name == message.Leader);
                if (matching == null)
                {
                    msg.Result = -2;
                    goto sendmsg;
                }
            }

            msg.Result = matching.TryJoin(session.Player, message.Password);
            msg.Text = matching.Text;
            msg.Gens = 0;
            msg.Name = matching.Player.Character.Name;
            msg.UsePassword = !string.IsNullOrEmpty(matching.Password);

        sendmsg:
            await session.SendAsync(msg);
        }

        [MessageHandler(typeof(CPartyMJoinList))]
        public async Task CPartyMJoinList(GSSession session)
        {
            var matching = PartyManager.GetMatchings().First(x => x.Player == session.Player);
            var msg = new SPartyMJoinList();

            msg.List = matching.Waiting.Select(x => new PartyMJoinListDto
            {
                Name = x.Character.Name,
                Level = x.Character.Level,
                Data = 0,
                Race = (byte)x.Character.BaseClass
            }).ToArray();
            msg.Count = msg.List.Count();

            await session.SendAsync(msg);
        }
        [MessageHandler(typeof(CPartyMAccept))]
        public async Task CPartyMAccept(GSSession session, CPartyMAccept message)
        {
            var matching = PartyManager.GetMatchings().First(x => x.Player == session.Player);
            var applicant = matching.Waiting.First(x => x.Character.Name == message.Applicant);
            matching.Waiting.Remove(applicant);

            if (message.Accept)
            {
                PartyManager.CreateLink(matching.Player, applicant);
            }
            else
            {

            }
            await session.SendAsync(new SPartyMJoinNotify());
        }

        [MessageHandler(typeof(CPartyMCancel))]
        public async Task CPartyMCancel(GSSession session, CPartyMCancel message)
        {
            var msg = new SPartyMCancel();
            msg.Result = PartyManager.CancelMatching(session.Player);
            msg.Type = message.Type;
            await session.SendAsync(msg);
        }
    }
}
