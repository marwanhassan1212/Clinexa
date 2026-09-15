namespace Clinexa.Models.Entities
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string? Notes { get; set; }
        public MedicalRecord MedicalRecord { get; set; } = null!;
        public int MedicalRecordId { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; }
            = new List<PrescriptionItem>();

    }
}
