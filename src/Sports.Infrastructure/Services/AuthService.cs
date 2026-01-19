using Microsoft.AspNetCore.Identity;
using Sports.Application.DTOs.Auth;
using Sports.Application.Exceptions;
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
    private readonly IGenericRepository<EmployeeDirectory> _employeeRepo;
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
            throw new ValidationException("Credentials", "Invalid email or password.");
        }

        // ✅ CHECK IF ACCOUNT IS LOCKED
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var minutesRemaining = (int)(lockoutEnd!.Value - DateTimeOffset.UtcNow).TotalMinutes + 1;
            throw new AccountLockedException(minutesRemaining);
        }

        // ✅ CHECK PASSWORD WITH LOCKOUT ENABLED
        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new AccountLockedException(15);
            }

            // Get remaining attempts
            var failedCount = await _userManager.GetAccessFailedCountAsync(user);
            var remaining = 5 - failedCount;
            
            throw new ValidationException("Credentials", $"Invalid email or password. {remaining} attempt(s) remaining.");
        }

        // ✅ RESET FAILED COUNT ON SUCCESSFUL LOGIN
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        Console.WriteLine($"✅ User logged in: {user.Email} | Roles: {string.Join(", ", roles)}");

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
        var employees = await _employeeRepo.FindAsync(
            e => e.Email == registerDto.Email && e.EmployeeCode == registerDto.EmployeeCode);
        var employee = employees.FirstOrDefault();

        if (employee == null)
        {
            throw new ValidationException("EmployeeCode", "Registration failed. Details do not match our Employee Directory.");
        }

        if (!employee.IsActive)
        {
            throw new ValidationException("Employee", "Registration failed. Employee is not active.");
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new DuplicateException("A user with this email address already exists.");
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
            var errors = result.Errors.ToDictionary(
                e => e.Code,
                e => new[] { e.Description });
            throw new ValidationException(errors);
        }

        // 3. Link User to Employee
        await _mapRepo.AddAsync(new UserEmployeeMap
        {
            UserId = user.Id,
            EmployeeId = employee.EmployeeId
        });
        await _uow.SaveChangesAsync();

        // 4. Add Default Role
        await _userManager.AddToRoleAsync(user, "User");

        // 5. Generate Token
        var roles = new List<string> { "User" };
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        Console.WriteLine($"✅ New user registered: {user.Email}");

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
