using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dia_chi_cu_the",
                table: "DiaChis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "so_dien_thoai",
                table: "DiaChis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ten_nguoi_nhan",
                table: "DiaChis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dia_chi_cu_the",
                table: "DiaChis");

            migrationBuilder.DropColumn(
                name: "so_dien_thoai",
                table: "DiaChis");

            migrationBuilder.DropColumn(
                name: "ten_nguoi_nhan",
                table: "DiaChis");
        }
    }
}
