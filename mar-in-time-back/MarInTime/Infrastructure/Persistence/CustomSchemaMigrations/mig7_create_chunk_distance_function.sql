DROP FUNCTION IF EXISTS distance_from_chunk_to_point;

CREATE OR REPLACE FUNCTION distance_from_chunk_to_point(IN chunk geometry, IN point_x double precision, IN point_y double precision)
    RETURNS double precision
    LANGUAGE 'plpgsql'
AS
$$
DECLARE
   distance float8;
BEGIN
   SELECT ST_Distance(ST_Centroid(chunk)::geometry, 
		      		  ST_Point(point_x, point_y, 4326)::geometry)
   INTO distance;
   RETURN distance;
END
$$;