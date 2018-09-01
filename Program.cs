using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectionBot
{
    public class Program
    {
        static void Main()
            => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        public static Dictionary<ulong, string[]> userInfo = new Dictionary<ulong, string[]>();

        public async Task StartAsync()
        {
            if (isConsole)
            {
                Console.Title = "UCSGG Election Bot";
            }

            bool isRunning = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);
                isRunning = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Count() > 1;

                if (isRunning)
                {
                    MessageBox.Show("Program is already running", "UCSGG Election Bot");
                    return;
                }
            }

            _config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };

            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await _client.StartAsync();

            await _client.SetGameAsync("with election stuff.");

            _handler = new CommandHandler(_client);

            userInfo = await Files.FileToDictArray("voterids.txt", "voterkeys.txt");

            if (isConsole)
            {
                Console.WriteLine("The Election Bot is all set.");
            }

            await Task.Delay(-1);
        }
    }
}
