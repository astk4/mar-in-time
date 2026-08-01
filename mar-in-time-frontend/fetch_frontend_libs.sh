#fetch leaflet.js
wget -O leaflet.zip https://github.com/Leaflet/Leaflet/releases/download/v1.9.4/leaflet.zip
mkdir -p ./src/assets/leaflet
unzip leaflet.zip -d ./src/assets/leaflet
rm leaflet.zip

#fetch signalr
mkdir -p ./src/assets/signalr
wget -O ./src/assets/signalr/microsoft-signalr_9.0.6.js https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/9.0.6/signalr.js

#fetch leaflet.glify
mkdir -p ./src/assets/leaflet-glify
wget -O ./src/assets/leaflet-glify/leaflet-glify@3.3.1.min.js https://cdn.jsdelivr.net/npm/leaflet.glify@3.3.1/dist/glify-browser.min.js