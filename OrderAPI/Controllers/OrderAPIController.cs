﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Service.IService;
using OrderAPI.Data;
using OrderAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using OrderAPI.Utility;
using OrderAPI.Models;
using Stripe.Checkout;
using Stripe;
using MessageBus;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderAPIController : ControllerBase
{
	private ResponseDto _response;
	private IMapper _mapper;
	private readonly AppDbContext _db;
	private readonly IProductService _productService;
	private readonly IMessageBus _messageBus;
	private readonly IConfiguration _configuration;

	public OrderAPIController(
		IMapper mapper,
		AppDbContext db,
		IProductService productService,
		IMessageBus messageBus,
		IConfiguration configuration)
	{
		_response = new ResponseDto();
		_mapper = mapper;
		_db = db;
		_productService = productService;
		_messageBus = messageBus;
		_configuration = configuration;
	}

	[Authorize]
	[HttpGet("GetOrders")]
	public ResponseDto? Get(string userId = "")
	{
		try
		{
			IEnumerable<OrderHeader> objList;

			if (User.IsInRole(SD.RoleAdmin))
			{
				objList = _db.OrderHeaders
					.Include(u => u.OrderDetails)
					.OrderByDescending(u => u.OrderHeaderId)
					.ToList();
			}
			else
			{
				objList = _db.OrderHeaders
					.Include(u => u.OrderDetails)
					.Where(u => u.UserId == userId)
					.OrderByDescending(u => u.OrderHeaderId)
					.ToList();
			}

			_response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	[Authorize]
	[HttpGet("GetOrder/{id:int}")]
	public ResponseDto? Get(int id)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.OrderDetails).First(u => u.OrderHeaderId == id);
			_response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
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

			var discountsObj = new List<SessionDiscountOptions>()
			{
				new SessionDiscountOptions()
				{
					Coupon=stripeRequestDto.OrderHeader.CouponCode
				}
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

			if (stripeRequestDto.OrderHeader.Discount > 0)
			{
				options.Discounts = discountsObj;
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

	[Authorize]
	[HttpPost("ValidateStripeSession")]
	public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);

			var service = new SessionService();
			Session session = service.Get(orderHeader.StripeSessionId);

			var paymentIntentService = new PaymentIntentService();
			PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

			if (paymentIntent.Status == "succeeded")
			{
				// then payment was successful
				orderHeader.PaymentIntentId = paymentIntent.Id;
				orderHeader.Status = SD.Status_Approved;
				_db.SaveChanges();

				RewardsDto rewardsDto = new RewardsDto()
				{
					OrderId = orderHeader.OrderHeaderId,
					RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
					UserId = orderHeader.UserId
				};

				string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
				await _messageBus.PublishMessage(rewardsDto, topicName);

				_response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
			}
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}

	[Authorize]
	[HttpPost("UpdateOrderStatus/{orderId:int}")]
	public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
	{
		try
		{
			OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);

			if (newStatus == SD.Status_Cancelled)
			{
				var options = new RefundCreateOptions()
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};

				var service = new RefundService();
				Refund refund = service.Create(options);
			}
			orderHeader.Status = newStatus;
			_db.SaveChanges();

			_response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
		}
		catch (Exception ex)
		{
			_response.IsSuccess = false;
			_response.Message = ex.Message;
		}

		return _response;
	}
}
