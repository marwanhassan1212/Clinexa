using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinexa.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceEdited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId1",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId1",
                table: "Invoices",
                column: "AppointmentId1",
                unique: true,
                filter: "[AppointmentId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId1",
                table: "Invoices",
                column: "AppointmentId1",
                principalTable: "Appointments",
                principalColumn: "AppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AppointmentId1",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AppointmentId1",
                table: "Invoices");
        }
    }
}
