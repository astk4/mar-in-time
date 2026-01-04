CREATE MATERIALIZED VIEW eez_v12_zoom_17_18
AS 
	SELECT gid,
		   ST_Subdivide(geom, 64) AS geom
	FROM eez_v12_copy
WITH DATA;