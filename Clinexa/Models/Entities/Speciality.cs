namespace Clinexa.Models.Entities
{
    public class Speciality
    {
        public int SpecialityId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    }
}
