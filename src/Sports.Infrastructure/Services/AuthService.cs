using Microsoft.AspNetCore.Identity;
using Sports.Application.DTOs.Auth;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly SignInManager<ApplicationUser> SignInManager;
    private readonly JwtTokenGenerator JwtTokenGenerator;
    private readonly IGenericRepository<EmployeeDirectory> EmployeeRepo; // Direct repo access for check
    private readonly IGenericRepository<UserEmployeeMap> MapRepo;
    private readonly IUnitOfWork Uow;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork uow)
    {
        UserManager = userManager;
        SignInManager = signInManager;
        JwtTokenGenerator = jwtTokenGenerator;
        Uow = uow;
        EmployeeRepo = Uow.Repository<EmployeeDirectory>();
        MapRepo = Uow.Repository<UserEmployeeMap>();
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await UserManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new Exception("Invalid Username or Password"); // In real app, use custom exception
        }

        var result = await SignInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            throw new Exception("Invalid Username or Password");
        }

        var roles = await UserManager.GetRolesAsync(user);
        var token = JwtTokenGenerator.GenerateToken(user, roles);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // 1. Validation: Check if user exists in Employee Directory
        var employees = await EmployeeRepo.FindAsync(e => e.Email == registerDto.Email && e.EmployeeCode == registerDto.EmployeeCode);
        var employee = employees.FirstOrDefault();

        if (employee == null)
        {
            throw new Exception("Registration Failed: Details do not match our Employee Directory.");
        }

        if (!employee.IsActive)
        {
             throw new Exception("Registration Failed: Employee is not active.");
        }

        // 2. Create Identity User
        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            DateRegistered = DateTime.UtcNow
        };

        var result = await UserManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration Failed: {errors}");
        }

        // 3. Link User to Employee
        await MapRepo.AddAsync(new UserEmployeeMap
        {
            UserId = user.Id,
            EmployeeId = employee.EmployeeId
        });
        await Uow.SaveChangesAsync();

        // 4. Default Role
        await UserManager.AddToRoleAsync(user, "User");

        // 5. Generate Token
        var roles = new List<string> { "User" };
        var token = JwtTokenGenerator.GenerateToken(user, roles);

         return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(1)
        };
    }
}
