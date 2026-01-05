
import { portPopupTemplate } from "./popup_exports.js";

var basicMap;
var eezLayer;
let minLat = -85.05112878, maxLat = 85.05112878,
    minLong = -180, maxLong = 180;

function initMainMap() {

    let myMaxBounds = [
      [maxLat, maxLong], //south west
      [minLat, minLong] //north east
    ];
    basicMap = L.map('mapid', 
      { 
        center: [20.0, 5.0], 
        zoom: 4,
        maxBounds: myMaxBounds, 
        maxBoundsViscosity: 1.0
      });
    basicMap.setMinZoom(2);

    eezLayer = L.layerGroup().addTo(basicMap);
    
    basicMap.on('drag', function() {
      basicMap.panInsideBounds(myMaxBounds, { animate: false });
    });
    basicMap.on('zoomend', async function() { 
      await updateEezLayer(basicMap.getBounds(), basicMap.getZoom());
    });
    basicMap.on('move', async function() { 
      await updateEezLayer(basicMap.getBounds(), basicMap.getZoom());
    });
    basicMap.on('resize', async function() { 
      await updateEezLayer(basicMap.getBounds(), basicMap.getZoom());
    });

    L.tileLayer( 'http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        subdomains: ['a','b','c'],
    }).addTo( basicMap );
}

async function addMarkers()
{
  let markers = await fetchGet("https://localhost:7120/locations/ports");

  for ( var i=0; i < markers.length; ++i ) 
  {
    L.marker( [markers[i].latitude, markers[i].longitude] )
        .bindPopup(portPopupTemplate(markers[i]))
        .addTo( basicMap );
  }
}

async function updateEezLayer(bounds, zoomLevel) {
    const bbox = {
        West: bounds.getWest(),
        South: bounds.getSouth(),
        East: bounds.getEast(), 
        North: bounds.getNorth()
    };

    let params = new URLSearchParams(bbox);
    params.append("zoom", zoomLevel);

    let features = await fetchGet(`https://localhost:7120/spatial/eez?${params.toString()}`);

    if (!features) {
      return;
    }
    
    eezLayer.clearLayers();
    L.geoJSON(features).addTo(eezLayer);
}

window.initMainMap = initMainMap;
window.addMarkers = addMarkers;