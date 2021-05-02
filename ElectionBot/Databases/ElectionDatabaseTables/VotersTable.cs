using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectionBot.Databases.ElectionDatabaseTables
{
    public class VotersTable : ITable
    {
        private readonly SqliteConnection connection;

        public VotersTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Voters (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, voter_key TEXT NOT NULL, weight INTEGER NOT NULL, UNIQUE (guild_id, user_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task SetVotersAsync(IEnumerable<(SocketGuildUser user, int voterKey, int weight)> userDatas)
        {
            string update = "UPDATE Voters SET voter_key = @voter_key, weight = @weight WHERE guild_id = @guild_id AND user_id = @user_id;";
            string insert = "INSERT INTO Voters (guild_id, user_id, voter_key, weight) SELECT @guild_id, @user_id, @voter_key, @weight WHERE (SELECT Changes() = 0);";

            List<Task> cmds = new();
            await Task.Yield();
            foreach ((SocketGuildUser user, int voterKey, int weight) in userDatas)
            {
                using SqliteCommand cmd = new(update + insert, connection);
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                cmd.Parameters.AddWithValue("@voter_key", voterKey.ToString());
                cmd.Parameters.AddWithValue("@weight", weight);

                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task<IEnumerable<(SocketGuildUser user, int voterKey, int weight)>> GetVotersAsync(SocketGuild g)
        {
            List<(SocketGuildUser user, int voterKey, int weight)> userDatas = new();

            string getData = "SELECT user_id, voter_key, weight FROM Voters WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getData, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                SocketGuildUser user;
                if (!ulong.TryParse(reader["user_id"].ToString(), out ulong userID) || (user = g.GetUser(userID)) == null)
                {
                    continue;
                }
                if (!int.TryParse(reader["voter_key"].ToString(), out int voterKey) || int.TryParse(reader["weight"].ToString(), out int weight))
                {
                    continue;
                }
                userDatas.Add((user, voterKey, weight));
            }
            await reader.CloseAsync();

            return userDatas;
        }

        public async Task RemoveElectionAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Voters WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
