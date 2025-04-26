using AutoMapper;
using MessageBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ShoppingCartAPI;
using ShoppingCartAPI.Data;
using ShoppingCartAPI.Extensions;
using ShoppingCartAPI.Service;
using ShoppingCartAPI.Service.IService;
using ShoppingCartAPI.Utility;

var builder = WebApplication.CreateBuilder(args);
// Add DbContext to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
	option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Add AutoMapper to the container.
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHandler>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IMessageBus, MessageBus.MessageBus>();
builder.Services.AddHttpClient("Product", u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"]))
	.AddHttpMessageHandler<BackendApiAuthenticationHandler>();
builder.Services.AddHttpClient("Coupon", u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponAPI"]))
	.AddHttpMessageHandler<BackendApiAuthenticationHandler>();
// Add Controllers to the container.
builder.Services.AddControllers();
// Add Swagger to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
	option.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Description = "Enter the Bearer Authorization string as following: 'Bearer Generated-JWT-Token'",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = JwtBearerDefaults.AuthenticationScheme
	});

	option.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type=ReferenceType.SecurityScheme,
					Id=JwtBearerDefaults.AuthenticationScheme
				}
			}, new string[]{ }
		}
	});
});
// Add Authentication and Authorization services.
builder.AddAppAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();


void ApplyMigration()
{
	using (var scope = app.Services.CreateScope())
	{
		var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		if (_db.Database.GetPendingMigrations().Count() > 0)
		{
			_db.Database.Migrate();
		}
	}
}
