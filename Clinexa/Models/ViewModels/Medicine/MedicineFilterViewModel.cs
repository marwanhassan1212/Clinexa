namespace Clinexa.Models.ViewModels.Medicine
{
    public class MedicineFilterViewModel
    {
        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<Entities.Medicine> Medicines { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(
                (double)TotalCount / PageSize);
    }
}
