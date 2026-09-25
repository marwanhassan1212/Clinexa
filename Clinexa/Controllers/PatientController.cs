using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Patient;
using Clinexa.Services.Implementations;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "OperationalAccess")]
    public class PatientController : Controller
    {
        private readonly IPatientService patientService;
        public PatientController(IPatientService patientService)
        {
            this.patientService = patientService;
        }
        public async Task<IActionResult> Index(PatientFilterViewModel model)
        {
            var result = await patientService.FilterAsync(
            model.Search,
            model.Gender,
            model.BloodType,
            model.IsActive,
            model.Page,
            model.PageSize);

            model.Patients = result.Patients;
            model.TotalCount = result.TotalCount;

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var patient = await patientService.GetByIdAsync(id);
            if(patient == null)
            {
                return NotFound();
            }
            else
            {
                return View(patient);
            }
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                var patient = new Patient()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Address = model.Address,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    BloodType = model.BloodType,
                    Allergies = model.Allergies
                };
                var result = await patientService.CreateAsync(patient);
                if(!result)
                {
                    ModelState.AddModelError(nameof(model.PhoneNumber)
                        , "A Patient with this phone number is already exists.");
                    return View(model);
                }
                else
                {
                    TempData["Success"] = "Patient Created Successfully";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(model);
            }
        }

        // GET
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await patientService.GetByIdAsync(id);
            if(patient == null)
            {
                return NotFound();
            }
            else
            {
                var patientView = new EditViewModel
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    DateOfBirth = patient.DateOfBirth,
                    Gender = patient.Gender,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,
                    Address = patient.Address,
                    EmergencyContactName = patient.EmergencyContactName,
                    EmergencyContactPhone = patient.EmergencyContactPhone,
                    BloodType = patient.BloodType,
                    Allergies = patient.Allergies

                };
                return View(patientView);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if(ModelState.IsValid)
            {
                var patient = await patientService.GetByIdAsync(model.PatientId);
                if(patient == null)
                {
                    return NotFound();
                }
                else
                {
                    patient.FirstName = model.FirstName;
                    patient.LastName = model.LastName;
                    patient.DateOfBirth = model.DateOfBirth;
                    patient.Gender = model.Gender;
                    patient.PhoneNumber = model.PhoneNumber;
                    patient.Email = model.Email;
                    patient.Address = model.Address;
                    patient.EmergencyContactName = model.EmergencyContactName;
                    patient.EmergencyContactPhone = model.EmergencyContactPhone;
                    patient.BloodType = model.BloodType;
                    patient.Allergies = model.Allergies;
                }
                var result = await patientService.UpdateAsync(patient);
                if(!result)
                {
                    return NotFound();
                }
                else
                {
                    TempData["Success"] = "Patient updated successfully.";
                    return RedirectToAction("Index");
                }


            }
            else
            {
                return View(model);
            }
        }

        // GET: /Patient/Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var patients = await patientService.SearchAsync(searchTerm);

            var model = new PatientFilterViewModel
            {
                Search = searchTerm,
                Patients = patients
            };

            return View("Index", model);
        }


        // POST: /Patient/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await patientService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Patient deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // ACTIVATE
        public async Task<IActionResult> Activate(int id)
        {
            var result = await patientService.ActivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Patient activated successfully.";

            return RedirectToAction(nameof(Index));
        }

    }
}
