namespace ChatView_API.Models.Db_Models
{
    public class DbVideo
    {
        public int Id { get; set; }
        public string YoutubeUrl { get; set; }
        public byte[] Mp4Bytes { get; set; }
    }
}
