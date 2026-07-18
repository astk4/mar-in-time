-- creates empty matview for all zooms
-- thats temporary untill figured out what to do with geojsons

CREATE MATERIALIZED VIEW IF NOT EXISTS eez_v12_zoom_0_18
AS 
	SELECT gid,
		   0 AS chunk_id,
		   geom
	FROM eez_v12
	WHERE gid < 0
WITH DATA;