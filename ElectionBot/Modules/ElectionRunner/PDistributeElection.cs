using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ElectionBot.DatabaseManager;

namespace ElectionBot.Modules.ElectionRunner
{
    public class PDistributeElection : ModuleBase<SocketCommandContext>
    {
        [Command("pdist-election")]
        [Alias("pdistelection", "pdistribute-election")]
        [RequireOwner]
        public async Task PDistributeElectionAsync(string type)
        {
            List<string> adminTypes = new List<string>()
            {
                "admin",
                "a",
                "administrator"
            };
            bool isAdmin = adminTypes.Contains(type);
            type = isAdmin ? "Administrator" : "Moderator";

            int i = 0;
            int j = 1;

            List<Task> cmds = new List<Task>();
            foreach ((SocketUser user, int voterKey, int weight) in await electionDatabase.Voters.GetVotersAsync(Context.Guild, isAdmin))
            {
                if (i >= 20)
                {
                    j++;
                    i = 0;
                }

                cmds.Add(user.SendMessageAsync($"__Voter Info for the Upcoming UCD {type} Election__\n" +
                    $"Voting Group: {j}\n" +
                    $"Voter ID: {user.Username}\n" +
                    $"Voter Key: {voterKey}"));

                i++;
            }
            await Task.WhenAll(cmds);

            await Context.Channel.SendMessageAsync("Voter IDs have been distributed via DMs.");
        }
    }
}