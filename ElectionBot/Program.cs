using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ElectionBot
{
    public class Program
    {
        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly Random rng = new Random();

        public static readonly SqliteConnection cnVoters = new SqliteConnection("Filename=Voters.db");
        public static readonly SqliteConnection cnElection = new SqliteConnection("Filename=Election.db");

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

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
                    if (isConsole)
                    {
                        Console.WriteLine("Program is already running");
                        await Task.WhenAny(
                            Task.Run(() => Console.ReadLine()),
                            Task.Delay(5000)
                        );
                    }
                    return;
                }
            }

            List<Task> initSqlite = new List<Task>()
            {
                InitVotersSqlite(),
                InitElectionSqlite()
            };

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
            Task initCmd = _handler.InitCommandsAsync();

            await Task.WhenAll(initSqlite);

            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            await initCmd;
            await Task.Delay(-1);
        }

        private static async Task InitVotersSqlite()
        {
            await cnVoters.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Voters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AdminVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Votes (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, cand_id TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModVotes (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, cand_id TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AdminVotes (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, cand_id TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnVoters))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        private static async Task InitElectionSqlite()
        {
            await cnElection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, voter_key TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnElection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AdminVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, voter_key TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", cnElection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
        }
    }
}
