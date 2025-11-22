public interface IMessageService
{
    Task SendMessageAsync(string message);
}