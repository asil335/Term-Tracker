using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Asil_C971
{
    public abstract class item : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("start")]
        public DateTime Start { get; set; }
        [Column("end")]
        public DateTime End { get; set; }

        public abstract string sum();
    }

    [Table("term")]
    public class term : item
    {
        public override string sum() => $"Term: {Title}";
    }

    [Table("course")]
    public class course : item
    {
        [Column("termid")]
        public int termId { get; set; }
        [Column("profemail")]
        public string profEmail { get; set; }
        [Column("profname")]
        public string ProfName { get; set; }
        [Column("profphone")]
        public string profPhone { get; set; }
        [Column("status")]
        public string Status { get; set; }
        [Column("notes")]
        public string Notes { get; set; }

        public override string sum() => $"Course: {Title} ({Status})";
    }

    [Table("test")]
    public class test : item
    {
        [Column("classid")]
        public int classId { get; set; }
        [Column("type")]
        public bool Type { get; set; }

        public override string sum() =>
            $"{(Type ? "OA" : "PA")}: {Title}";
    }

    [Table("statuses")]
    public class Statuses : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("statustype")]
        public string StatusType { get; set; }
    }
}