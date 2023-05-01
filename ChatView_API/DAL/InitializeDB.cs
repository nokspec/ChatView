using ChatView_API.Models.Db_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatView_API.DAL
{
    public class InitializeDB
    {
        private readonly ModelBuilder _modelBuilder;

        public InitializeDB(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        public void Seed()
        {
            var bytes = new byte[] { 0x10, 0x20, 0x30, 0x40 };

            _modelBuilder.Entity<DbVideo>().HasData(
                new DbVideo
                {
                    Id = 99,
                    Mp4Bytes = bytes,
                    YoutubeUrl = "Seed"
                });


        }
    }
}
