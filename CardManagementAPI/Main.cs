using Microsoft.EntityFrameworkCore;
using CardManagementAPI.Data;
using CardManagementAPI.Models;
using CardManagementAPI.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<ApiDataContext>(opt => opt.UseInMemoryDatabase("CardDb"));

builder.Services.AddSingleton<IPaymentSingleton, UFEFee>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = new PathString("/api/CardManagementAPI/auth/login");
            options.AccessDeniedPath = new PathString("/api/CardManagementAPI/auth/denied");
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Employee", policy => policy.RequireClaim("role", "Employee"));
    options.AddPolicy("Customer", policy => policy.RequireClaim("role", "Customer"));
    options.AddPolicy("Administrator", policy => policy.RequireClaim("role", "Administrator"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Card Management API Demo", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token:",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
