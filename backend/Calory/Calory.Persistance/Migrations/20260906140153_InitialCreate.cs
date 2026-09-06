using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calory.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FoodEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FoodName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyCalorieTarget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ProteinTarget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CarbTarget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FatTarget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    WeightTarget = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodNutrition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Calories = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ProteinG = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CarbohydratesG = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FatG = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FiberG = table.Column<decimal>(type: "numeric", nullable: false),
                    SugarG = table.Column<decimal>(type: "numeric", nullable: false),
                    SodiumMg = table.Column<decimal>(type: "numeric", nullable: false),
                    CalciumMg = table.Column<decimal>(type: "numeric", nullable: false),
                    IronMg = table.Column<decimal>(type: "numeric", nullable: false),
                    MagnesiumMg = table.Column<decimal>(type: "numeric", nullable: false),
                    PotassiumMg = table.Column<decimal>(type: "numeric", nullable: false),
                    ZincMg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminAMcg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminB1Mg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminB2Mg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminB3Mg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminB6Mg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminB12Mcg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminCMg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminDMcg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminEMg = table.Column<decimal>(type: "numeric", nullable: false),
                    VitaminKMcg = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodNutrition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodNutrition_FoodEntries_FoodEntryId",
                        column: x => x.FoodEntryId,
                        principalTable: "FoodEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodEntries_UserId_ConsumedAt",
                table: "FoodEntries",
                columns: new[] { "UserId", "ConsumedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FoodNutrition_FoodEntryId",
                table: "FoodNutrition",
                column: "FoodEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthGoals_UserId_IsActive",
                table: "HealthGoals",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodNutrition");

            migrationBuilder.DropTable(
                name: "HealthGoals");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "FoodEntries");
        }
    }
}
