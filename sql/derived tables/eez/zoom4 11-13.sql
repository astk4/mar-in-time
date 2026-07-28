ALTER SEQUENCE eez_chunk_id_seq RESTART WITH 1;
CREATE TABLE IF NOT EXISTS eez_v12_zoom_11_13
AS 
	SELECT gid,
		   nextval('eez_chunk_id_seq') AS chunk_id,
		   ST_QuantizeCoordinates(
			   ST_Subdivide(ST_SimplifyPreserveTopology(geom, 0.0002), 
				256),
			10) AS geom
	FROM eez_v12_copy
WITH DATA;