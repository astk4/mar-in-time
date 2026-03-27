function makeGeoJsonPoint(individualArray) {
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
        "typeId": individualArray[3],
      }
    };
}

function arrayToGeoJsonFeatureCollection(arrayOfPointArrays) {
 
  let validFeatures = arrayOfPointArrays.reduce((acc, crt) => {
    if (Math.abs(crt[1]) > Number.EPSILON && Math.abs(crt[2]) > Number.EPSILON) {
      acc.push(makeGeoJsonPoint(crt));
    }
    return acc;
  }, []);
  
  return {
    "type": "FeatureCollection",
    "features": validFeatures
  };
}

onmessage = function (e) {
    if (e.data === undefined) {  return; }
    
    postMessage(arrayToGeoJsonFeatureCollection(e.data));
}

