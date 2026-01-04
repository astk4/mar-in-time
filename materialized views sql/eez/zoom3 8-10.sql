CREATE MATERIALIZED VIEW eez_v12_zoom_8_10
AS 
	SELECT gid,
		   ST_Subdivide(ST_SimplifyPreserveTopology(geom, (
		   		CASE 
            		WHEN gid = 254 THEN 0.005	-- micronesia problem
            		ELSE 0.002	-- other countries
        		END
			)), 512) AS geom
	FROM eez_v12_copy
WITH DATA;