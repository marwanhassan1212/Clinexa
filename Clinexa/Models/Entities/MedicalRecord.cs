namespace Clinexa.Models.Entities
{
    public class MedicalRecord
    {
        public int MedicalRecordId { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public string? Treatment { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PatientId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int DoctorId { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public int AppointmentId { get; set; }
        public Prescription? Prescription { get; set; }


    }
}
