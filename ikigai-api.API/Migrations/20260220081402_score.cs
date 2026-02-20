using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ikigai_api.API.Migrations
{
    /// <inheritdoc />
    public partial class score : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "GoodAtPercentage",
                table: "IkigaiResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LovePercentage",
                table: "IkigaiResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PaidForPercentage",
                table: "IkigaiResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WorldNeedsPercentage",
                table: "IkigaiResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoodAtPercentage",
                table: "IkigaiResults");

            migrationBuilder.DropColumn(
                name: "LovePercentage",
                table: "IkigaiResults");

            migrationBuilder.DropColumn(
                name: "PaidForPercentage",
                table: "IkigaiResults");

            migrationBuilder.DropColumn(
                name: "WorldNeedsPercentage",
                table: "IkigaiResults");
        }
    }
}
