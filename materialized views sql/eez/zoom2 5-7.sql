CREATE MATERIALIZED VIEW eez_v12_zoom_5_7
AS 
	SELECT gid,
		   ST_QuantizeCoordinates(ST_Subdivide(ST_SimplifyPreserveTopology(geom, 0.02), 1024), 2) AS geom
	FROM eez_v12_copy
WITH DATA;