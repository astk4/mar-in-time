/*gids of zones that need extra simplification:
4 - Falkland Islands
11 - Chile
22 - Argentina
187 - Bangladesh
197 - Philippines
210 - Finland
230 - Myanmar
269, 270, 272 - Norway
*/
ALTER SEQUENCE eez_chunk_id_seq RESTART WITH 1;
CREATE MATERIALIZED VIEW eez_v12_zoom_8_10
AS 
	SELECT gid,
		   nextval('eez_chunk_id_seq') AS chunk_id,
		   ST_QuantizeCoordinates(
			   ST_Subdivide(
			    CASE 
        			WHEN gid IN (4, 11, 22, 187, 197, 210, 230, 269, 270, 272) 
						THEN ST_MakeValid(ST_Simplify(geom, 0.002))
        			ELSE ST_SimplifyPreserveTopology(geom, 0.002)
    			END, 
				512),
			3) AS geom
	FROM eez_v12_copy
WITH DATA;