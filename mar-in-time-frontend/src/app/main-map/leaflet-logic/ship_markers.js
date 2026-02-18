var mapRef;
let pointsCollection = null;

export function initMarkerLayer(map) {
  mapRef = map;
}

function arrayToGeoJsonPoint(individualArray) {
  return {
      "type": "Feature",
      "geometry": {
        "type": "Point",
        "coordinates": [
          individualArray[1],
          individualArray[2]
        ]
      },
      "properties": {
        "mmsi": individualArray[0],
      }
    };
}

export function onCoordsArrived(arrayOfPointArrays) {

  //0 - MMSI, 1 - lng, 2 - lat

  let geoJsonCollection = {
    "type": "FeatureCollection",
    "features": arrayOfPointArrays.map(arrayToGeoJsonPoint)
  };

  pointsCollection?.remove();

  pointsCollection = L.glify.points({
    map: mapRef,
    data: geoJsonCollection,
    interactive: false,
    size: 10,
    color: {r: 0.5, g: 0.5, b: 0.5 }
  });
}