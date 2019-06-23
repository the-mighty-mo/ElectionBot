using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectionBot
{
    public class Program
    {
        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        public static Dictionary<List<string>, List<string>> userData = new Dictionary<List<string>, List<string>>();

        public async Task StartAsync()
        {
            if (isConsole)
            {
                Console.Title = SecurityInfo.botName;
            }

            bool isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);
                isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;

                if (isRunning)
                {
                    MessageBox.Show("Program is already running", SecurityInfo.botName);
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

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

            _handler = new CommandHandler(_client, _services);

            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.csv");
            foreach (string file in files)
            {
                char[] splits = { ',', '\n' };
                string[] datas = File.ReadAllText(file).Split(splits);

                for (int i = 6; i < datas.Length; i++)
                {
                    List<string> userNames = new List<string>
                    {
                        datas[i],
                        datas[i + 2]
                    };
                    List<string> userValues = new List<string>
                    {
                        datas[i + 1],
                        datas[i + 3]
                    };
                    userData.Add(userNames, userValues);

                    i += 4;
                }
            }

            await Task.Delay(-1);
        }
    }
}
