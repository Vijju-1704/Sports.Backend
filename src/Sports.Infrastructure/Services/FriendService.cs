using Microsoft.EntityFrameworkCore;
using Sports.Application.Interfaces;
using Sports.Domain.Entities;
using Sports.Infrastructure.Data;
using Sports.Infrastructure.Identity;

namespace Sports.Infrastructure.Services;

public class FriendService : IFriendService
{
    private readonly ApplicationDbContext _context;

    public FriendService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SendFriendRequestAsync(string requesterId, string addresseeId)
    {
        if (requesterId == addresseeId) return false;

        var existing = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

        if (existing != null) return false;

        var friendship = new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AcceptFriendRequestAsync(int friendshipId, string userId)
    {
        var friendship = await _context.Friendships.FindAsync(friendshipId);
        
        // Only the addressee can accept
        if (friendship == null || friendship.AddresseeId != userId) return false;

        friendship.Status = FriendshipStatus.Accepted;
        friendship.ActionedAt = DateTime.UtcNow;
        
        // Log to Activity Feed
        var feed = new ActivityFeed 
        {
            ActorId = userId,
            Type = ActivityType.FriendRequestAccepted,
            TargetId = friendship.RequesterId,
            Description = "became friends with " + friendship.RequesterId
        };
        _context.ActivityFeeds.Add(feed);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeclineFriendRequestAsync(int friendshipId, string userId)
    {
        var friendship = await _context.Friendships.FindAsync(friendshipId);
        if (friendship == null || friendship.AddresseeId != userId) return false;

        friendship.Status = FriendshipStatus.Declined;
        friendship.ActionedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Friendship>> GetFriendsAsync(string userId)
    {
        // Manual join logic if we ever needed names, but we are returning Entities
        return await _context.Friendships
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(string userId)
    {
        return await _context.Friendships
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityFeed>> GetActivityFeedAsync(string userId)
    {
        var friends1 = await _context.Friendships
            .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.AddresseeId)
            .ToListAsync();
            
        var friends2 = await _context.Friendships
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Accepted)
            .Select(f => f.RequesterId)
            .ToListAsync();
            
        var friendIds = friends1.Concat(friends2).ToList();

        return await _context.ActivityFeeds
            .Where(a => friendIds.Contains(a.ActorId))
            .OrderByDescending(a => a.OccurredAt)
            .Take(20)
            .ToListAsync();
    }
}
