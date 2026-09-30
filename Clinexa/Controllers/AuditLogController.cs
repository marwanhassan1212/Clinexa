using Clinexa.Models.ViewModels.AuditLog;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // =========================================================
        // INDEX
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogService.GetAllAsync();

            var model = new AuditLogFilterViewModel
            {
                AuditLogs = logs,
                TotalCount = logs.Count,
                Page = 1,
                PageSize = 20,

                Users = await _auditLogService.GetUsersAsync(),
                EntityNames = await _auditLogService.GetEntityNamesAsync(),
                Actions = await _auditLogService.GetActionsAsync()
            };

            return View(model);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _auditLogService.GetByIdAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }


        // =========================================================
        // BY USER
        // =========================================================

        [HttpGet]
        public IActionResult ByUser(int userId)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    userId
                });
        }


        // =========================================================
        // BY ENTITY
        // =========================================================

        [HttpGet]
        public IActionResult ByEntity(
            string entityName,
            int entityId)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                return BadRequest();
            }

            return RedirectToAction(
                nameof(Index),
                new
                {
                    entityName,
                    search = entityId.ToString()
                });
        }
    }
}
