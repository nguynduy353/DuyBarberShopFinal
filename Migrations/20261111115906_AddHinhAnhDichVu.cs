using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuyBarberShop.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhDichVu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "DICHVU",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "DICHVU");
        }
    }
}
