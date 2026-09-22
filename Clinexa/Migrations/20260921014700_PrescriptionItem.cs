using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinexa.Migrations
{
    /// <inheritdoc />
    public partial class PrescriptionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_MedicineId_PrescriptionId",
                table: "PrescriptionItems");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MedicineId_PrescriptionId",
                table: "PrescriptionItems",
                columns: new[] { "MedicineId", "PrescriptionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItems_MedicineId_PrescriptionId",
                table: "PrescriptionItems");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MedicineId_PrescriptionId",
                table: "PrescriptionItems",
                columns: new[] { "MedicineId", "PrescriptionId" });
        }
    }
}
