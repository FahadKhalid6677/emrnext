using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMRNext.Core.Services.Clinical;
using EMRNext.Core.Infrastructure.Reporting;

namespace EMRNext.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ClinicalWorkflowService _workflowService;
        private readonly ReportingEngine.ReportingService _reportingService;

        public DashboardController(
            ClinicalWorkflowService workflowService,
            ReportingEngine.ReportingService reportingService)
        {
            _workflowService = workflowService;
            _reportingService = reportingService;
        }

        public IActionResult Index()
        {
            // Placeholder for dashboard data
            var model = new DashboardViewModel
            {
                TotalPatients = 1000,
                ActiveWorkflows = 50,
                PendingTasks = 25
            };

            return View(model);
        }

        public class DashboardViewModel
        {
            public int TotalPatients { get; set; }
            public int ActiveWorkflows { get; set; }
            public int PendingTasks { get; set; }
        }
    }
}
