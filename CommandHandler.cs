using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElectionBot
{
    public class CommandHandler
    {
        public const char prefix = '\\';

        private DiscordSocketClient _client;
        private CommandService _service;
        private IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;

            _services = services;

            _service = new CommandService();
            _service.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            _client.MessageReceived += HandleCommandAsync;
            _client.Connected += SendConnectMessage;
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync("UCD Election Bot is Online");
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            SocketUserMessage msg = m as SocketUserMessage;
            if (msg == null)
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if(!context.User.IsBot)
            {
                if (msg.HasCharPrefix(prefix, ref argPos))
                {
                    var result = await _service.ExecuteAsync(context, argPos, _services);

                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync("Error: " + result.ErrorReason);
                    }
                }
            }
        }
    }
}
