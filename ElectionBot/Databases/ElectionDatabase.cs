using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectionBot.Databases
{
    public class ElectionDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Election.db");

        public readonly VotersTable Voters;

        public ElectionDatabase()
        {
            Voters = new VotersTable(connection);
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, voter_key TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AdminVoters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, voter_key TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await connection.CloseAsync();

        public class VotersTable
        {
            private readonly SqliteConnection connection;

            public VotersTable(SqliteConnection connection) => this.connection = connection;

            public async Task SetVotersAsync(IEnumerable<(SocketGuildUser user, int voterKey, int weight)> userDatas, bool isAdmin)
            {
                string update = $"UPDATE {(isAdmin ? "Admin" : "Mod")}Voters SET voter_key = @voter_key, weight = @weight WHERE guild_id = @guild_id AND user_id = @user_id;";
                string insert = $"INSERT INTO {(isAdmin ? "Admin" : "Mod")}Voters (guild_id, user_id, voter_key, weight) SELECT @guild_id, @user_id, @voter_key, @weight WHERE (SELECT Changes() = 0);";

                List<Task> cmds = new List<Task>();
                await Task.Yield();
                foreach ((SocketGuildUser user, int voterKey, int weight) in userDatas)
                {
                    using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                    {
                        cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                        cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                        cmd.Parameters.AddWithValue("@voter_key", voterKey.ToString());
                        cmd.Parameters.AddWithValue("@weight", weight);

                        cmds.Add(cmd.ExecuteNonQueryAsync());
                    }
                }

                await Task.WhenAll(cmds);
            }

            public async Task<IEnumerable<(SocketGuildUser user, int voterKey, int weight)>> GetVotersAsync(SocketGuild g, bool isAdmin)
            {
                List<(SocketGuildUser user, int voterKey, int weight)> userDatas = new List<(SocketGuildUser user, int voterKey, int weight)>();

                string getData = $"SELECT user_id, voter_key, weight FROM {(isAdmin ? "Admin" : "Mod")}Voters WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getData, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        SocketGuildUser user;
                        if (!ulong.TryParse(reader["user_id"].ToString(), out ulong userID) || (user = g.GetUser(userID)) == null)
                        {
                            continue;
                        }
                        userDatas.Add((user, int.Parse(reader["voter_key"].ToString()), int.Parse(reader["weight"].ToString())));
                    }
                    await reader.CloseAsync();
                }

                return userDatas;
            }

            public async Task RemoveElectionAsync(SocketGuild g, bool isAdmin)
            {
                string delete = $"DELETE FROM {(isAdmin ? "Admin" : "Mod")}Voters WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
