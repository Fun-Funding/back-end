using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/system-wallet")]
    [ApiController]
    public class SystemWalletController : ControllerBase
    {
        private ISystemWalletService _systemWalletService;
        public SystemWalletController(ISystemWalletService systemWalletService)
        {
            _systemWalletService = systemWalletService;
        }

        [HttpGet("platform-revenue")]
        public async Task<IActionResult> GetPlatformRevenue()
        {
            var response = await _systemWalletService.GetPlatformRevenue();
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet()
        {
            var res = await _systemWalletService.CreateWallet();
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var res = _systemWalletService.GetSystemWallet();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var res = await _systemWalletService.GetDashboardMetrics();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/users")]
        public async Task<IActionResult> GetDashboardUsers()
        {
            var res = await _systemWalletService.GetDashboardUsers();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/funding-projects")]
        public async Task<IActionResult> GetDashboardFundingProjects()
        {
            var res = await _systemWalletService.GetDashboardFundingProjects();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/marketplace-projects")]
        public async Task<IActionResult> GetDashboardMarketplaceProjects()
        {
            var res = await _systemWalletService.GetDashboardMarketplaceProjects();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/milestones")]
        public async Task<IActionResult> GetDashboardMilestones()
        {
            var res = await _systemWalletService.GetDashboardMilestones();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/categories")]
        public async Task<IActionResult> GetDashboardCategories()
        {
            var res = await _systemWalletService.GetDashboardCategories();
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/transactions")]
        public async Task<IActionResult> GetDashboardTransactions([FromQuery] ListRequest request, [FromQuery] List<TransactionTypes>? types)
        {
            var res = await _systemWalletService.GetDashboardTransactions(request, types);
            return Ok(res);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("/api/dashboard/income")]
        public IActionResult GetDashboardIncome()
        {
            var res = _systemWalletService.GetDashboardIncome();
            return Ok(res);
        }
    }
}
