﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApp.Models;
using WebApp.Service.IService;

namespace WebApp.Controllers
{
	public class CouponController : Controller
	{
		private readonly ICouponService _couponService;

		public CouponController(ICouponService couponService)
		{
			_couponService = couponService;
		}

		public async Task<IActionResult> CouponIndex()
		{
			List<CouponDto>? list = new List<CouponDto>();

			ResponseDto? response = await _couponService.GetAllCouponsAsync();

			if (response != null && response.IsSuccess)
			{
				list = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(list);
		}

		public async Task<IActionResult> CouponCreate()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CouponCreate(CouponDto model)
		{
			if (ModelState.IsValid)
			{
				ResponseDto? response = await _couponService.CreateCouponAsync(model);

				if (response != null && response.IsSuccess)
				{
					TempData["success"] ="Coupon created successfully";
					return RedirectToAction(nameof(CouponIndex));
				}
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

			return View(model);
		}

		public async Task<IActionResult> CouponDelete(int couponId)
		{
			ResponseDto? response = await _couponService.GetCouponsByIdAsync(couponId);

			if (response != null && response.IsSuccess)
			{
				var model = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
				return View(model);
			}

			return NotFound();
		}

		[HttpPost]
		public async Task<IActionResult> CouponDelete(CouponDto couponDto)
		{
			ResponseDto? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "Coupon deleted successfully";
				return RedirectToAction(nameof(CouponIndex));
			}
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(couponDto);
		}

	}
}
