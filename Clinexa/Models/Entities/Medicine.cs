namespace Clinexa.Models.Entities
{
    public class Medicine
    {
        public int MedicineId { get; set; }
        public string Name { get; set; } = null!;
        public string? GenericName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; }
           = new List<PrescriptionItem>();

    }
}
