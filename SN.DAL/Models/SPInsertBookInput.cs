using System.Collections.Generic;

namespace SN.DAL.Models
{
    public class SPInsertBookInput
    {
        public string BOOK_TITLE { get; set; }
        public List<string> AUTHORS_NAMES { get; set; } 
    }
}
