using Clinexa.Data;
using Clinexa.Enums;
using Clinexa.Models.ViewModels.Dashboard;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _db;

        public DashboardRepository(AppDbContext db)
        {
            _db = db;
        }


        // Today's Appointments
        public async Task<int> GetTodayAppointmentsCountAsync()
        {
            var today = DateTime.Today;

            return await _db.Appointments
                .AsNoTracking()
                .CountAsync(x =>
                    x.AppointmentDate == today);
        }


        // Active Patients
        public async Task<int> GetActivePatientsCountAsync()
        {
            return await _db.Patients
                .AsNoTracking()
                .CountAsync(x => x.IsActive);
        }


        // Active Doctors
        public async Task<int> GetActiveDoctorsCountAsync()
        {
            return await _db.Doctors
                .AsNoTracking()
                .CountAsync(x => x.IsActive);
        }


        // Today's Revenue
        public async Task<decimal> GetTodayRevenueAsync()
        {
            var start = DateTime.Today;
            var end = start.AddDays(1);

            return await _db.Payments
                .AsNoTracking()
                .Where(x =>
                    x.PaymentDate >= start &&
                    x.PaymentDate < end)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;
        }


        // Completed Appointments
        public async Task<int> GetCompletedAppointmentsCountAsync()
        {
            var today = DateTime.Today;

            return await _db.Appointments
                .AsNoTracking()
                .CountAsync(x =>
                    x.AppointmentDate == today &&
                    x.AppointmentStatus == AppointmentStatus.Completed);
        }



        // Upcoming Appointments
        public async Task<int> GetUpcomingAppointmentsCountAsync()
        {
            var today = DateTime.Today;
            var now = DateTime.Now.TimeOfDay;

            return await _db.Appointments
                .AsNoTracking()
                .CountAsync(x =>
                    x.AppointmentDate == today &&
                    x.StartTime >= now &&
                    x.AppointmentStatus != AppointmentStatus.Cancelled &&
                    x.AppointmentStatus != AppointmentStatus.NoShow &&
                    x.AppointmentStatus != AppointmentStatus.Completed);
        }


        // Cancelled Appointments

        public async Task<int> GetCancelledAppointmentsCountAsync()
        {
            var today = DateTime.Today;

            return await _db.Appointments
                .AsNoTracking()
                .CountAsync(x =>
                    x.AppointmentDate == today &&
                    x.AppointmentStatus == AppointmentStatus.Cancelled);
        }


        // No-Show Appointments

        public async Task<int> GetNoShowAppointmentsCountAsync()
        {
            var today = DateTime.Today;

            return await _db.Appointments
                .AsNoTracking()
                .CountAsync(x =>
                    x.AppointmentDate == today &&
                    x.AppointmentStatus == AppointmentStatus.NoShow);
        }


        // Unpaid Invoices

        public async Task<int> GetUnpaidInvoicesCountAsync()
        {
            return await _db.Invoices
                .AsNoTracking()
                .CountAsync(x =>
                    x.InvoiceStatus == InvoiceStatus.Unpaid ||
                    x.InvoiceStatus == InvoiceStatus.PartiallyPaid);
        }


        // Outstanding Amount

        public async Task<decimal> GetOutstandingAmountAsync()
        {
            return await _db.Invoices
                .AsNoTracking()
                .Where(x =>
                    x.InvoiceStatus == InvoiceStatus.Unpaid ||
                    x.InvoiceStatus == InvoiceStatus.PartiallyPaid)
                .SumAsync(x => (decimal?)x.RemainingAmount) ?? 0;
        }


        // Today's Appointments List

        public async Task<List<DashboardAppointmentViewModel>> GetTodayAppointmentsAsync()
        {
            var today = DateTime.Today;

            return await _db.Appointments
                .AsNoTracking()
                .Where(x => x.AppointmentDate == today)
                .OrderBy(x => x.StartTime)
                .Take(8)
                .Select(x => new DashboardAppointmentViewModel
                {
                    AppointmentId = x.AppointmentId,

                    PatientName =
                        x.Patient.FirstName + " " +
                        x.Patient.LastName,

                    DoctorName =
                        "Dr. " +
                        x.Doctor.User.FirstName + " " +
                        x.Doctor.User.LastName,

                    StartTime = x.StartTime,

                    Status = x.AppointmentStatus
                })
                .ToListAsync();
        }

        public async Task<List<DashboardAlertViewModel>> GetAlertsAsync()
        {
            var alerts = new List<DashboardAlertViewModel>();

            var unpaidInvoices = await GetUnpaidInvoicesCountAsync();

            if (unpaidInvoices > 0)
            {
                alerts.Add(new DashboardAlertViewModel
                {
                    Title = "Unpaid Invoices",
                    Description = $"{unpaidInvoices} invoice(s) require payment.",
                    Icon = "bi bi-receipt",
                    CssClass = "text-warning",
                    Controller = "Invoice",
                    Action = "Index"
                });
            }

            var noShowAppointments = await GetNoShowAppointmentsCountAsync();

            if (noShowAppointments > 0)
            {
                alerts.Add(new DashboardAlertViewModel
                {
                    Title = "No-Show Appointments",
                    Description = $"{noShowAppointments} appointment(s) marked as no-show today.",
                    Icon = "bi bi-person-x",
                    CssClass = "text-danger",
                    Controller = "Appointment",
                    Action = "Index"
                });
            }

            return alerts;
        }
    }
}
