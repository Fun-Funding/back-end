using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Net.payOS;
using Fun_Funding.Application.ViewModel.WalletDTO;
using Org.BouncyCastle.Utilities;
using Fun_Funding.Application.IService;
using static System.Net.WebRequestMethods;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/PayOS")]
    [ApiController]
    public class PayOSController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly IWalletService _walletService;

        public PayOSController(PayOS payOS, IWalletService walletService)
        {
            _payOS = payOS;
            _walletService = walletService;
        }

        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] WalletRequest walletRequest) {

            await _walletService.AddMoneyToWallet(walletRequest);
            return Redirect("http://localhost:5173/account/wallet");
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel()
        {
            return Redirect("http://localhost:5173/account/wallet");
        }

        [HttpGet("create-payment-link")]
        public async Task<IActionResult> Checkout([FromQuery] WalletRequest walletRequest)
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                string description = $"Add money to wallet";
                ItemData item = new ItemData(description, 1, (int)walletRequest.Balance);
                List<ItemData> items = new List<ItemData>();
                items.Add(item);
                PaymentData paymentData = new PaymentData(orderCode, (int)walletRequest.Balance, description, items,
                    "https://localhost:7044/api/payos/cancel",
                    $"https://localhost:7044/api/payos/success?balance={walletRequest.Balance}&walletId={walletRequest.WalletId}");
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                return Redirect(createPayment.checkoutUrl);
            }
            catch (System.Exception exception)
            {
                //Console.WriteLine(exception);
                //return Redirect("https://localhost:3002/");
                throw new System.Exception(exception.Message);
            }
        }
    }
}
