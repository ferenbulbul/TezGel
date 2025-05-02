using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TezGel.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPropAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                table: "BusinessUsers",
                newName: "CompanyType");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitute",
                table: "AspNetUsers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Longitute",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CompanyType",
                table: "BusinessUsers",
                newName: "TaxNumber");
        }
    }
}
