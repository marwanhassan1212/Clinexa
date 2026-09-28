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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogService
                .GetAllAsync();

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _auditLogService
                .GetByIdAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        [HttpGet]
        public async Task<IActionResult> ByUser(int userId)
        {
            var logs = await _auditLogService
                .GetByUserIdAsync(userId);

            return View(
                "Index",
                logs);
        }
        [HttpGet]
        public async Task<IActionResult> ByEntity(
            string entityName,
            int entityId)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                return BadRequest();
            }

            var logs = await _auditLogService
                .GetByEntityAsync(
                    entityName,
                    entityId);

            return View(
                "Index",
                logs);
        }
    }
}