import { portPopupTemplate } from "./popup_exports.js";
import { fetchGet } from "./vanilla-api-service.js";
import  * as spatialOptimization from "./spatial_optimization.js";

var basicMap;
var eezLayer;
let minLat = -85.05112878, maxLat = 85.05112878,
    minLong = -180, maxLong = 180;
var prevZoomLevel = 4;
var layersToRemove = [];

async function myOnMove() 
{
  const bbox = boundsToObject(basicMap.getBounds());
  if (!spatialOptimization.validateMove(bbox, basicMap.getCenter())) { return; }
  
  await updateEezLayer(bbox, basicMap.getZoom());
};

async function myOnZoom() 
{
  let zoomNow = basicMap.getZoom();

  console.log("zoom level:", zoomNow);
  const bbox = boundsToObject(basicMap.getBounds());
  await updateEezLayer(bbox, zoomNow, prevZoomLevel);

  prevZoomLevel = zoomNow;
};

export function patchIconPaths() 
{
  delete L.Icon.Default.prototype._getIconUrl;

  L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'assets/leaflet/dist/images/marker-icon-2x.png',
    iconUrl: 'assets/leaflet/dist/images/marker-icon.png',
    shadowUrl: 'assets/leaflet/dist/images/marker-shadow.png',
  });
}

export function initMainMap() {

    let myMaxBounds = [
      [maxLat, maxLong], //south west
      [minLat, minLong] //north east
    ];
    basicMap = L.map('mapid', 
      { 
        center: [20.0, 5.0], 
        zoom: prevZoomLevel, // set to 4
        maxBounds: myMaxBounds, 
        inertia: false,
        maxBoundsViscosity: 1.0,
        preferCanvas: true
      });
    basicMap.setMinZoom(2);

    eezLayer = L.layerGroup().addTo(basicMap);
    
    basicMap.on('moveend', myOnMove);
    basicMap.on('zoomend', myOnZoom);

    L.tileLayer( 'http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        subdomains: ['a','b','c'],
    }).addTo( basicMap );
    
    eezLayer = L.geoJSON(null, {
        style: {
            weight: spatialOptimization.selectOutlineThickness(prevZoomLevel)
        }
    }).addTo(basicMap);

    myOnMove();
}

export async function addMarkers()
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

function handleZoneIncrement(feature)
{
  let jsonFeature;
  try {
    jsonFeature = JSON.parse(feature);
  }
  catch (err) {
   console.log(feature);
   console.error(err);
   return;
  }

  if (!Object.hasOwn(jsonFeature, "delete"))
  {
    try {
      eezLayer.addData(jsonFeature);
    }
    catch (err) {
      console.log("Error adding zone:", err);
    }
  }
  else if (jsonFeature.delete) {
    eezLayer.eachLayer(function (layer) {
        if (layer.feature && layer.feature.properties && layer.feature.properties.ChunkId == jsonFeature.chunkId) {
            layersToRemove.push(layer);
        }
    });
  }
}

async function updateEezLayer(bbox, zoomLevel, prevZoomLevel=undefined) {

    let params = new URLSearchParams(bbox);
    params.append("zoom", zoomLevel);
    if (prevZoomLevel) {
      params.append("prevZoom", prevZoomLevel);
    }

    let featuresResponse = await spatialOptimization.fetchGetRequestAbortable(`https://localhost:7120/spatial/eez?${params.toString()}`);
    eezLayer.setStyle({
      weight: spatialOptimization.selectOutlineThickness(zoomLevel)
    });

    if (!featuresResponse || featuresResponse == null || !featuresResponse.ok) {
      return;
    }

    try {
      if (featuresResponse.headers.has("Marintime-Zoom-Tier-Change")) {
        eezLayer.clearLayers();
      }
      await spatialOptimization.consumeStreamedResponse(featuresResponse, handleZoneIncrement);
    }
    catch (err) {
      if (err.name === 'AbortError') {
        console.log('EEZ stream closed due to fetch abort');
      }
      else {
        console.error(err);
      }
    }
    finally {
      featuresResponse = null;
      layersToRemove.forEach(layer => {
        eezLayer.removeLayer(layer);
      });
      layersToRemove = [];
    }
}

window.patchIconPaths = patchIconPaths;
window.initMainMap = initMainMap;
window.addMarkers = addMarkers;