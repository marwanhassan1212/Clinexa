using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IPrescriptionItemRepository prescriptionItemRepository;
        private readonly UserManager<User> userManager;
 
        public PrescriptionItemService(IPrescriptionItemRepository prescriptionItemRepository
            ,UserManager<User> userManager)
        {
            this.prescriptionItemRepository = prescriptionItemRepository;
            this.userManager = userManager;
        }

        public async Task<bool> CanAccessAsync(
                 int prescriptionItemId,
                 int currentUserId)
        {
            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return false;
            }

            // Admin can access everything.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can access only items
            // belonging to their own prescription.
            return await prescriptionItemRepository
                .BelongsToDoctorAsync(
                    prescriptionItemId,
                    currentUserId);
        }

        public async Task<bool>
            CanAccessPrescriptionAsync(
                int prescriptionId,
                int currentUserId)
        {
            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return false;
            }

            // Admin can access everything.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can create items only inside
            // their own prescription.
            return await prescriptionItemRepository
                .PrescriptionBelongsToDoctorAsync(
                    prescriptionId,
                    currentUserId);
        }

        public async Task<bool> CreateAsync(PrescriptionItem prescriptionItem)
        {
            bool medicineAlreadyExists =
                await prescriptionItemRepository
                    .ExistsForPrescriptionAsync(
                        prescriptionItem.PrescriptionId,
                        prescriptionItem.MedicineId);

            if (medicineAlreadyExists)
            {
                return false;
            }

            bool PrescriptionExists = await prescriptionItemRepository.PrescriptionExistsAsync(prescriptionItem.PrescriptionId);
            if(!PrescriptionExists)
            {
                return false;
            }

            bool ActiveMedicineExists = await prescriptionItemRepository.ActiveMedicineExistsAsync(prescriptionItem.MedicineId);
            if(!ActiveMedicineExists)
            {
                return false;
            }
            await prescriptionItemRepository.AddAsync(prescriptionItem);
            await prescriptionItemRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<PrescriptionItem>> GetAllAsync()
        {
            return await prescriptionItemRepository.GetAllAsync();
        }

        public async Task<PrescriptionItem?> GetByIdAsync(int id)
        {
            return await prescriptionItemRepository.GetByIdAsync(id);
        }

        public async Task<List<PrescriptionItem>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            return await prescriptionItemRepository.GetByPrescriptionIdAsync(prescriptionId);
        }

        public async Task<bool> UpdateAsync(PrescriptionItem prescriptionItem)
        {
            var prescriptionItemExists = await prescriptionItemRepository.GetByIdAsync(prescriptionItem.PrescriptionItemId);
            if(prescriptionItemExists == null)
            {
                return false;
            }

            bool prescriptionExists = await prescriptionItemRepository.PrescriptionExistsAsync(prescriptionItem.PrescriptionId);
            if(!prescriptionExists)
            {
                return false;
            }
            bool ActiveMedicineExists = await prescriptionItemRepository.ActiveMedicineExistsAsync(prescriptionItem.MedicineId);
            if(!ActiveMedicineExists)
            {
                return false;
            }
            bool medicineAlreadyExists =
            await prescriptionItemRepository
                .ExistsForPrescriptionAsync(
                    prescriptionItem.PrescriptionId,
                    prescriptionItem.MedicineId,
                    prescriptionItem.PrescriptionItemId);

            if (medicineAlreadyExists)
            {
                return false;
            }
            prescriptionItemRepository.Update(prescriptionItem);
            await prescriptionItemRepository.SaveChangesAsync();
            return true;

        }

     
    }
}
