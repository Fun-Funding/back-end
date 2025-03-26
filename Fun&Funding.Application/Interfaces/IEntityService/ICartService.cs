using Fun_Funding.Application.ViewModel.CartDTO;
using Fun_Funding.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Application.ViewModel.UserDTO;

namespace Fun_Funding.Application.Interfaces.IEntityService
{
    public interface ICartService
    {
        Task<ResultDTO<CartInfoResponse>> GetUserCartInfo();
        Task<ResultDTO<CartInfoResponse>> AddGameToUserCart(Guid marketplaceProjectId);
        Task<ResultDTO<string>> ClearCart();
        Task<ResultDTO<string>> DeleteCartItem(Guid marketplaceProjectId);
        Task<ResultDTO<decimal>> CountUserCartItem();
    }
}
