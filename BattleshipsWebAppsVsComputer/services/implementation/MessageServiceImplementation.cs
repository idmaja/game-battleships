using Microsoft.AspNetCore.SignalR;

public class MessageService : IMessageService
{
    private readonly IHubContext<MessageHub> _hubContext;

    public MessageService(IHubContext<MessageHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public virtual async Task SendMessageAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
    }
}