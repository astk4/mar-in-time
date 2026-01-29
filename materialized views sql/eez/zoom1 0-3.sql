ALTER SEQUENCE eez_chunk_id_seq RESTART WITH 1;
CREATE MATERIALIZED VIEW eez_v12_zoom_0_3
AS 
	SELECT gid,
		   nextval('eez_chunk_id_seq') AS chunk_id,
		   ST_QuantizeCoordinates(ST_Subdivide(St_MakeValid(ST_Simplify(geom, 0.3)), 4096), 1) AS geom
	FROM eez_v12_copy
WITH DATA;