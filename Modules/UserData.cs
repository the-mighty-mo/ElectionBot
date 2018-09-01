using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ElectionBot.Modules
{
    public class Manual : ModuleBase<SocketCommandContext>
    {
        [Command("giveme")]
        public async Task GiveMe()
        {
            if (Program.userInfo.ContainsKey(Context.User.Id))
            {
                string userID = Program.userInfo[Context.User.Id][0];
                string userPass = Program.userInfo[Context.User.Id][1];

                await Context.User.SendMessageAsync("**__Moderator Election User Info__**\n" +
                    "**Voter ID:** " + userID + "\n" +
                    "**Voter Key:** " + userPass);
            }
            else
            {
                await Context.Channel.SendMessageAsync("You are not eligible to vote in the Moderator Election.");
            }
        }

        [Command("addvoter")]
        [RequireOwner()]
        public async Task AddUser(string userID, string voterID, string voterKey)
        {
            bool added = true;
            if (Program.userInfo.ContainsKey(Convert.ToUInt64(userID)))
            {
                added = false;
            }

            string[] voterInfo = { voterID, voterKey };
            Program.userInfo.Put(Convert.ToUInt64(userID), voterInfo);

            await Files.DictArrayToFile(Program.userInfo, "voterids.txt", "voterkeys.txt");

            if (added)
            {
                await Context.Channel.SendMessageAsync("Voter Info Added!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Voter Info Updated!");
            }
        }
    }
}
