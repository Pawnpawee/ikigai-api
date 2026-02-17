using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ikigai_api.API.Migrations
{
    /// <inheritdoc />
    public partial class AddShortSummaryToIkigaiSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortSummary",
                table: "IkigaiSummaries",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortSummary",
                table: "IkigaiSummaries");
        }
    }
}
