using System;

namespace MailNewsLetterFromQueueToUsers
{
    public class ArticleFM
    {
        public int Id { get; set; }
        public DateTime DateStamp { get; set; } = DateTime.Now;
        public required string LinkText { get; set; }
        public required string Headline { get; set; }
        public required string ContentSummary { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
