using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.WalletDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/wallets")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetWalletByUserId()
        {
            var walletResult = await _walletService.GetWalletByUser();
            if (walletResult._isSuccess)
                return Ok(walletResult);
            else
                return BadRequest(walletResult);
        }

        //[HttpPut]
        //public async Task<IActionResult> AddMoneyToWallet([FromBody] WalletRequest walletRequest)
        //{
        //    if (walletRequest == null)
        //    {
        //        return BadRequest("WalletRequest cannot be null.");
        //    }

        //    var walletResult = await _walletService.AddMoneyToWallet(walletRequest);

        //    if (walletResult._isSuccess)
        //    {
        //        return Ok(walletResult);
        //    }

        //    return BadRequest(walletResult);

        //}
    }
}
