using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(
            IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogService.GetAllAsync();

            return View(logs);
        }
    }
}