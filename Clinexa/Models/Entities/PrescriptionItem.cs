namespace Clinexa.Models.Entities
{
    public class PrescriptionItem
    {
        public int PrescriptionItemId { get; set; }
        public string Dosage { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string Duration { get; set; } = null!;
        public string? Instructions { get; set; }
        public Prescription Prescription { get; set; } = null!;
        public int PrescriptionId { get; set; }
        public Medicine Medicine { get; set; } = null!;
        public int MedicineId { get; set; }

    }
}
