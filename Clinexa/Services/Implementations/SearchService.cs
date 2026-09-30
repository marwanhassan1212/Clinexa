using Clinexa.Models.ViewModels.Search;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class SearchService : ISearchService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public SearchService(
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<SearchResultViewModel> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.Trim();

            // Patients
            var patients =
                await _patientRepository.SearchAsync(searchTerm);

            // Doctors
            var doctorResult =
                await _doctorRepository.FilterAsync(
                    searchTerm,
                    null,
                    null,
                    1,
                    100);


            // Appointments
            var appointmentResult =
                await _appointmentRepository.FilterAsync(
                    searchTerm,
                    null,
                    null,
                    null,
                    null,
                    1,
                    100);

            // Build Search Result
            var result = new SearchResultViewModel
            {
                SearchTerm = searchTerm,

                Patients = patients
                    .Select(x => new PatientSearchResultViewModel
                    {
                        PatientId = x.PatientId,

                        FullName =
                            $"{x.FirstName} {x.LastName}",

                        Phone = x.PhoneNumber
                    })
                    .ToList(),

                Doctors = doctorResult.Doctors
                    .Select(x => new DoctorSearchResultViewModel
                    {
                        DoctorId = x.DoctorId,

                        FullName =
                            $"{x.User.FirstName} {x.User.LastName}",

                        SpecialityName =
                            x.Speciality?.Name
                    })
                    .ToList(),

                Appointments = appointmentResult.Appointments
                    .Select(x => new AppointmentSearchResultViewModel
                    {
                        AppointmentId = x.AppointmentId,

                        PatientName =
                            $"{x.Patient.FirstName} {x.Patient.LastName}",

                        DoctorName =
                            $"{x.Doctor.User.FirstName} {x.Doctor.User.LastName}",

                        AppointmentDate = x.AppointmentDate,

                        StartTime = x.StartTime,

                        Status = x.AppointmentStatus.ToString()
                    })
                    .ToList()
            };

            return result;
        }
    }
}
