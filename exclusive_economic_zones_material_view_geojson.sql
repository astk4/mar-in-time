CREATE MATERIALIZED VIEW eez_v12_geojson 
AS 
	SELECT gid,  geoname, x_1, y_1, ST_AsGeoJSON(geom) AS geom_geojson
	FROM public.eez_v12
WITH DATA;
