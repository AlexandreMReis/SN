namespace SN.DAL.Models
{
    public enum LikedRating
    {
        Nothing = 1,
        Little = 2,
        ALot = 3
    }

    public class BOOK
    {
        public int ID { get; set; }
        public string TITLE { get; set; }
    }
}
