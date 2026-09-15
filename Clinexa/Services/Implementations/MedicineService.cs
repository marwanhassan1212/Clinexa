using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository medicineRepository;
        public MedicineService(IMedicineRepository medicineRepository)
        {
            this.medicineRepository = medicineRepository;
        }
        public async Task<bool> CreateAsync(Medicine medicine)
        {
            bool nameExists =
               await medicineRepository
                   .ExistsByNameAsync(
                       medicine.Name);

            if (nameExists)
            {
                return false;
            }

            medicine.IsActive = true;
            medicine.CreatedAt = DateTime.Now;

            await medicineRepository
                .AddAsync(medicine);

            await medicineRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var medicine =
                await medicineRepository
                    .GetByIdAsync(id);

            if (medicine == null)
            {
                return false;
            }

            medicine.IsActive = false;

            medicineRepository.Update(medicine);

            await medicineRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<List<Medicine>> GetActiveAsync()
        {
            return await medicineRepository
            .GetActiveAsync();
        }

        public async Task<List<Medicine>> GetAllAsync()
        {
            return await medicineRepository.GetAllAsync();
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            return await medicineRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Medicine medicine)
        {
            var medicineExists =
             await medicineRepository
                 .GetByIdAsync(
                     medicine.MedicineId);

            if (medicineExists == null)
            {
                return false;
            }

            bool nameExists =
                await medicineRepository
                    .ExistsByNameAsync(
                        medicine.Name,
                        medicine.MedicineId);

            if (nameExists)
            {
                return false;
            }

            medicineRepository.Update(medicine);

            await medicineRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
