using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ElectionBot.DatabaseManager;

namespace ElectionBot.Modules.Votes
{
    public class InitVote : ModuleBase<SocketCommandContext>
    {
        [Command("init-vote")]
        [Alias("initvote")]
        [RequireOwner]
        public async Task InitVoteAsync(string type = null)
        {
            List<string> adminTypes = new List<string>()
            {
                "admin",
                "a",
                "administrator"
            };
            (bool isAdmin, bool isMod) voteType = (adminTypes.Contains(type), type != null);

            //await ClearElection.RemoveElectionAsync(Context.Guild, isAdmin);
            await votersDatabase.Voters.SetVotersAsync(GetWeights(voteType), voteType);
            await Context.Channel.SendMessageAsync("Voter IDs and Keys have been recorded.");
        }

        private IEnumerable<(SocketGuildUser user, int weight)> GetWeights((bool isAdmin, bool isMod) voteTypes)
        {
            int weight;

            var roles = Context.Guild.Roles;
            SocketRole adminRole = roles.FirstOrDefault(x => x.Name == "A");
            SocketRole modRole = roles.FirstOrDefault(x => x.Name == "M");
            SocketRole adminCand = roles.FirstOrDefault(x => x.Name == "Admin Candidate");
            SocketRole modCand = roles.FirstOrDefault(x => x.Name == "Moderator Candidate");

            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.IsBot && x != Context.Guild.Owner))
            {
                weight = 1;
                if (user.Roles.Contains(adminRole))
                {
                    weight = voteTypes.isAdmin
                        ? user.Roles.Contains(adminCand) ? 1 : 3
                        : voteTypes.isMod ? 2 : 1;
                }
                else if (user.Roles.Contains(modRole))
                {
                    weight = (voteTypes.isAdmin && user.Roles.Contains(adminCand)) || !voteTypes.isMod ? 1 : 2;
                }

                yield return (user, weight);
            }
        }
    }
}
