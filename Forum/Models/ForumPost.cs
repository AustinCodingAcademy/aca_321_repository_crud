using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models
{
    public class ForumPost
    {
        public int? Id { get; set; }

        public DateTime Date { get; set; }

        public string User { get; set; }

        public string Message { get; set; }
    }
}
