using Microsoft.AspNetCore.Identity;
using Sports.Application.DTOs.Auth;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IGenericRepository<EmployeeDirectory> _employeeRepo; // Direct repo access for check
    private readonly IGenericRepository<UserEmployeeMap> _mapRepo;
    private readonly IUnitOfWork _uow;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _uow = uow;
        _employeeRepo = _uow.Repository<EmployeeDirectory>();
        _mapRepo = _uow.Repository<UserEmployeeMap>();
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new Exception("Invalid Username or Password"); // In real app, use custom exception
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
        {
            throw new Exception("Invalid Username or Password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

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
        var employees = await _employeeRepo.FindAsync(e => e.Email == registerDto.Email && e.EmployeeCode == registerDto.EmployeeCode);
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

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Registration Failed: {errors}");
        }

        // 3. Link User to Employee
        await _mapRepo.AddAsync(new UserEmployeeMap
        {
            UserId = user.Id,
            EmployeeId = employee.EmployeeId
        });
        await _uow.SaveChangesAsync();

        // 4. Default Role
        await _userManager.AddToRoleAsync(user, "User");

        // 5. Generate Token
        var roles = new List<string> { "User" };
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

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
