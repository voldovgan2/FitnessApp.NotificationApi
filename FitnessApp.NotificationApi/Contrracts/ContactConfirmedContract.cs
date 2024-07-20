namespace FitnessApp.NotificationApi.Contracts;

public class ContactConfirmedContract
{
    public const string CONTRACT_TYPE = "ContactConfirmation";
    public string UserId { get; set; }
    public string FollowerUserId { get; set; }
}
