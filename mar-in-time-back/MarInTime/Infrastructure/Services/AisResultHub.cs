using Microsoft.AspNetCore.SignalR;

namespace MarInTime.Infrastructure.Services
{
    public class AisResultHub : Hub
    {
        public const string receiveShipData = "ReceiveShipData";
    }
}
