using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectionBot.Databases
{
    public class VotersDatabase
    {
        private readonly SqliteConnection cnVoters = new SqliteConnection("Filename=Voters.db");

        public VotersTable Voters;
        public VotesTable Votes;

        public VotersDatabase()
        {
            Voters = new VotersTable(cnVoters);
            Votes = new VotesTable(cnVoters);
        }

        public async Task InitAsync()
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

        public async Task CloseAsync() => await cnVoters.CloseAsync();

        public class VotersTable
        {
            private readonly SqliteConnection cnVoters;

            public VotersTable(SqliteConnection cnVoters) => this.cnVoters = cnVoters;

            public async Task SetVotersAsync(IEnumerable<(SocketGuildUser user, int weight)> userDatas, (bool isAdmin, bool isMod) voteTypes)
            {
                string update = $"UPDATE {(voteTypes.isAdmin ? "Admin" : voteTypes.isMod ? "Mod" : "")}Voters SET weight = @weight WHERE guild_id = @guild_id AND user_id = @user_id;";
                string insert = $"INSERT INTO {(voteTypes.isAdmin ? "Admin" : voteTypes.isMod ? "Mod" : "")}Voters (guild_id, user_id, weight) SELECT @guild_id, @user_id, @weight WHERE (SELECT Changes() = 0);";

                List<Task> cmds = new List<Task>();
                await Task.Yield();
                foreach ((SocketGuildUser user, int weight) in userDatas)
                {
                    using (SqliteCommand cmd = new SqliteCommand(update + insert, cnVoters))
                    {
                        cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                        cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                        cmd.Parameters.AddWithValue("@weight", weight);

                        cmds.Add(cmd.ExecuteNonQueryAsync());
                    }
                }

                await Task.WhenAll(cmds);
            }
        }

        public class VotesTable
        {
            private readonly SqliteConnection cnVoters;

            public VotesTable(SqliteConnection cnVoters) => this.cnVoters = cnVoters;
        }
    }
}
