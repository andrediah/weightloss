using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeightLossTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodPressureEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloodPressureEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Systolic = table.Column<int>(type: "INTEGER", nullable: false),
                    Diastolic = table.Column<int>(type: "INTEGER", nullable: false),
                    Pulse = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodPressureEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodPressureEntries_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodPressureEntries_UserProfileId",
                table: "BloodPressureEntries",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodPressureEntries");
        }
    }
}
