using Clinexa.Data;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Implementations;
using Clinexa.Services.Interfaces;
using Clinexa.Services.PDF;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Clinexa.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Clinexa
{
    public class Program
    {
        public static  void Main(string[] args)
        {

            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // Repositories
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<ISpecialityRepository, SpecialityRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();
            builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
           

            // Services
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<ISpecialityService, SpecialityService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
            builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
            builder.Services.AddScoped<IMedicineService, MedicineService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IInvoiceItemService, InvoiceItemService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();


            //PDF
            builder.Services.AddScoped<IPrescriptionPdfService, PrescriptionPdfService>();


            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("Constr"),
                sqlOptions => sqlOptions.UseCompatibilityLevel(120)));

            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.Cookie.Name = "Clinexa.Auth";

                options.Cookie.HttpOnly = true;

                options.Cookie.SameSite = SameSiteMode.Lax;

                options.Cookie.SecurePolicy =
                    CookieSecurePolicy.SameAsRequest;

                options.ExpireTimeSpan =
                    TimeSpan.FromMinutes(30);

                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization(options =>
            {
                // Administration
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });

                // Clinical access
                options.AddPolicy("ClinicalAccess", policy =>
                {
                    policy.RequireRole("Admin", "Doctor");
                });

                // Reception / operational access
                options.AddPolicy("OperationalAccess", policy =>
                {
                    policy.RequireRole(
                        "Admin",
                        "Doctor",
                        "Receptionist");
                });

                // Billing
                options.AddPolicy("BillingAccess", policy =>
                {
                    policy.RequireRole(
                        "Admin",
                        "Receptionist");
                });
            });



            // Add services to the container.
            builder.Services.AddControllersWithViews();


            var app = builder.Build();

           

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
