using Clinexa.Models.ViewModels.Search;

namespace Clinexa.Services.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultViewModel> SearchAsync(string searchTerm);
    }
}
