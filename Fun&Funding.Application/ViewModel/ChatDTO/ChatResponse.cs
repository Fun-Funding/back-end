namespace Fun_Funding.Application.ViewModel.ChatDTO
{
    public class ChatResponse
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
    }
}
