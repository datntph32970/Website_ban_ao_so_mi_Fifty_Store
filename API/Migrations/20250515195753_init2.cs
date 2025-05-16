using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_KhachHangs_id_khach_hang",
                table: "GioHangChiTiets");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_KhachHangs_id_khach_hang",
                table: "GioHangChiTiets",
                column: "id_khach_hang",
                principalTable: "KhachHangs",
                principalColumn: "id_khach_hang",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangChiTiets_KhachHangs_id_khach_hang",
                table: "GioHangChiTiets");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangChiTiets_KhachHangs_id_khach_hang",
                table: "GioHangChiTiets",
                column: "id_khach_hang",
                principalTable: "KhachHangs",
                principalColumn: "id_khach_hang",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
