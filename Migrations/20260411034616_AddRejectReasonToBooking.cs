using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSocietyMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectReasonToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Bookings");
        }
    }
}
