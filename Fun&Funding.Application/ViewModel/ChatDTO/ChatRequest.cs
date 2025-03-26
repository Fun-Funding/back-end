namespace Fun_Funding.Application.ViewModel.ChatDTO
{
    public class ChatRequest
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
