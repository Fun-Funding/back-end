using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.OrderDTO;

namespace Fun_Funding.Application.IService
{
    public interface IOrderService
    {
        Task<ResultDTO<PaginatedResponse<OrderInfoResponse>>> GetUserOrders(ListRequest request);
        Task<ResultDTO<Guid>> CreateOrder(CreateOrderRequest createOrderRequest);
        Task<ResultDTO<PaginatedResponse<OrderInfoResponse>>> GetAllOrders(ListRequest request);
        Task<ResultDTO<OrderInfoResponse>> GetOrderById(Guid orderId);
        Task<ResultDTO<List<OrderSummary>>> GetOrdersGroupedByDate(Guid marketplaceProjectId);
        Task<ResultDTO<IEnumerable<object>>> GetOrderDetailsByMarketplaceProjectId(Guid marketplaceProjectId);
    }
}
