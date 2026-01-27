using Sports.Domain.Entities;

namespace Sports.Application.Interfaces;

public interface IFriendService
{
    Task<bool> SendFriendRequestAsync(string requesterId, string addresseeId);
    Task<bool> AcceptFriendRequestAsync(int friendshipId, string userId);
    Task<bool> DeclineFriendRequestAsync(int friendshipId, string userId);
    Task<IEnumerable<Friendship>> GetFriendsAsync(string userId);
    Task<IEnumerable<Friendship>> GetPendingRequestsAsync(string userId);
    Task<IEnumerable<ActivityFeed>> GetActivityFeedAsync(string userId);
}
