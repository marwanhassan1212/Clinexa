using Clinexa.Models.Entities;

namespace Clinexa.Services.PDF
{
    public interface IPrescriptionPdfService
    {
        byte[] Generate(Prescription prescription);
    }
}
