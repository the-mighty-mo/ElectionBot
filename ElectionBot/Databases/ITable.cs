using System.Threading.Tasks;

namespace ElectionBot.Databases
{
    interface ITable
    {
        public Task InitAsync();
    }
}
