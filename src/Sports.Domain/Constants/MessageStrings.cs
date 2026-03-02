namespace Sports.Domain.Constants
{
    public class MessageStrings
    {
        // Sport
        public const string SportInUse = "Cannot delete sport that is being used in games";

        // Venue
        public const string VenueInUse = "Cannot delete venue that is being used in games";


        // Role (Dynamic message)
        public static string RoleDoesNotExist(string role)
            => $"Role '{role}' does not exist";


        // Generic
        public const string Unknown = "Unknown";

        //Authentication Messages
        public const string InvalidCredentials ="Invalid email or password.";
        public static string InvalidCredentialsWithAttempts(int remaining)
            => $"Invalid email or password. {remaining} attempt(s) remaining.";

        public const string UserAlreadyExists = "A user with this email address already exists.";

        // Employee Messages

        public const string EmployeeExists = "Employee with this email or code already exists";

        public const string EmployeeInactive = "Registration failed. Employee is not active.";

        public const string EmployeeDetailsMismatch = "Registration failed. Details do not match our Employee Directory.";
        public const string MustBeParticipantToSendMessage = "You must be a participant to send messages";

        public const string MaxPlayersMustBeGreaterOrEqualMinPlayers = "Max players must be greater than or equal to Min players.";


        // Game Validation Messages
        public const string GameDateInPast =
            "Game date must not be in the past.";

        public const string HostCannotLeave =
            "Host cannot leave their own game. Cancel it instead.";

        public const string OnlyHostCanUpdateStatus =
            "Only the host can update game status.";

        public const string OnlyHostCanUpdateGame =
            "Only the host can update the game.";

        public const string OnlyHostCanCancelGame =
            "Only the host can cancel the game.";


        // Notification Messages
        public static string GameCompletedRate(string title) =>
            $"Game '{title}' has been completed! Rate your teammates now.";

        public static string GameUpdated(string title) =>
            $"Game '{title}' has been updated by the host.";

        public static string GameCancelled(string title) =>
            $"Game '{title}' has been cancelled by the host.";

        public static string GameStatusChanged(string title, string statusMessage) =>
            $"Game '{title}' {statusMessage}";

        public static string UserJoinedGame(string userName, string title) =>
            $"{userName} joined your game '{title}'";

        public static string UserLeftGame(string userName, string title) =>
            $"{userName} left your game '{title}'";

        public const string Someone = "Someone";


        // Join Request / Status
        public const string PendingStatus = "Pending";

        public const string NotAParticipantInGame = "You are not a participant in this game.";


        // Background Service Messages

        public const string GameStatusServiceStarted =
            "Game Status Background Service started";

        public const string GameStatusServiceStopped =
            "Game Status Background Service stopped";

        public const string ErrorUpdatingGameStatuses =
            "Error updating game statuses";

        public const string CheckingGameStatusUpdates =
            "Checking for games that need status updates...";

        // Join Request Validation Messages

        public const string GameNotFound =
            "Game not found";

        public const string GameDoesNotRequireApproval =
            "This game does not require approval";

        public const string AlreadyHasPendingJoinRequest =
            "You already have a pending request for this game";

        public const string AlreadyParticipantInGame =
            "You are already a participant in this game";

        public const string GameIsNowFull =
            "Game is now full";


        // Join Request Notification Messages

        public static string UserRequestedToJoin(string userName, string gameTitle) => $"{userName} requested to join your game '{gameTitle}'";

        public static string JoinRequestDeclinedGameFull(string gameTitle) => $"Your request to join '{gameTitle}' was declined - game is full";

        public static string JoinRequestApproved(string gameTitle) => $"Your request to join '{gameTitle}' was approved!";

        public static string JoinRequestDeclined(string gameTitle) => $"Your request to join '{gameTitle}' was declined";

        public const string SportProfileAlreadyExists = "Sport profile already exists for this sport";

        // Exception Messages
        public const string ValidationErrorsOccurred = "One or more validation errors occurred.";

        public const string ValidationError = "Validation error.";
        public const string ThisGameCancelled ="This game has been cancelled.";

        public const string NotAuthorized = "You are not authorized to perform this action.";

        public const string GameAlreadyFull = "This game is already full.";

        public const string AlreadyJoinedGame = "You have already joined this game.";

        public const string CannotJoinPastGame = "Cannot join a game that has already started or ended.";

        public static string EntityNotFound(string entityName, object key) => $"{entityName} with id '{key}' was not found.";

        public static string AccountLocked(int minutesRemaining) => $"Account locked due to multiple failed attempts. Try again in {minutesRemaining} minutes.";


        // User Profile Messages

        public const string FailedToUpdateProfile = "Failed to update profile";

        public const string ProfileUpdatedSuccessfully = "Profile updated successfully";

        public const string SportProfileUpdated = "Sport profile updated";

        public const string SportProfileRemoved = "Sport profile removed";

        public const string RatingNotificationsSent = "Rating notifications sent";

        public const string NotificationMarkedAsRead = "Notification marked as read";

        public const string AllNotificationsMarkedAsRead = "All notifications marked as read";

        public const string NotificationDeleted = "Notification deleted";

        public const string AllNotificationsDeleted = "All notifications deleted";

        // Games Controller Messages

        public const string UnableToUpdateGame = "Unable to update game (not found or not host)";

        public const string UnableToJoinGame = "Unable to join game (Full, Closed, or Already Joined)";

        public const string JoinedSuccessfully = "Joined successfully";

        public const string UnableToLeaveGame = "Unable to leave game (Not a participant or you are the host)";

        public const string LeftGameSuccessfully = "Left game successfully";

        public const string UnableToCancelGame = "Unable to cancel game (Not Authorized or Not Found)";

        public const string GameCancelledSuccessfully = "Game cancelled";

        public const string UnableToUpdateGameStatus = "Unable to update game status";

        public const string GameStatusUpdated = "Game status updated";

        public const string UnableToDeleteMessage = "Unable to delete message";

        // Admin - Sport Messages

        public const string SportUpdatedSuccessfully =
            "Sport updated successfully";

        public const string SportDeletedSuccessfully =
            "Sport deleted successfully";


        // Admin - Venue Messages

        public const string VenueUpdatedSuccessfully =
            "Venue updated successfully";

        public const string VenueDeletedSuccessfully =
            "Venue deleted successfully";


        // Admin - Employee Messages

        public const string EmployeeCreatedSuccessfully =
            "Employee created successfully";

        public const string EmployeeUpdatedSuccessfully =
            "Employee updated successfully";

        public const string EmployeeDeactivated =
            "Employee deactivated";

        public const string EmployeeActivated =
            "Employee activated";


        // Admin - User Messages

        public const string UserDeactivated =
            "User deactivated";

        public const string UserActivated =
            "User activated";

        public static string UserRoleChanged(string role) =>
            $"User role changed to {role}";

        // Chat Hub - Log Messages

        public const string UserJoinedGameRoomLog =
            "User {UserId} joined game room {GameId}";

        public const string UserLeftGameRoomLog =
            "User {UserId} left game room {GameId}";

        public const string UnauthorizedSendMessageAttempt =
            "Unauthorized send message attempt";

        public const string MessageSentLog =
            "Message sent in game {GameId} by user {UserId}";

        public const string ErrorSendingMessageLog =
            "Error sending message in game {GameId}";

        public const string UserConnectedToChatHub =
            "User {UserId} connected to ChatHub";

        public const string UserDisconnectedFromChatHub =
            "User {UserId} disconnected from ChatHub";


        // Chat Hub - Client Messages

        public const string UserJoinedChatMessage = "A user joined the chat";

        public const string FailedToSendMessage = "Failed to send message";


        // SignalR Event Names

        public const string SignalRUserJoinedEvent = "UserJoined";

        public const string SignalRReceiveMessageEvent = "ReceiveMessage";

        public const string SignalRErrorEvent = "Error";


        // SignalR Group Prefix

        public const string GameGroupPrefix = "game_";

        // Notification Hub - Log Messages

        public const string UserConnectedToNotificationHub = "User {UserId} connected to NotificationHub";

        public const string UserDisconnectedFromNotificationHub = "User {UserId} disconnected from NotificationHub";

        public const string UserMarkedNotificationAsRead = "User {UserId} marked notification {NotificationId} as read";


        // Notification Hub - SignalR Events

        public const string SignalRNotificationReadEvent = "NotificationRead";

        public const string SignalRReceiveNotificationEvent = "ReceiveNotification";


        // Notification Hub - Group Prefix
          
        public const string UserGroupPrefix = "user_";

        // Global Exception - Log Messages

        public const string UnhandledExceptionLog = "Unhandled exception occurred: {Message}";


        // Global Exception - Titles

        public const string ResourceNotFoundTitle = "Resource Not Found";

        public const string ValidationErrorTitle = "Validation Error";

        public const string UnauthorizedTitle = "Unauthorized";

        public const string GameFullTitle = "Game Full";

        public const string AlreadyJoinedTitle = "Already Joined";

        public const string GameCancelledTitle = "Game Cancelled";

        public const string PastGameTitle = "Past Game";

        public const string DuplicateEntryTitle = "Duplicate Entry";

        public const string AccountLockedTitle = "Account Locked";

        public const string EntityInUseTitle = "Entity In Use";

        public const string InternalServerErrorTitle = "Internal Server Error";


        // Global Exception - Messages

        public const string GameFullMessage = "This game is already full. Please try another game.";

        public const string AlreadyJoinedMessage = "You have already joined this game.";

        public const string GameCancelledMessage = "This game has been cancelled.";

        public const string PastGameMessage = "Cannot perform this action on a game that has already started.";

        public const string GenericUnexpectedError = "An unexpected error occurred. Please try again later.";

        // Response Content Type

        public const string ApplicationJsonContentType = "application/json";
    }
}
