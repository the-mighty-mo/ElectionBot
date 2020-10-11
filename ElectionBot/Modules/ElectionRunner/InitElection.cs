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
        public async Task InitElectionAsync(string type)
        {
            List<string> adminTypes = new List<string>()
            {
                "admin",
                "a",
                "administrator"
            };
            bool isAdmin = adminTypes.Contains(type.ToLower());

            await electionDatabase.Voters.RemoveElectionAsync(Context.Guild, isAdmin);
            await electionDatabase.Voters.SetVotersAsync(GetWeights(isAdmin), isAdmin);
            await Context.Channel.SendMessageAsync("Voter IDs and Keys have been recorded.");
        }

        private IEnumerable<(SocketGuildUser user, int voterKey, int weight)> GetWeights(bool isAdmin)
        {
            int weight;

            var roles = Context.Guild.Roles;
            SocketRole acceptedRole = roles.FirstOrDefault(x => x.Name.Contains("Accepted"));
            SocketRole adminRole = roles.FirstOrDefault(x => x.Name == "Adm");
            SocketRole modRole = roles.FirstOrDefault(x => x.Name == "Mod");
            SocketRole adminCand = roles.FirstOrDefault(x => x.Name == "Admin Candidate");

            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.IsBot && x != Context.Guild.Owner && x.Roles.Contains(acceptedRole)))
            {
                weight = 1;
                if (user.Roles.Contains(adminRole))
                {
                    weight = isAdmin
                        ? user.Roles.Contains(adminCand) ? 2 : 3
                        : 2;
                }
                else if (user.Roles.Contains(modRole))
                {
                    weight = 2;
                }

                yield return (user, Program.rng.Next(100000, 999999), weight);
            }
        }
    }
}
