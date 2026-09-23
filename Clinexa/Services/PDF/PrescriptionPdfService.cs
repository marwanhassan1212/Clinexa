using Clinexa.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinexa.Services.PDF
{
    public class PrescriptionPdfService : IPrescriptionPdfService
    {
        public byte[] Generate(Prescription prescription)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10));

                    page.Header()
                        .Text("CLINEXA")
                        .FontSize(22)
                        .Bold();

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item()
                                .Text("Medical Prescription")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Text($"Prescription #{prescription.PrescriptionId}");

                            column.Item()
                                .Text(
                                    $"Date: {prescription.PrescriptionDate:dd MMM yyyy}");

                            column.Item()
                                .LineHorizontal(1);

                            column.Item()
                                .Text("Patient Information")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Patient: {prescription.MedicalRecord.Patient.FirstName} " +
                                    $"{prescription.MedicalRecord.Patient.LastName}");

                            column.Item()
                                .Text(
                                    $"Doctor: Dr. " +
                                    $"{prescription.MedicalRecord.Doctor.User.FirstName} " +
                                    $"{prescription.MedicalRecord.Doctor.User.LastName}");

                            column.Item()
                                .LineHorizontal(1);

                            column.Item()
                                .Text("Prescription")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Medicine").Bold();
                                        header.Cell().Text("Dosage").Bold();
                                        header.Cell().Text("Frequency").Bold();
                                        header.Cell().Text("Duration").Bold();
                                        header.Cell().Text("Instructions").Bold();
                                    });

                                    foreach (var item in prescription.PrescriptionItems)
                                    {
                                        table.Cell()
                                            .Text(item.Medicine.Name);

                                        table.Cell()
                                            .Text(item.Dosage);

                                        table.Cell()
                                            .Text(item.Frequency);

                                        table.Cell()
                                            .Text(item.Duration);

                                        table.Cell()
                                            .Text(item.Instructions ?? "-");
                                    }
                                });

                            if (!string.IsNullOrWhiteSpace(prescription.Notes))
                            {
                                column.Item()
                                    .PaddingTop(10)
                                    .Text("Notes")
                                    .FontSize(13)
                                    .Bold();

                                column.Item()
                                    .Text(prescription.Notes);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Clinexa • Medical Management System")
                        .FontSize(8);
                });
            });

            return document.GeneratePdf();
        }
    }
}