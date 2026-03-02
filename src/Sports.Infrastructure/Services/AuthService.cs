using Microsoft.AspNetCore.Identity;
using Sports.Application.DTOs.Auth;
using Sports.Application.Exceptions;
using Sports.Application.Interfaces;
using Sports.Domain.Constants;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly SignInManager<ApplicationUser> SignInManager;
    private readonly JwtTokenGenerator JwtTokenGenerator;
    private readonly IGenericRepository<EmployeeDirectory> EmployeeRepo;
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
    /// <summary>
    /// Authenticates a user using the provided login credentials and generates an authentication token upon successful
    /// login.
    /// </summary>
    /// <remarks>The account lockout policy is enforced during authentication. After multiple failed login
    /// attempts, the account may be temporarily locked. On successful login, the failed access count is
    /// reset.</remarks>
    /// <param name="loginDto">An object containing the user's email and password used for authentication. Cannot be null.</param>
    /// <returns>An AuthResponseDto containing user information and a JWT token if authentication is successful.</returns>
    /// <exception cref="ValidationException">Thrown if the email or password is invalid, or if the maximum number of failed login attempts is nearly reached.</exception>
    /// <exception cref="AccountLockedException">Thrown if the user's account is locked due to too many failed login attempts.</exception>
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await UserManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new ValidationException("Credentials", MessageStrings.InvalidCredentials);
        }

        //  CHECK IF ACCOUNT IS LOCKED
        if (await UserManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await UserManager.GetLockoutEndDateAsync(user);
            var minutesRemaining = (int)(lockoutEnd!.Value - DateTimeOffset.UtcNow).TotalMinutes + 1;
            throw new AccountLockedException(minutesRemaining);
        }

        //  CHECK PASSWORD WITH LOCKOUT ENABLED
        var result = await SignInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                throw new AccountLockedException(15);
            }

            // Get remaining attempts
            var failedCount = await UserManager.GetAccessFailedCountAsync(user);
            var remaining = 5 - failedCount;
            
            throw new ValidationException("Credentials", MessageStrings.InvalidCredentialsWithAttempts(remaining));
        }

        //  RESET FAILED COUNT ON SUCCESSFUL LOGIN
        await UserManager.ResetAccessFailedCountAsync(user);

        var roles = await UserManager.GetRolesAsync(user);
        var token = JwtTokenGenerator.GenerateToken(user, roles);

        Console.WriteLine($" User logged in: {user.Email} | Roles: {string.Join(", ", roles)}");

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(1)
        };
    }
    /// <summary>
    /// RegisterAsync
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    /// <exception cref="DuplicateException"></exception>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // 1. Validation: Check if user exists in Employee Directory
        var employees = await EmployeeRepo.FindAsync(
            e => e.Email == registerDto.Email && e.EmployeeCode == registerDto.EmployeeCode);
        var employee = employees.FirstOrDefault();

        if (employee == null)
        {
            throw new ValidationException("EmployeeCode", MessageStrings.EmployeeDetailsMismatch);
        }

        if (!employee.IsActive)
        {
            throw new ValidationException("Employee", MessageStrings.EmployeeInactive );
        }

        // Check if user already exists
        var existingUser = await UserManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new DuplicateException(MessageStrings.UserAlreadyExists);
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
            var errors = result.Errors.ToDictionary(
                e => e.Code,
                e => new[] { e.Description });
            throw new ValidationException(errors);
        }

        // 3. Link User to Employee
        await MapRepo.AddAsync(new UserEmployeeMap
        {
            UserId = user.Id,
            EmployeeId = employee.EmployeeId
        });
        await Uow.SaveChangesAsync();

        // 4. Add Default Role
        await UserManager.AddToRoleAsync(user, "User");

        // 5. Generate Token
        var roles = new List<string> { "User" };
        var token = JwtTokenGenerator.GenerateToken(user, roles);

        Console.WriteLine($"New user registered: {user.Email}");

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
