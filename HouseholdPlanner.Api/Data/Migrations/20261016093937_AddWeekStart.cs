using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdPlanner.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeekStartsOn",
                table: "Households",
                type: "integer",
                nullable: false,
                // Existing households start their weeks on Monday, like new ones.
                defaultValue: (int)DayOfWeek.Monday);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeekStartsOn",
                table: "Households");
        }
    }
}
