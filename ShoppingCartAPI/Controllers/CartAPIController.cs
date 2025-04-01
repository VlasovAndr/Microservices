﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Models.Dto;

namespace ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private ResponseDto _response;
    private IMapper _mapper;
    private readonly AppDbContext _db;

    public CartAPIController(IMapper mapper, AppDbContext db)
    {
        _response = new ResponseDto();
        _mapper = mapper;
        _db = db;
    }

    [HttpPost("CartUpsert")]
    public async Task<ResponseDto> CartUpsert(CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                //create header and details
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _db.CartHeaders.Add(cartHeader);
                await _db.SaveChangesAsync();

                cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                await _db.SaveChangesAsync();
            }
            else
            {
                // if header is not null
                // check if details has same product
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    u => u.ProductId == cartDto.CartDetails.First().ProductId
                    && u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if (cartDetailsFromDb == null)
                {
                    // create cart details
                    cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();

                }
                else
                {
                    // update count in cart details
                    cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                    cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
            }
            _response.Result = cartDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }
        return _response;
    }

    [HttpPost("RemoveCart")]
    public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
    {
        try
        {
            CartDetails cartDetails = _db.CartDetails.First(u => u.CartDetailsId == cartDetailsId);
            int totalCountOfCartItem = _db.CartDetails.Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

            _db.CartDetails.Remove(cartDetails);


            if (totalCountOfCartItem == 1)
            {
                var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                _db.CartHeaders.Remove(cartHeaderToRemove);
            }

            await _db.SaveChangesAsync();
            _response.Result = true;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }
        return _response;
    }
}
