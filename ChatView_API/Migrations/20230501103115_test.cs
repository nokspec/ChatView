using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatView_API.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Videos",
                columns: new[] { "Id", "Mp4Bytes", "YoutubeUrl" },
                values: new object[] { 99, new byte[] { 16, 32, 48, 64 }, "Seed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Videos",
                keyColumn: "Id",
                keyValue: 99);
        }
    }
}
