namespace Clinexa.Models.Entities
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public int UserId { get; set; }
        public Speciality Speciality { get; set; } = null!;
        public int SpecialityId { get; set; }
        public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();


    }
}
