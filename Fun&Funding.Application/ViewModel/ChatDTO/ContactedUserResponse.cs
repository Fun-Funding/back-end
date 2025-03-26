namespace Fun_Funding.Application.ViewModel.ChatDTO
{
    public class ContactedUserResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string LatestMessage { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
