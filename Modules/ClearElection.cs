using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ElectionBot.Modules
{
    public class ClearElection : ModuleBase<SocketCommandContext>
    {
        [Command("clear-election")]
        [Alias("clearelection")]
        [RequireOwner]
        public async Task ClearElectionAsync(string type)
        {
            List<string> adminTypes = new List<string>()
            {
                "admin",
                "a",
                "administrator"
            };
            bool isAdmin = adminTypes.Contains(type);

            await Task.WhenAll
            (
                DeleteFilesAsync(isAdmin),
                RemoveElectionAsync(Context.Guild, isAdmin)
            );
            await Context.Channel.SendMessageAsync("The election has been cleared.");
        }

        private async Task DeleteFilesAsync(bool isAdmin)
        {
            await Task.Yield();
            int i = 1;
            while (File.Exists($"voters{i}-{(isAdmin ? "a" : "m")}-{Context.Guild.Id}.csv"))
            {
                File.Delete($"voters{i}-{(isAdmin ? "a" : "m")}-{Context.Guild.Id}.csv");
                i++;
            }
        }

        public static async Task RemoveElectionAsync(SocketGuild g, bool isAdmin)
        {
            string delete = $"DELETE FROM {(isAdmin ? "AdminVoters" : "ModVoters")} WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnElection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
