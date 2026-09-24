using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinexa.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentStatusWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultationStartedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationStartedAt",
                table: "Appointments");
        }
    }
}
