using AutoMapper;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Application.ExceptionHandler;
using System.Net;
using System.Security.Claims;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel.CartDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Identity;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class CartService : ICartService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public CartService(IMapper mapper, IUnitOfWork unitOfWork, IUserService userService, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ResultDTO<CartInfoResponse>> GetUserCartInfo()
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User existUser = _mapper.Map<User>(user._data);
                Cart? cart = _unitOfWork.CartRepository.GetQueryable().Where(cart => cart.UserId == existUser.Id).SingleOrDefault();
                if(cart == null)
                {
                    cart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = existUser.Id,
                        Items = new List<BsonDocument>()
                    };
                    await _unitOfWork.CartRepository.CreateAsync(cart);
                }
                var cartResponse = _mapper.Map<CartInfoResponse>(_unitOfWork.CartRepository.GetQueryable().Where(cart => cart.UserId == existUser.Id).SingleOrDefault());
                return ResultDTO<CartInfoResponse>.Success(cartResponse, "Cart found!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<ResultDTO<CartInfoResponse>> AddGameToUserCart(Guid marketplaceProjectId)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                var existUser = _mapper.Map<User>(user._data);

                var roles = await _userManager.GetRolesAsync(existUser);
                if (roles.Contains(Role.GameOwner))
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "Game Owner cannot purchase games.");
                }
                else if (roles.Contains(Role.Admin))
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "Admin cannot purchase games.");
                }

                var cart = _unitOfWork.CartRepository
                    .GetQueryable()
                    .SingleOrDefault(c => c.UserId == existUser.Id);

                var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetByIdAsync(marketplaceProjectId);
                if (marketplaceProject == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace project not found.");
                }
                if (marketplaceProject.Status != Domain.Enum.ProjectStatus.Processing)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Marketplace project is not viable for purchase!");
                }

                bool itemExists = cart != null && cart.Items.Any(item =>
                    item.Contains("marketplaceProjectId") &&
                    item["marketplaceProjectId"].AsGuid == marketplaceProjectId);

                if (itemExists)
                {
                    throw new ExceptionError((int)HttpStatusCode.Conflict, "The project is already in the cart.");
                }

                var newItem = new BsonDocument
                {
                    { "marketplaceProjectId", marketplaceProject.Id },
                    { "createdDate", DateTime.Now }
                };

                if (cart == null)
                {
                    cart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = existUser.Id,
                        Items = new List<BsonDocument> { newItem }
                    };
                    await _unitOfWork.CartRepository.CreateAsync(cart);
                }
                else
                {
                    var updateDefinition = Builders<Cart>.Update.Push(x => x.Items, newItem);
                    _unitOfWork.CartRepository.Update(x => x.Id == cart.Id, updateDefinition);
                }

                var cartInfoResponse = _mapper.Map<CartInfoResponse>(
                    _unitOfWork.CartRepository.GetQueryable().SingleOrDefault(c => c.UserId == existUser.Id));
                return ResultDTO<CartInfoResponse>.Success(cartInfoResponse, "Added to cart successfully!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public async Task<ResultDTO<string>> DeleteCartItem(Guid marketplaceProjectId)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                var existUser = _mapper.Map<User>(user._data);

                var cart = _unitOfWork.CartRepository
                    .GetQueryable()
                    .SingleOrDefault(c => c.UserId == existUser.Id);

                if (cart == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Cart not found.");
                }

                var updateDefinition = Builders<Cart>.Update.PullFilter(
                    c => c.Items,
                    Builders<BsonDocument>.Filter.Eq("marketplaceProjectId", marketplaceProjectId)
                );

                _unitOfWork.CartRepository.Update(x => x.Id == cart.Id, updateDefinition);
                return ResultDTO<string>.Success("", "Item removed from cart successfully!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<string>> ClearCart()
        {
            try
            {
                var user = await _userService.GetUserInfo();
                var existUser = _mapper.Map<User>(user._data);

                var cart = _unitOfWork.CartRepository
                    .GetQueryable()
                    .SingleOrDefault(c => c.UserId == existUser.Id);

                if (cart == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Cart not found.");
                }

                var updateDefinition = Builders<Cart>.Update.Set(c => c.Items, new List<BsonDocument>());

                _unitOfWork.CartRepository.Update(x => x.Id == cart.Id, updateDefinition);
                return ResultDTO<string>.Success("", "Item removed from cart successfully!");
            }
            catch (Exception ex) when (ex is not ExceptionError)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<decimal>> CountUserCartItem()
        {
            try
            {
                var user = await _userService.GetUserInfo();
                var existUser = _mapper.Map<User>(user._data);
                var cart = _unitOfWork.CartRepository
                    .GetQueryable()
                    .SingleOrDefault(c => c.UserId == existUser.Id);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = existUser.Id,
                        Items = new List<BsonDocument>()
                    };
                    await _unitOfWork.CartRepository.CreateAsync(cart);
                    return ResultDTO<decimal>.Success(0, "Cart found!");
                }
                return ResultDTO<decimal>.Success(cart.Items.Count, "Cart found!");
            }
            catch (Exception ex) when (ex is not ExceptionError)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
