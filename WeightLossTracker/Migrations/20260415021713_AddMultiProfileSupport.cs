using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeightLossTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiProfileSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkoutScheduleDays_DayOfWeek",
                table: "WorkoutScheduleDays");

            migrationBuilder.DropIndex(
                name: "IX_WeightEntries_Date",
                table: "WeightEntries");

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "WorkoutScheduleDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "WeightEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "MealLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "ExerciseSuggestions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "AiPromptLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "UserProfiles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Default");

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 3,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 4,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 5,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 6,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "WorkoutScheduleDays",
                keyColumn: "Id",
                keyValue: 7,
                column: "UserProfileId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutScheduleDays_UserProfileId_DayOfWeek",
                table: "WorkoutScheduleDays",
                columns: new[] { "UserProfileId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_UserProfileId_Date",
                table: "WeightEntries",
                columns: new[] { "UserProfileId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealLogs_UserProfileId",
                table: "MealLogs",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSuggestions_UserProfileId",
                table: "ExerciseSuggestions",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptLogs_UserProfileId",
                table: "AiPromptLogs",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPromptLogs_UserProfiles_UserProfileId",
                table: "AiPromptLogs",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseSuggestions_UserProfiles_UserProfileId",
                table: "ExerciseSuggestions",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealLogs_UserProfiles_UserProfileId",
                table: "MealLogs",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeightEntries_UserProfiles_UserProfileId",
                table: "WeightEntries",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutScheduleDays_UserProfiles_UserProfileId",
                table: "WorkoutScheduleDays",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPromptLogs_UserProfiles_UserProfileId",
                table: "AiPromptLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseSuggestions_UserProfiles_UserProfileId",
                table: "ExerciseSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_MealLogs_UserProfiles_UserProfileId",
                table: "MealLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WeightEntries_UserProfiles_UserProfileId",
                table: "WeightEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutScheduleDays_UserProfiles_UserProfileId",
                table: "WorkoutScheduleDays");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutScheduleDays_UserProfileId_DayOfWeek",
                table: "WorkoutScheduleDays");

            migrationBuilder.DropIndex(
                name: "IX_WeightEntries_UserProfileId_Date",
                table: "WeightEntries");

            migrationBuilder.DropIndex(
                name: "IX_MealLogs_UserProfileId",
                table: "MealLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseSuggestions_UserProfileId",
                table: "ExerciseSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AiPromptLogs_UserProfileId",
                table: "AiPromptLogs");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "WorkoutScheduleDays");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "WeightEntries");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "MealLogs");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "ExerciseSuggestions");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "AiPromptLogs");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutScheduleDays_DayOfWeek",
                table: "WorkoutScheduleDays",
                column: "DayOfWeek",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeightEntries_Date",
                table: "WeightEntries",
                column: "Date",
                unique: true);
        }
    }
}
