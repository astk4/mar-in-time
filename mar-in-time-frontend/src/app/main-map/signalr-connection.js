var hubConnection;

export function initSignalR() {
    hubConnection = new signalR.HubConnectionBuilder()
                               .withUrl("/hubs/ais")
                               .build(); 

    hubConnection.on("ReceiveShipData", function (arg1, arg2) {
        console.log("Received ship data:", arg1, arg2);
    });

    hubConnection.start();
    console.log("SignalR connection started");
}