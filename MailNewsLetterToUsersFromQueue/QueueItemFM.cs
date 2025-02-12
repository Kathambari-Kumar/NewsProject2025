using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailNewsLetterToUsersFromQueue
{
    public class QueueItemFM
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<ArticleFM> NewsLetterArticles { get; set; } = new List<ArticleFM>();
    }
}
