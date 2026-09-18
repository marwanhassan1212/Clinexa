using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorScheduleRepository doctorScheduleRepository;
        public DoctorScheduleService(IDoctorScheduleRepository doctorScheduleRepository)
        {
            this.doctorScheduleRepository = doctorScheduleRepository;
        }
        public async Task<bool> CreateAsync(DoctorSchedule schedule)
        {
            bool doctor = await doctorScheduleRepository.DoctorExistsAsync(schedule.DoctorId);
            if(!doctor)
            {
                return false;
            }
            if(schedule.StartTime >= schedule.EndTime)
            {
                return false;
            }
            bool scheduleExist = await doctorScheduleRepository.ExistsAsync(schedule.DoctorId, schedule.DayOfWeek, schedule.StartTime);
            if(scheduleExist)
            {
                return false;
            }

            schedule.IsAvailable = true;
            await doctorScheduleRepository.AddAsync(schedule);
            await doctorScheduleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var schedule = await doctorScheduleRepository.GetByIdAsync(id);
            if(schedule == null)
            {
                return false;
            }
            else
            {
                schedule.IsAvailable = false;
                doctorScheduleRepository.Update(schedule);
                await doctorScheduleRepository.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<DoctorSchedule>> GetAllAsync()
        {
            return await doctorScheduleRepository.GetAllAsync();
        }

        public async Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId)
        {
            return await doctorScheduleRepository.GetByDoctorIdAsync(doctorId);
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int id)
        {
            return await doctorScheduleRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(DoctorSchedule schedule)
        {
            var scheduleExists = await doctorScheduleRepository.GetByIdAsync(schedule.DoctorScheduleId);
            if(scheduleExists == null)
            {
                return false;
            }
            bool doctorExists = await doctorScheduleRepository.DoctorExistsAsync(schedule.DoctorId);
            if(!doctorExists)
            {
                return false;
            }
            if(schedule.StartTime >= schedule.EndTime)
            {
                return false;
            }

            bool scheduleDuplicate = await doctorScheduleRepository.ExistsAsync(schedule.DoctorId, schedule.DayOfWeek, schedule.StartTime);
            if(scheduleDuplicate && (scheduleExists.DoctorId != schedule.DoctorId
                || scheduleExists.DayOfWeek != schedule.DayOfWeek
                || scheduleExists.StartTime != schedule.StartTime))
            {
                return false;
            }
            doctorScheduleRepository.Update(schedule);
            await doctorScheduleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var schedule = await doctorScheduleRepository.GetByIdAsync(id);

            if (schedule == null)
                return false;

            schedule.IsAvailable = true;

            doctorScheduleRepository.Update(schedule);
            await doctorScheduleRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(List<DoctorSchedule> Schedules, int TotalCount)> FilterAsync(
                    string? search,
                    int? doctorId,
                    DayOfWeek? dayOfWeek,
                    bool? isAvailable,
                    int page,
                    int pageSize)
        {
            return await doctorScheduleRepository.FilterAsync(
                search,
                doctorId,
                dayOfWeek,
                isAvailable,
                page,
                pageSize);
        }
    }
}
