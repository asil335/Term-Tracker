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
            var directory = Path.GetDirectoryName(_databasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _connection = new SQLiteAsyncConnection(_databasePath);
            await _connection.CreateTableAsync<term>();
            await _connection.CreateTableAsync<course>();
            await _connection.CreateTableAsync<test>();
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
        public async Task<List<course>> getCourses()
        {
            await init();
            return await _connection.Table<course>().ToListAsync();
        }

        public async Task<int> saveCourse(course course)
        {
            await init();
            if (course.Id != 0)
                return await _connection.UpdateAsync(course);
            else
                return await _connection.InsertAsync(course);
        }

        public async Task<int> delCourse(course course)
        {
            await init();
            return await _connection.DeleteAsync(course);
        }
        public async Task<List<course>> getTermCourses(int termId)
        {
            await init();
            return await _connection.Table<course>().Where(c => c.termId == termId).ToListAsync();
        }
        public async Task<List<test>> getTests(int courseId)
        {
            await init();
            return await _connection.Table<test>().Where(t => t.classId == courseId).ToListAsync();
        }

        public async Task<int> saveTests(test t)
        {
            await init();
            if (t.Id != 0)
                return await _connection.UpdateAsync(t);
            else
                return await _connection.InsertAsync(t);
        }

        public async Task<int> delTests(test t)
        {
            await init();
            return await _connection.DeleteAsync(t);
        }
    }
}
