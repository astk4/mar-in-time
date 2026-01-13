CREATE MATERIALIZED VIEW eez_v12_zoom_0_4
AS 
	SELECT gid,
		   ST_QuantizeCoordinates(ST_Subdivide(ST_SimplifyPreserveTopology(geom, 0.2), 2048), 1) AS geom
	FROM eez_v12_copy
WITH DATA;