using AisCommunication.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace MarInTime.Infrastructure.Services
{
    public class AisReceiverService(IHubContext<AisResultHub> hubContext) : AisSender.AisSenderBase
    {
        public override async Task<Empty> SendMessages(IAsyncStreamReader<AISResult> requestStream, ServerCallContext context)
        {
            try
            {
                await foreach (AISResult message in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    Console.Write("GRPC received ");
                    if (message.Position != null)
                    {
                        Console.WriteLine($"position {message.Position.Latitude}, {message.Position.Longitude}");
                    }
                    else if (message.ShipData != null)
                    {
                        Console.WriteLine($"some data for ship IMO{message.ShipData.IMONumber} {message.ShipData.Name}");

                        await hubContext.Clients.All.SendAsync(AisResultHub.receiveShipData,
                                                                 message.ShipData.IMONumber,
                                                                 message.ShipData.Name);
                    }
                    else
                    {
                        Console.WriteLine($"safety message: {message.Safety.Text}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("grpc server turned off");
            }
            catch (IOException io)
            {
                if (io.InnerException is not ConnectionAbortedException)
                {
                    throw;
                }
                Debug.WriteLine("grpc communication stream aborted");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("grpc server error " + ex.Message);
            }
            
            return new Empty();
        }
    }
}
