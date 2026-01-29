ALTER SEQUENCE eez_chunk_id_seq RESTART WITH 1;
CREATE MATERIALIZED VIEW eez_v12_zoom_4_4
AS 
	SELECT gid,
		   nextval('eez_chunk_id_seq') AS chunk_id,
		   ST_QuantizeCoordinates(ST_Subdivide(ST_MakeValid(ST_Simplify(geom, 0.2)), 2048), 1) AS geom
	FROM eez_v12_copy
WITH DATA;