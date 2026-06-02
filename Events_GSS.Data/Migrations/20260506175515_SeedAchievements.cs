using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatAndEvents.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "AchievementId", "Description", "IsUnlocked", "Name" },
                values: new object[,]
                {
                    { 1, "Attend your first event.", false, "First Steps" },
                    { 2, "Create 3 events.", false, "Proper Host" },
                    { 3, "Approve 25 quest submissions.", false, "Quest Solver" },
                    { 4, "Add 50 memories with photos.", false, "Memory Keeper" },
                    { 5, "Send 100 discussion messages.", false, "Social Butterfly" },
                    { 6, "Attend 10 events.", false, "Event Veteran" },
                    { 7, "Complete every quest in an event.", false, "Perfectionist" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: 7);
        }
    }
}
