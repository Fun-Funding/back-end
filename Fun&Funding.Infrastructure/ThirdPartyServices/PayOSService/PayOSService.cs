using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.ExternalServices.PayOSService
{
    public class PayOSService
    {
        private readonly PayOS _payOS;

        public PayOSService(PayOS payOS)
        {
            _payOS = payOS;
        }

        public async Task<CreatePaymentResult> Checkout()
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("Mì tôm hảo hảo ly", 1, 2000);
                List<ItemData> items = new List<ItemData>();
                items.Add(item);
                PaymentData paymentData = new PaymentData(orderCode, 2000, "Thanh toan don hang", items, "https://localhost:3002/cancel", "https://localhost:3002/success");

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                return createPayment;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
                //Console.WriteLine(exception);
                //return Redirect("https://localhost:3002/");
            }
        }
    }
}
