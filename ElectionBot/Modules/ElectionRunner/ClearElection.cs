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
        public async Task ClearElectionAsync()
        {
            await Task.WhenAll
            (
                DeleteFilesAsync(),
                electionDatabase.Voters.RemoveElectionAsync(Context.Guild)
            );
            await Context.Channel.SendMessageAsync("The election has been cleared.");
        }

        private async Task DeleteFilesAsync()
        {
            await Task.Yield();
            int i = 1;
            while (File.Exists($"voters{i}-{Context.Guild.Id}.csv"))
            {
                File.Delete($"voters{i}-{Context.Guild.Id}.csv");
                i++;
            }
        }
    }
}