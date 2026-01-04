CREATE MATERIALIZED VIEW eez_v12_zoom_11_13
AS 
	SELECT gid,
		   ST_Subdivide(ST_SimplifyPreserveTopology(geom, (
		   		CASE 
            		WHEN gid = 254 THEN 0.0005	-- micronesia problem
            		ELSE 0.0002	-- other countries
        		END
			)), 256) AS geom
	FROM eez_v12_copy
WITH DATA;