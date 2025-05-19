using AutoMapper;
using MessageBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Models.Dto;
using ShoppingCartAPI.Service.IService;

namespace ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
	private ResponseDto _response;
	private IMapper _mapper;
	private readonly AppDbContext _db;
	private readonly IProductService _productService;
	private readonly ICouponService _couponService;
	private readonly IMessageBus _messageBus;
	private readonly IConfiguration _configuration;

	public CartAPIController(
		IMapper mapper,
		AppDbContext db,
		IProductService productService,
		ICouponService couponService,
		IMessageBus messageBus,
		IConfiguration configuration)
	{
		_response = new ResponseDto();
		_mapper = mapper;
		_db = db;
		_productService = productService;
		_couponService = couponService;
		_messageBus = messageBus;
		_configuration = configuration;
	}

	[HttpGet("GetCart/{userId}")]
	public async Task<ResponseDto> GetCart(string userId)
	{
		try
		{
			CartDto cart = new CartDto()
			{
				CartHeader = _mapper
				.Map<CartHeaderDto>(_db.CartHeaders.First(u => u.UserId == userId))
			};
			cart.CartDetails = _mapper
				.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails.Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

			var productDtos = await _productService.GetProducts();

			foreach (var item in cart.CartDetails)
			{
				item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
				cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
			}

			//apply coupon if exists
			if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
			{
				var coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
				if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
				{
					cart.CartHeader.CartTotal -= coupon.DiscountAmount;
					cart.CartHeader.Discount = coupon.DiscountAmount;
				}
			}

			_response.Result = cart;
		}
		catch (Exception ex)
		{
			_response.Message = ex.Message.ToString();
			_response.IsSuccess = false;
		}
		return _response;
	}

	[HttpPost("ApplyCoupon")]
	public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
	{
		try
		{
			var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
			cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
			_db.CartHeaders.Update(cartFromDb);
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

	[HttpPost("EmailCartRequest")]
	public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
	{
		try
		{
			await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
			_response.Result = true;
		}
		catch (Exception ex)
		{
			_response.Message = ex.Message.ToString();
			_response.IsSuccess = false;
		}
		return _response;
	}

	[HttpPost("RemoveCoupon")]
	public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
	{
		try
		{
			var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
			cartFromDb.CouponCode = "";
			_db.CartHeaders.Update(cartFromDb);
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
