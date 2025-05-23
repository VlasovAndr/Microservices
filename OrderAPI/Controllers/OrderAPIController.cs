using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Service.IService;
using OrderAPI.Data;
using OrderAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using OrderAPI.Utility;
using OrderAPI.Models;
using Stripe.Checkout;

namespace OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
	private ResponseDto _response;
	private IMapper _mapper;
	private readonly AppDbContext _db;
	private readonly IProductService _productService;

	public OrderAPIController(
		IMapper mapper,
		AppDbContext db,
		IProductService productService
		)
	{
		_response = new ResponseDto();
		_mapper = mapper;
		_db = db;
		_productService = productService;
	}

	[Authorize]
	[HttpPost("CreateOrder")]
	public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
	{
		try
		{
			OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
			orderHeaderDto.OrderTime = DateTime.Now;
			orderHeaderDto.Status = SD.Status_Pending;
			orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

			OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
			await _db.SaveChangesAsync();

			orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
			_response.Result = orderHeaderDto;
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	[Authorize]
	[HttpPost("CreateStripeSession")]
	public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
	{
		try
		{
			var options = new SessionCreateOptions
			{
				SuccessUrl = stripeRequestDto.ApprovedUrl,
				CancelUrl = stripeRequestDto.CancelUrl,
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
			};

			foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100), // $20.99 -> 2099
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Name
						}
					},
					Quantity = item.Count
				};

				options.LineItems.Add(sessionLineItem);
			}

			var service = new SessionService();
			Session session = service.Create(options);

			stripeRequestDto.StripeSessionUrl = session.Url;

			OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
			orderHeader.StripeSessionId = session.Id;
			_db.SaveChanges();
			_response.Result = stripeRequestDto;
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

}
