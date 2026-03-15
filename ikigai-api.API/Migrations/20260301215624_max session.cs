using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ikigai_api.API.Migrations
{
    /// <inheritdoc />
    public partial class maxsession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaxSessionPercentage",
                table: "IkigaiResults",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSessionPercentage",
                table: "IkigaiResults");
        }
    }
}
