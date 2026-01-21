CREATE FUNCTION public.distance_to_viewport_center(IN center_x double precision, IN center_y double precision, IN viewport_center_x double precision, IN viewport_center_y double precision)
    RETURNS double precision
    LANGUAGE 'plpgsql'
AS
$$
DECLARE
   distance float8;
BEGIN
   SELECT ST_Distance(ST_Point(center_x, center_y, 4326)::geometry, 
		      ST_Point(viewport_center_x, viewport_center_y, 4326)::geometry)
   INTO distance;
   RETURN distance;
END
$$;

ALTER FUNCTION public.distance_to_viewport_center(double precision, double precision, double precision, double precision)
    OWNER TO postgres;