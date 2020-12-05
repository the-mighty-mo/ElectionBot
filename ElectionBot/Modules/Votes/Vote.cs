using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectionBot.Modules.Votes
{
    public class Vote : ModuleBase<SocketCommandContext>
    {
        [Command("vote")]
        public async Task VoteAsync(string type = null)
        {
            List<string> adminTypes = new List<string>()
            {
                "admin",
                "a",
                "administrator"
            };
            (bool isAdmin, bool isMod) voteType = (adminTypes.Contains(type), type != null);
        }
    }
}