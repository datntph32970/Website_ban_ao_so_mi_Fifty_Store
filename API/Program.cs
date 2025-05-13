using API.DbConects;
using API.Extensions;
using API.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomServices();
builder.Services.AddCustomCors();
builder.Services.AddCustomAuthentication(builder.Configuration);

// tránh vòng lặp vô hạn
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Add global filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<DiscountValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();

#region swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DATN API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
#endregion

#region DBcontext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not found.");
    }
    options.UseSqlServer(connectionString);
});
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DATN API v1"));
}

// Add CORS middleware before other middleware
app.UseCors("AllowAll");

app.UseStaticFiles();
app.UseHttpsRedirection();

// Add custom middleware
app.AddCustomMiddleware();

// Thêm middleware xác thực trước middleware phân quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
