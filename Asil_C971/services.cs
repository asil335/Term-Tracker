using SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Asil_C971;

namespace Asil_C971
{
    public class dbService
    {
        private SQLiteAsyncConnection _connection;
        private readonly string _databasePath;

        public dbService()
        {
            _databasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "AsilC971.db3");
        }

        public async Task init()
        {
            if (_connection != null) return;

            _connection = new SQLiteAsyncConnection(_databasePath);
            await _connection.CreateTableAsync<term>();
        }

        public async Task<List<term>> getTerms()
        {
            await init();
            return await _connection.Table<term>().ToListAsync();
        }

        public async Task<int> saveTerm(term term)
        {
            await init();
            if (term.Id != 0)
                return await _connection.UpdateAsync(term);
            else
                return await _connection.InsertAsync(term);
        }

        public async Task<int> delTerm(term term)
        {
            await init();
            return await _connection.DeleteAsync(term);
        }
    }
}