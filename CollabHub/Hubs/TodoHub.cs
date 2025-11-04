using Microsoft.AspNetCore.SignalR;

namespace CollabHub.Hubs
{
    public class TodoHub : Hub
    {
        // Можна залишити порожнім – нам достатньо Clients.All.SendAsync(...) з контролера
    }
}
