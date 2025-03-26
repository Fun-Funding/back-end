using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.OrderDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserOrders([FromQuery] ListRequest request)
        {
            var response = await _orderService.GetUserOrders(request);
            return Ok(response);
        }
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            var response = await _orderService.GetOrderById(orderId);
            return Ok(response);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
        {
            var response = await _orderService.CreateOrder(createOrderRequest);
            return Ok(response);
        }
        [HttpGet("all-orders")]
        [Authorize]
        public async Task<IActionResult> GetAllOrders([FromQuery] ListRequest request)
        {
            var response = await _orderService.GetAllOrders(request);
            return Ok(response);
        }

        [HttpGet("group-orders")]
        public async Task<IActionResult> GroupOrders([FromQuery] Guid id)
        {
            var result = await _orderService.GetOrdersGroupedByDate(id);
            return Ok(result);
        }

        [HttpGet("/api/order-details/{marketplaceProjectId}/purchases")]
        public async Task<IActionResult> GetPurchases([FromRoute] Guid marketplaceProjectId)
        {
            var result = await _orderService.GetOrderDetailsByMarketplaceProjectId(marketplaceProjectId);
            return Ok(result);
        }
    }
}
