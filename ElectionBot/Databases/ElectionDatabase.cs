using ElectionBot.Databases.ElectionDatabaseTables;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectionBot.Databases
{
    public class ElectionDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Election.db");
        private readonly Dictionary<System.Type, ITable> tables = new Dictionary<System.Type, ITable>();

        public VotersTable Voters => tables[typeof(VotersTable)] as VotersTable;

        public ElectionDatabase()
        {
            tables.Add(typeof(VotersTable), new VotersTable(connection));
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();
            IEnumerable<Task> GetTableInits()
            {
                foreach (var table in tables.Values)
                {
                    yield return table.InitAsync();
                }
            }
            await Task.WhenAll(GetTableInits());
        }

        public async Task CloseAsync() => await connection.CloseAsync();
    }
}