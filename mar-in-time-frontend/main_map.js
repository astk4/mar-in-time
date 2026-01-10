
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
      
      //console.log("eez update here");
      await updateEezLayer(bbox, basicMap.getZoom());
    };

    const myOnZoom = async function() {

      const bbox = boundsToObject(basicMap.getBounds());
      //console.log("eez update here");
      await updateEezLayer(bbox, basicMap.getZoom());
    };

    basicMap.on('load', myOnMove);

    basicMap.on('moveend', myOnMove);
    basicMap.on('zoomend', myOnZoom);

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

function boundsToObject(bounds) {
    return {
        West: bounds.getWest(),
        South: bounds.getSouth(),
        East: bounds.getEast(), 
        North: bounds.getNorth()
    };
}

async function updateEezLayer(bbox, zoomLevel) {

    let params = new URLSearchParams(bbox);
    params.append("zoom", zoomLevel);

    let features = await spatialOptimization.fetchGetAbortable(`https://localhost:7120/spatial/eez?${params.toString()}`);

    if (!features || features == null) {
      return;
    }

    eezLayer.remove();  
    eezLayer.clearLayers();
    L.geoJSON(features).addTo(eezLayer);
    eezLayer.addTo(basicMap);

    features = null;
}

window.initMainMap = initMainMap;
window.addMarkers = addMarkers;