ALTER SEQUENCE eez_chunk_id_seq RESTART WITH 1;
CREATE TABLE IF NOT EXISTS eez_v12_zoom_14_16
AS 
	SELECT gid,
		   nextval('eez_chunk_id_seq') AS chunk_id,
		   ST_Subdivide(ST_SimplifyPreserveTopology(geom, (0.00001)), 128) AS geom
	FROM eez_v12_copy
WITH DATA;