
import { portPopupTemplate } from "./popup_exports.js";
import  * as spatialOptimization from "./spatial_optimization.js";

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
        inertia: false,
        maxBoundsViscosity: 1.0,
        preferCanvas: true
      });
    basicMap.setMinZoom(2);

    eezLayer = L.layerGroup().addTo(basicMap);
    
    const myOnMove = async function() 
    {
      const bbox = boundsToObject(basicMap.getBounds());
      if (!spatialOptimization.validateMove(bbox, basicMap.getCenter())) { return; }
      
      await updateEezLayer(bbox, basicMap.getZoom());
    };

    const myOnZoom = async function() {

      console.log("zoom level:", basicMap.getZoom());
      const bbox = boundsToObject(basicMap.getBounds());
      await updateEezLayer(bbox, basicMap.getZoom());
    };

    basicMap.on('moveend', myOnMove);
    basicMap.on('zoomend', myOnZoom);

    L.tileLayer( 'http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        subdomains: ['a','b','c'],
    }).addTo( basicMap );

    myOnMove();
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

function boundsToObject(bounds) {
    return {
        West: bounds.getWest(),
        South: bounds.getSouth(),
        East: bounds.getEast(), 
        North: bounds.getNorth()
    };
}

function addSingleZone(feature) 
{
  let jsonFeature = JSON.parse(feature);
  try {
    L.geoJSON(jsonFeature).addTo(eezLayer);
    eezLayer.addTo(basicMap);
  }
  catch (err) {
    console.log("Error adding zone:", err);
  }
}

async function updateEezLayer(bbox, zoomLevel) {

    let params = new URLSearchParams(bbox);
    params.append("zoom", zoomLevel);

    let featuresResponse = await spatialOptimization.fetchGetRequestAbortable(`https://localhost:7120/spatial/eez?${params.toString()}`);

    if (!featuresResponse || featuresResponse == null || !featuresResponse.ok) {
      return;
    }

    try {
      eezLayer.remove();  
      eezLayer.clearLayers();
      await spatialOptimization.consumeStreamedResponse(featuresResponse, addSingleZone);
    }
    catch (err) {
      if (err.name === 'AbortError') {
        console.log('EEZ stream closed due to fetch abort');
      }
      else {
        console.error(err);
      }
    }

    featuresResponse = null;
}

window.initMainMap = initMainMap;
window.addMarkers = addMarkers;