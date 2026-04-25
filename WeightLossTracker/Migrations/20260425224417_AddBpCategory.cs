using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeightLossTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddBpCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BloodPressureEntries",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "BloodPressureEntries");
        }
    }
}
