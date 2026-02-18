using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ikigai_api.API.Migrations
{
    /// <inheritdoc />
    public partial class initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IkigaiResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IkigaiResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IkigaiResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoveSessionDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedHobbies = table.Column<string>(type: "text", nullable: false),
                    CustomHobbies = table.Column<string>(type: "text", nullable: false),
                    TopThreeHobbies = table.Column<string>(type: "text", nullable: false),
                    DreamAnswer = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoveSessionDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoveSessionDatas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaidSessionDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EverPaidAnswer = table.Column<string>(type: "text", nullable: false),
                    SelectedJobCards = table.Column<string>(type: "text", nullable: false),
                    MonetizableExperience = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaidSessionDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaidSessionDatas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrologueDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedReasons = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrologueDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrologueDatas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillSessionDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedHardSkills = table.Column<string>(type: "text", nullable: false),
                    CustomHardSkills = table.Column<string>(type: "text", nullable: false),
                    SelectedSoftSkills = table.Column<string>(type: "text", nullable: false),
                    CustomSoftSkills = table.Column<string>(type: "text", nullable: false),
                    SkillsMatchJob = table.Column<string>(type: "text", nullable: false),
                    UseSkillsInNewRole = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillSessionDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkillSessionDatas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorldSessionDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalledUponAnswer = table.Column<string>(type: "text", nullable: false),
                    SelectedGifts = table.Column<string>(type: "text", nullable: false),
                    NoManualChoice = table.Column<string>(type: "text", nullable: false),
                    MismatchChoice = table.Column<string>(type: "text", nullable: false),
                    FutureValueAnswer = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldSessionDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldSessionDatas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IkigaiSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<string>(type: "text", nullable: false),
                    OverallSummary = table.Column<string>(type: "text", nullable: false),
                    StrengthsJson = table.Column<string>(type: "text", nullable: false),
                    DevelopmentPointsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IkigaiSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IkigaiSummaries_IkigaiResults_ResultId",
                        column: x => x.ResultId,
                        principalTable: "IkigaiResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IkigaiResults_UserId",
                table: "IkigaiResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IkigaiSummaries_ResultId",
                table: "IkigaiSummaries",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_LoveSessionDatas_UserId",
                table: "LoveSessionDatas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaidSessionDatas_UserId",
                table: "PaidSessionDatas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrologueDatas_UserId",
                table: "PrologueDatas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillSessionDatas_UserId",
                table: "SkillSessionDatas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorldSessionDatas_UserId",
                table: "WorldSessionDatas",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IkigaiSummaries");

            migrationBuilder.DropTable(
                name: "LoveSessionDatas");

            migrationBuilder.DropTable(
                name: "PaidSessionDatas");

            migrationBuilder.DropTable(
                name: "PrologueDatas");

            migrationBuilder.DropTable(
                name: "SkillSessionDatas");

            migrationBuilder.DropTable(
                name: "WorldSessionDatas");

            migrationBuilder.DropTable(
                name: "IkigaiResults");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
