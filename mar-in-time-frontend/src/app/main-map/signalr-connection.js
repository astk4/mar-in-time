import { onCoordsArrived } from "./leaflet-logic/ship_markers";
import { MessagePackHubProtocol } from "@microsoft/signalr-protocol-msgpack";

var hubConnection;

export function initSignalR() {
    hubConnection = new signalR.HubConnectionBuilder()
                               .withUrl("/hubs/ais")
                               .withHubProtocol(new MessagePackHubProtocol())
                               .build(); 

    hubConnection.on("ShipPositions", function (args) {
        console.log("Received ShipPositions args:", args.length);
        onCoordsArrived(args);
    });

    hubConnection.start();
    console.log("SignalR connection started");
}
