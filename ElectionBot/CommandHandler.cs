using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ElectionBot
{
    public class CommandHandler
    {
        public const char prefix = '\\';

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            _commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            _client.Connected += SendConnectMessage;
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }

            SocketCommandContext Context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            bool isCommand = msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasCharPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _services);
                if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    await Context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }
}
