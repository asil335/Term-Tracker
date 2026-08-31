using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Supabase;
using static Supabase.Postgrest.Constants;

namespace Asil_C971
{
    public class dbService
    {
        private Client _client;

        public async Task init()
        {
            if (_client != null) return;
            var options = new SupabaseOptions { AutoConnectRealtime = false };
            _client = new Client(Secrets.SupabaseUrl, Secrets.SupabaseKey, options);
            await _client.InitializeAsync();

            var statuses = await _client.From<Statuses>().Get();
            if (statuses.Models.Count == 0)
            {
                var defaultStatuses = new List<Statuses>
                {
                    new Statuses { StatusType = "In Progress" },
                    new Statuses { StatusType = "Completed" },
                    new Statuses { StatusType = "Dropped" },
                    new Statuses { StatusType = "Plan to Take" }
                };
                foreach (var s in defaultStatuses)
                {
                    await _client.From<Statuses>().Insert(s);
                }
            }
        }
        public async Task<List<term>> getTerms()
        {
            await init();
            var result = await _client.From<term>().Order(t => t.Start, Ordering.Ascending).Get();
            return result.Models;
        }

        public async Task<int> saveTerm(term term)
        {
            await init();
            if (term.Id != 0)
            {
                await _client.From<term>().Where(t => t.Id == term.Id).Update(term);
                return term.Id;
            }
            var ins = await _client.From<term>().Insert(term);
            var row = ins.Models.FirstOrDefault();
            if (row != null) term.Id = row.Id;
            return term.Id;
        }

        public async Task<int> delTerm(term term)
        {
            await init();
            await _client.From<term>().Where(t => t.Id == term.Id).Delete();
            return term.Id;
        }
        public async Task<List<course>> getCourses()
        {
            await init();
            var result = await _client.From<course>().Get();
            return result.Models;
        }

        public async Task<int> saveCourse(course course)
        {
            await init();
            if (course.Id != 0)
            {
                await _client.From<course>().Where(c => c.Id == course.Id).Update(course);
                return course.Id;
            }
            var inserted = await _client.From<course>().Insert(course);
            var row = inserted.Models.FirstOrDefault();
            if (row != null) course.Id = row.Id;
            return course.Id;
        }

        public async Task<int> delCourse(course course)
        {
            await init();
            await _client.From<course>().Where(c => c.Id == course.Id).Delete();
            return course.Id;
        }
        public async Task<List<course>> getTermCourses(int termId)
        {
            await init();
            var result = await _client.From<course>().Where(c => c.termId == termId).Get();
            return result.Models;
        }
        public async Task<List<test>> getTests(int courseId)
        {
            await init();
            var result = await _client.From<test>().Where(t => t.classId == courseId).Get();
            return result.Models;
        }

        public async Task<int> saveTests(test t)
        {
            await init();
            if (t.Id != 0)
            {
                await _client.From<test>().Where(x => x.Id == t.Id).Update(t);
                return t.Id;
            }
            var inserted = await _client.From<test>().Insert(t);
            var row = inserted.Models.FirstOrDefault();
            if (row != null) t.Id = row.Id;
            return t.Id;
        }

        public async Task<int> delTests(test t)
        {
            await init();
            await _client.From<test>().Where(x => x.Id == t.Id).Delete();
            return t.Id;
        }

        public async Task<List<Statuses>> getStatuses()
        {
            await init();
            var result = await _client.From<Statuses>().Get();
            return result.Models;
        }
        public async Task<List<test>> getTests()
        {
            await init();
            var result = await _client.From<test>().Get();
            return result.Models;
        }
    }
}