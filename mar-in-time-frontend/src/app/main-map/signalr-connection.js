import { onCoordsArrived, onTypesArrived } from "./leaflet-logic/ship_markers";
import { MessagePackHubProtocol } from "@microsoft/signalr-protocol-msgpack";

var hubConnection;

export function initSignalR() {
    hubConnection = new signalR.HubConnectionBuilder()
                               .withUrl("/hubs/ais")
                               .withHubProtocol(new MessagePackHubProtocol())
                               .build(); 

    hubConnection.on("ShipPositions", function (args) {
        onCoordsArrived(args);
    });

    hubConnection.on("ShipData", function (args) {
        onTypesArrived(args);
    });

    hubConnection.start();
    console.log("SignalR connection started");
}
