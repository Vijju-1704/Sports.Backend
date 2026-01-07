using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sports.Domain.Entities;
using Sports.Domain.Interfaces;

namespace Sports.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<UserSportProfile> _profileRepo;
    private readonly IGenericRepository<Game> _gameRepo;
    private readonly IGenericRepository<GameParticipant> _participantRepo;

    public UserProfileController(IUnitOfWork uow)
    {
        _uow = uow;
        _profileRepo = uow.Repository<UserSportProfile>();
        _gameRepo = uow.Repository<Game>();
        _participantRepo = uow.Repository<GameParticipant>();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // Get Sport Profiles (Skills)
        var sportProfiles = await _profileRepo.FindAsync(p => p.UserId == userId);

        // Get Stats (Games Hosted, Games Joined)
        var hostedCount = (await _gameRepo.FindAsync(g => g.HostUserId == userId)).Count();
        var joinedCount = (await _participantRepo.FindAsync(p => p.UserId == userId)).Count();

        return Ok(new 
        { 
            SportProfiles = sportProfiles,
            Stats = new { Hosted = hostedCount, Joined = joinedCount }
        });
    }

    [HttpPost("skills")]
    public async Task<IActionResult> UpdateSkill([FromBody] UserSportProfile dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var existing = (await _profileRepo.FindAsync(p => p.UserId == userId && p.SportId == dto.SportId)).FirstOrDefault();
        if (existing != null)
        {
            existing.SkillLevel = dto.SkillLevel;
        }
        else
        {
            dto.UserId = userId;
             await _profileRepo.AddAsync(dto);
        }
        
        await _uow.SaveChangesAsync();
        
        return Ok(new { message = "Skill updated" });
    }
}
