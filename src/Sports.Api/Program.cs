using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Sports.Infrastructure.Data;
using Sports.Infrastructure.Identity;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Repositories;
using Sports.Application.Interfaces;
using Sports.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ ENHANCED SWAGGER CONFIGURATION WITH JWT SUPPORT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Sports System API",
        Description = "Corporate Sports Management System - ASP.NET Core Web API",
        Contact = new OpenApiContact
        {
            Name = "Sports System",
            Email = "support@sportsapp.com"
        }
    });

    // ✅ ADD JWT AUTHENTICATION TO SWAGGER
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// 1. Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userEmail = context.Principal?.FindFirst("email")?.Value ?? "Unknown";
            var roles = context.Principal?.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
            Console.WriteLine($"✅ JWT Token validated for: {userEmail} | Roles: {string.Join(", ", roles)}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"⚠️ JWT Challenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// 4. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001",
                "https://localhost:7086",
                "http://localhost:5086"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 5. Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 6. JWT Token Generator
builder.Services.AddScoped<JwtTokenGenerator>();

// 7. Application Services
builder.Services.AddScoped<IJoinRequestService, JoinRequestService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminService>(sp =>
    new AdminService(
        sp.GetRequiredService<IUnitOfWork>(),
        sp.GetRequiredService<UserManager<ApplicationUser>>(),
        sp.GetRequiredService<RoleManager<IdentityRole>>()
    ));
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// 8. Seed Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    Console.WriteLine("\n🔧 Setting up database and roles...\n");

    // Seed Roles
    var roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Created role: {role}");
        }
    }

    // Seed Admin User
    var adminEmail = "admin@techcorp.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Admin user created: {adminEmail}");

            // Link to employee directory
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            var adminEmployee = dbContext.EmployeeDirectory.FirstOrDefault(e => e.Email == adminEmail);
            if (adminEmployee != null)
            {
                dbContext.UserEmployeeMap.Add(new Sports.Domain.Entities.UserEmployeeMap
                {
                    UserId = adminUser.Id,
                    EmployeeId = adminEmployee.EmployeeId
                });
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"✅ Admin linked to employee directory");
            }
        }
        else
        {
            Console.WriteLine($"❌ Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        var existingRoles = await userManager.GetRolesAsync(adminUser);
        if (!existingRoles.Contains("Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Added Admin role to existing admin user");
        }
        Console.WriteLine($"✅ Admin user exists: {adminEmail} | Roles: {string.Join(", ", existingRoles)}");
    }

    Console.WriteLine("\n✨ Database setup complete!\n");
}

// ✅ CONFIGURE SWAGGER FOR ALL ENVIRONMENTS
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sports System API v1");
    options.RoutePrefix = "swagger"; // Access at /swagger
    options.DocumentTitle = "Sports System API";
    options.DisplayRequestDuration(); // Show request duration
    options.EnableDeepLinking(); // Enable deep linking
    options.EnableFilter(); // Enable search filter
    options.ShowExtensions(); // Show vendor extensions
});

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ LOG STARTUP INFORMATION
var baseUrl = app.Environment.IsDevelopment()
    ? "https://localhost:7164"
    : "https://yourdomain.com";

Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("🚀 SPORTS SYSTEM API - RUNNING");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"📍 API URL:     {baseUrl}");
Console.WriteLine($"📚 Swagger UI:  {baseUrl}/swagger");
Console.WriteLine($"📄 OpenAPI:     {baseUrl}/swagger/v1/swagger.json");
Console.WriteLine(new string('=', 60));
Console.WriteLine("\n💡 Quick Test:");
Console.WriteLine($"   Login: POST {baseUrl}/api/auth/login");
Console.WriteLine("   Credentials: admin@techcorp.com / Admin@123");
Console.WriteLine("\n⚠️  Don't forget to authorize in Swagger with your JWT token!");
Console.WriteLine(new string('=', 60) + "\n");

app.Run();