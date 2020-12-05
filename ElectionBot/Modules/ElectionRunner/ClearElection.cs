using Discord.Commands;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ElectionBot.DatabaseManager;

namespace ElectionBot.Modules.ElectionRunner
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
                electionDatabase.Voters.RemoveElectionAsync(Context.Guild, isAdmin)
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
    }
}