using AutoMapper;
using ProductAPI.Models;
using ProductAPI.Models.Dto;

namespace ProductAPI
{
	public class MappingConfig
	{
		public static MapperConfiguration RegisterMaps()
		{
			var mappingConfig = new MapperConfiguration(config =>
			{
				config.CreateMap<ProductDto, Product>();
				config.CreateMap<Product, ProductDto>();
			});
			return mappingConfig;
		}
	}
}
