using SQLite;
using System;

namespace Asil_C971
{
    public class term
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class course
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int termId { get; set; }
        public string profEmail { get; set; }
        public string ProfName { get; set; }
        public string profPhone { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
    public class test
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime End { get; set; }
        public int classId { get; set; }
        public bool Type { get; set; }
        }

    }