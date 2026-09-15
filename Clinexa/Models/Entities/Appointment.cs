using Clinexa.Enums;

namespace Clinexa.Models.Entities
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public AppointmentStatus AppointmentStatus { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PatientId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int DoctorId { get; set; }
        public MedicalRecord? MedicalRecord { get; set; }

        public Invoice? Invoice { get; set; }



    }
}
