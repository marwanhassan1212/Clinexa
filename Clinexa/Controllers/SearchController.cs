
using Clinexa.Models.ViewModels.Search;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return View(new SearchResultViewModel());
            }

            var model = await _searchService.SearchAsync(search);

            return View(model);
        }
    }
}