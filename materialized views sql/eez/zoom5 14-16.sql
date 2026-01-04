CREATE MATERIALIZED VIEW eez_v12_zoom_14_16
AS 
	SELECT gid,
		   ST_Subdivide(ST_SimplifyPreserveTopology(geom, (0.00002)), 128) AS geom
	FROM eez_v12_copy
WITH DATA;