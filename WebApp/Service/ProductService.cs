﻿using WebApp.Models;
using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Service
{
	public class ProductService : IProductService
	{
		private readonly IBaseService _baseService;

		public ProductService(IBaseService baseService)
		{
			_baseService = baseService;
		}

		public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = productDto,
				Url = SD.ProductAPIBaseUrl + "/api/product/"
			});
		}

		public async Task<ResponseDto?> DeleteProductAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.DELETE,
				Url = SD.ProductAPIBaseUrl + "/api/product/" + id
			});
		}

		public async Task<ResponseDto?> GetAllProductsAsync()
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.GET,
				Url = SD.ProductAPIBaseUrl + "/api/product/"
			});
		}

		public async Task<ResponseDto?> GetProductByIdAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.GET,
				Url = SD.ProductAPIBaseUrl + "/api/product/" + id
			});
		}

		public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.PUT,
				Data = productDto,
				Url = SD.ProductAPIBaseUrl + "/api/product/"
			});
		}
	}
}
