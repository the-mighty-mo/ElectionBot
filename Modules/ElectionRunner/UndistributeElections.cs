using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectionBot.Modules.ElectionRunner
{
    public class UndistributeElections : ModuleBase<SocketCommandContext>
    {
        [Command("undist-elections")]
        [Alias("undistelections", "undistribute-elections")]
        [RequireOwner]
        public async Task UndistributeElectionsAsync()
        {
            var categories = Context.Guild.CategoryChannels.Where(x => x.Name.ToLower().StartsWith("voter group"));

            List<Task> cmds = new List<Task>();
            foreach (SocketTextChannel channel in Context.Guild.TextChannels.Where(x => categories.Contains(x.Category)))
            {
                cmds.Add(channel.DeleteAsync());
            }
            await Task.WhenAll(cmds);

            cmds.Clear();
            foreach (SocketCategoryChannel category in categories)
            {
                cmds.Add(category.DeleteAsync());
            }
            await Task.WhenAll(cmds);

            await Context.Channel.SendMessageAsync("The elections have been undistributed.");
        }
    }
}
