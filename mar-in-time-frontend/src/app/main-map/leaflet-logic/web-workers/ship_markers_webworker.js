var headingMarkerR = 0.15;

function makeGeoJsonPointGeometry(individualArray) {
  return {
        "type": "Point",
        "coordinates": [
          individualArray[1],
          individualArray[2]
        ]
      };
}

function makeGeoJsonMarkerGeometry(individualArray) {
  
  var myMarkerSize = Math.cos(toRadians(individualArray[1])) * headingMarkerR;

  return {
        "type": "Polygon",
        "coordinates": [getMarkerShapeCoordinates(individualArray[1], individualArray[2], 
                                                  individualArray[4], myMarkerSize, 30)]
  };
}

function arrayToGeoJsonFeatures(arrayOfPointArrays) {
 
  let validFeatures = arrayOfPointArrays.reduce((acc, crt) => {
    if (Math.abs(crt[1]) > Number.EPSILON && Math.abs(crt[2]) > Number.EPSILON) {
      
      let geometryObj = crt[4] >= 360 ? makeGeoJsonPointGeometry(crt) : makeGeoJsonMarkerGeometry(crt);
      var feature = {
        "type": "Feature",
        "geometry": geometryObj,
        "properties": {
          "mmsi": crt[0],
          "typeId": crt[3],
        }
      };
      if (crt[4] >= 360) {
        acc.points.push(feature);
      }
      else {
        acc.shapes.push(feature);
      }
    }
    return acc;
  },  {
        points: [],
        shapes: []
      }
  );
  
  return validFeatures;
}

function formatFeatureCollection(features) {
  return {
    "type": "FeatureCollection",
    "features": features
  };
}

onmessage = function (e) {
    if (e.data === undefined) {  return; }
    
    if (!Array.isArray(e.data)) 
    {
      headingMarkerR = e.data;
      return;
    }

    let featObj = arrayToGeoJsonFeatures(e.data);

    featObj.points = formatFeatureCollection(featObj.points);
    featObj.shapes = formatFeatureCollection(featObj.shapes);

    postMessage(featObj);
}

function getMarkerShapeCoordinates(centerX, centerY, courseDeg, radius, markerWidthAngleDeg) {
  let trigRad = toRadians(courseDeg);

  let mainPointX = Math.cos(trigRad) * radius + centerX;
  let mainPointY = Math.sin(trigRad) * radius + centerY;

  let basePoint1AngleDeg = courseDeg - (180 - markerWidthAngleDeg);
  let basePoint1Rad = toRadians(basePoint1AngleDeg);

  let basePoint1X = Math.cos(basePoint1Rad) * radius + centerX;
  let basePoint1Y = Math.sin(basePoint1Rad) * radius + centerY;

  let basePoint2AngleDeg = courseDeg + (180 - markerWidthAngleDeg);
  let basePoint2Rad = toRadians(basePoint2AngleDeg);

  let basePoint2X = Math.cos(basePoint2Rad) * radius + centerX;
  let basePoint2Y = Math.sin(basePoint2Rad) * radius + centerY;

  return [
    [mainPointY, mainPointX%360],
    [basePoint1Y, basePoint1X%360],
    [basePoint2Y, basePoint2X%360],
    [mainPointY, mainPointX%360]
  ];
}

function toRadians(angle) {
  return angle * (Math.PI / 180);
}