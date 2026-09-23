using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IPrescriptionItemRepository prescriptionItemRepository;
 
        public PrescriptionItemService(IPrescriptionItemRepository prescriptionItemRepository)
        {
            this.prescriptionItemRepository = prescriptionItemRepository;
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
