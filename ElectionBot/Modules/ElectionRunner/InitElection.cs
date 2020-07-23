using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            await ClearElection.RemoveElectionAsync(Context.Guild, isAdmin);
            await SetVotersAsync(GetWeights(isAdmin), isAdmin);
            await Context.Channel.SendMessageAsync("Voter IDs and Keys have been recorded.");
        }

        private IEnumerable<(SocketGuildUser user, int voterKey, int weight)> GetWeights(bool isAdmin)
        {
            int weight;

            var roles = Context.Guild.Roles;
            SocketRole acceptedRole = roles.FirstOrDefault(x => x.Name.Contains("Accepted"));
            SocketRole adminRole = roles.FirstOrDefault(x => x.Name == "A");
            SocketRole modRole = roles.FirstOrDefault(x => x.Name == "M");
            SocketRole adminCand = roles.FirstOrDefault(x => x.Name == "Admin Candidate");

            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.IsBot && x != Context.Guild.Owner && x.Roles.Contains(acceptedRole)))
            {
                weight = 1;
                if (user.Roles.Contains(adminRole))
                {
                    weight = isAdmin
                        ? user.Roles.Contains(adminCand) ? 1 : 3
                        : 2;
                }
                else if (user.Roles.Contains(modRole))
                {
                    weight = isAdmin && user.Roles.Contains(adminCand) ? 1 : 2;
                }

                yield return (user, Program.rng.Next(100000, 999999), weight);
            }
        }

        public static async Task SetVotersAsync(IEnumerable<(SocketGuildUser user, int voterKey, int weight)> userDatas, bool isAdmin)
        {
            string update = $"UPDATE {(isAdmin ? "Admin" : "Mod")}Voters SET voter_key = @voter_key, weight = @weight WHERE guild_id = @guild_id AND user_id = @user_id;";
            string insert = $"INSERT INTO {(isAdmin ? "Admin" : "Mod")}Voters (guild_id, user_id, voter_key, weight) SELECT @guild_id, @user_id, @voter_key, @weight WHERE (SELECT Changes() = 0);";

            List<Task> cmds = new List<Task>();
            await Task.Yield();
            foreach ((SocketGuildUser user, int voterKey, int weight) in userDatas)
            {
                using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnElection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                    cmd.Parameters.AddWithValue("@voter_key", voterKey.ToString());
                    cmd.Parameters.AddWithValue("@weight", weight);

                    cmds.Add(cmd.ExecuteNonQueryAsync());
                }
            }

            await Task.WhenAll(cmds);
        }
    }
}
