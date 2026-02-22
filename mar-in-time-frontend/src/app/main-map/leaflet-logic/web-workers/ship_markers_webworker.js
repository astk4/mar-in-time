let mmsiShipTypeMap = new Map();

function findColor(mmsi) {
  var shipType = mmsiShipTypeMap.get(mmsi);
  if (shipType === undefined) {
    return defaultColor;
  }
  return shipTypesColors[shipType] || defaultColor;
}

function makeGeoJsonPoint(individualArray, typeId) {
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
        "typeId": typeId,
      }
    };
}

function arrayToGeoJsonFeatureCollection(arrayOfPointArrays) {
  return {
    "type": "FeatureCollection",
    "features": arrayOfPointArrays.map(arr => makeGeoJsonPoint(arr, mmsiShipTypeMap.get(arr[0]) || 21))
  };
}

onmessage = function (e) {
    if (e.data.hasCoords) {
      let messageObj = {
          result: arrayToGeoJsonFeatureCollection(e.data.arrivedCoords)
      }
      postMessage(messageObj);
      return;
    }

    for (let arr of e.data.arrivedTypes) {
        mmsiShipTypeMap.set(arr[0], arr[1]);
    }
}

