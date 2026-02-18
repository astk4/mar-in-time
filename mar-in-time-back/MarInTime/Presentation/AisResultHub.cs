using MarInTime.Application.Services;
using MarInTime.Presentation.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace MarInTime.Presentation
{
    public class AisResultHub : Hub
    {
        public const string ReceiveShipPosKey = "ShipPositions";
    }
}
