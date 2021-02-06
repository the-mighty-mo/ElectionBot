using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ElectionBot.DatabaseManager;

namespace ElectionBot.Modules.ElectionRunner
{
    public class InitElection : ModuleBase<SocketCommandContext>
    {
        [Command("init-election")]
        [Alias("initelection")]
        [RequireOwner]
        public async Task InitElectionAsync()
        {
            await electionDatabase.Voters.RemoveElectionAsync(Context.Guild);
            await electionDatabase.Voters.SetVotersAsync(GetWeights());
            await Context.Channel.SendMessageAsync("Voter IDs and Keys have been recorded.");
        }

        private IEnumerable<(SocketGuildUser user, int voterKey, int weight)> GetWeights()
        {
            int weight;

            var roles = Context.Guild.Roles;
            SocketRole acceptedRole = roles.FirstOrDefault(x => x.Name.Contains("Accepted"));

            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.IsBot && x != Context.Guild.Owner && x.Roles.Contains(acceptedRole)))
            {
                weight = 1;
                yield return (user, Program.rng.Next(100000, 1000000), weight);
            }
        }
    }
}