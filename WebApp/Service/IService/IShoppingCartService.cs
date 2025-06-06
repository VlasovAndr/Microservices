﻿using WebApp.Models;

namespace WebApp.Service.IService;

public interface IShoppingCartService
{
    Task<ResponseDto?> GetCartByUserIdAsync(string userId);
    Task<ResponseDto?> UpsertCartAsync(CartDto cartDto);
    Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId);
    Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto);
    Task<ResponseDto?> EmailCart(CartDto cartDto);
}
