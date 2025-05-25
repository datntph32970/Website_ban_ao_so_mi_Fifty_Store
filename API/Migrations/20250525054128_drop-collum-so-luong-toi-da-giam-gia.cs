using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class dropcollumsoluongtoidagiamgia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "so_luong_toi_da",
                table: "GiamGias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "so_luong_toi_da",
                table: "GiamGias",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
