let shipTypesColors = [
  { r: 0.4, g: 0.8, b: 0.6 }, // wig
  { r: 1, g: 0.6, b: 0 }, // fishing
  { r: 0.118, g: 0.565, b: 1 }, // towing
  { r: 0.498, g: 0.498, b: 0.498 }, // dredging
  { r: 0.31, g: 0.31, b: 0.31 }, // diving

  { r: 0.173, g: 0.322, b: 0.18 }, // military
  { r: 0.878, g: 0.4, b: 1 }, // sailing
  { r: 0.79, g: 0.137, b: 0.878 }, // pleasure craft
  { r: 0.09, g: 0.757, b: 1 }, // high speed craft
  { r: 0.008, g: 0.58, b: 0.769 }, // pilot

  { r: 1, g: 0.271, b: 0 }, // search and rescue
  { r: 0.42, g: 0.753, b: 0.902 }, // tug
  { r: 0.522, g: 0.878, b: 1 }, // port tender
  { r: 0.471, g: 0.02, b: 0.02 }, // anti pollution
  { r: 1, g: 0.843, b: 0 }, // law enforcement

  { r: 0.8, g: 0.8, b: 0.8 }, // local
  { r: 0.929, g: 0.6, b: 0.192 }, // medical
  { r: 1, g: 1, b: 1 }, // resolution no 18
  { r: 0, g: 0.271, b: 0.647 }, // passenger
  { r: 0, g: 0.702, b: 0.31 }, // cargo
  { r: 0.941, g: 0, b: 0 }, // tanker

  defaultColor // other
];

let defaultColor = {r: 0.5, g: 0.5, b: 0.5};

var mapRef;
var markerSize = 6;
var markerOpacity = 0.5;
let pointsCollection = null;

let ww = new Worker(new URL('./web-workers/ship_markers_webworker.js', import.meta.url));


export function initMarkerLayer(map) {
  mapRef = map;

  ww.onmessage = function (e) {
    refillMarkers(e.data);
  }
}

export function onShipPointsArrived(pointsWithTypes) {

  ww.postMessage(pointsWithTypes);
}

export function onZoomForMarkersChanged(zoom) {
  if (zoom < 4) {
    markerSize = 6;
    markerOpacity = 0.5;
    return;
  }
  if (zoom < 6) {
    markerSize = 8;
    markerOpacity = 0.7;
    return;
  }
  
  markerOpacity = (zoom < 11)? 0.9 : 1;

  if (zoom < 11) {
    markerSize = 10;
    return;
  }
  if (zoom < 15) {
    markerSize = 12;
    return;
  }
  if (zoom < 17) {
    markerSize = 15;
    return;
  }
  markerSize = 18;
}

function refillMarkers(geojson) {

  pointsCollection?.remove();
  
  pointsCollection = L.glify.points({
    map: mapRef,
    data: geojson,
    interactive: false,
    size: markerSize,
    color: (index, ship) => {
      var colorObj = shipTypesColors[ship.properties.typeId] || defaultColor;
      colorObj.a = markerOpacity;
      return colorObj;
    }
  });
}