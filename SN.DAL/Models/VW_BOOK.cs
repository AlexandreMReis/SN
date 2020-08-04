using System.Collections.Generic;

namespace SN.DAL.Models
{
    public class VW_BOOK
    {
        public int ID { get; set; }
        public string TITLE { get; set; }
        public List<string> AUTHORS { get; set; } 
    }
}
