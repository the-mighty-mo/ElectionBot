using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ElectionBot.Modules
{
    public class Manual : ModuleBase<SocketCommandContext>
    {
        [Command("distribute")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireOwner()]
        public async Task Distribute()
        {
            OverwritePermissions allow = new OverwritePermissions().Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny);
            OverwritePermissions deny = new OverwritePermissions().Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny);

            int i = 20, j = 1;

            ulong id = 0;
            Action<TextChannelProperties> action = new Action<TextChannelProperties>(x =>
            {
                x.CategoryId = id;
            });

            foreach (string user in Program.userData.Keys)
            {

                if (i >= 20)
                {
                    id = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower() == "voter group " + j) == null
                        ? (await Context.Guild.CreateCategoryChannelAsync("voter group " + j)).Id
                        : Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower() == "voter group " + j).Id;
                    
                    action = new Action<TextChannelProperties>(x =>
                    {
                        x.CategoryId = id;
                    });

                    j++;
                    i = 0;
                }
                if (Context.Guild.Channels.FirstOrDefault(x => x.Name == Context.Guild.Users.FirstOrDefault(y => y.Username == user).Discriminator) == null)
                {
                    RestTextChannel chan = await Context.Guild.CreateTextChannelAsync(Context.Guild.Users.FirstOrDefault(x => x.Username == user).Discriminator, action);
                    await chan.AddPermissionOverwriteAsync(Context.Guild.Users.FirstOrDefault(x => x.Username == user), allow);
                    await chan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, deny);

                    await chan.SendMessageAsync("Voter ID: " + user + "\n" +
                        "Voter Key: " + Program.userData[user][0]);
                }

                i++;
            }

            await Context.Channel.SendMessageAsync("Voter IDs have been distributed.");
        }

        [Command("undistribute")]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireOwner()]
        public async Task Undistribute()
        {
            var categories = Context.Guild.CategoryChannels.Where(x => x.Name.ToLower().StartsWith("voter group"));
            foreach (SocketTextChannel channel in Context.Guild.TextChannels)
            {
                foreach (SocketCategoryChannel category in categories)
                {
                    if (channel.CategoryId == category.Id)
                    {
                        await channel.DeleteAsync();
                    }
                }
            }
            foreach (SocketCategoryChannel category in categories)
            {
                await category.DeleteAsync();
            }

            await Context.Channel.SendMessageAsync("Voter IDs have been purged.");
        }

        [Command("pdistribute")]
        public async Task DistributePM(string type = "moderator")
        {
            type = type.ElementAt(0).ToString().ToUpper() + type.Substring(1).ToLower();
            int i = 0, j = 1;

            foreach (string user in Program.userData.Keys)
            {
                if (i >= 20)
                {
                    j++;
                    i = 0;
                }

                await Context.Guild.Users.FirstOrDefault(x => x.Username == user).SendMessageAsync(
                    "__Voter Info for the Upcoming UCD " + type + " Election__\n" +
                    "Voting Group " + j.ToString() + "\n" +
                    "Voter ID: " + user + "\n" +
                    "Voter Key: " + Program.userData[user][0]);

                i++;
            }

            await Context.Channel.SendMessageAsync("Voter IDs have been distributed via PM.");
        }

        [Command("voters")]
        [RequireOwner()]
        public async Task AddVoters(string type = "mod")
        {
            Dictionary<string, List<string>> userInfo = new Dictionary<string, List<string>>();
            Random rng = new Random();
            int weight;

            foreach (SocketGuildUser user in Context.Guild.Users.Where(x => !x.IsBot))
            {
                weight = 1;

                SocketRole admRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "A");
                SocketRole modRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "M");
                SocketRole admCand = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admin Candidate");
                SocketRole modCand = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Moderator Candidate");

                if (user.Roles.Contains(admRole))
                {
                    weight = (type == "admin") ? (user.Roles.Contains(admCand) ? 1 : 3) : 2;
                }
                else if (user.Roles.Contains(modRole))
                {
                    weight = (type == "admin" && user.Roles.Contains(admCand)) ? 1 : 2;
                }

                List<string> userValues = new List<string>
                {
                    rng.Next(10000, 99999).ToString(),
                    weight.ToString()
                };

                userInfo.Add(user.Username, userValues);
            }
            Program.userData = userInfo;

            string path = "voters1.csv";

            int i = 0;
            int j = 1;

            File.WriteAllText(path, "name,voter_identifier,voter_key,email,vote_weight");
            foreach (string key in userInfo.Keys)
            {
                if (i >= 20)
                {
                    j++;
                    path = "voters" + j.ToString() + ".csv";
                    File.WriteAllText(path, "name,voter_identifier,voter_key,email,vote_weight");

                    i = 0;
                }

                File.AppendAllText(path, '\n' + key + "," + key + "," + userInfo[key][0] + ",," + userInfo[key][1]);
                i++;
            }

            await Context.Channel.SendMessageAsync("Voter IDs and Keys saved to `voters.csv` files.");
        }
    }
}
