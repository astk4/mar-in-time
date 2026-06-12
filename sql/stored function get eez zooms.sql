drop function get_eez_zoom_names;
CREATE OR REPLACE FUNCTION get_eez_zoom_names()
RETURNS TABLE(view_name varchar) 
LANGUAGE plpgsql AS 
$$
BEGIN
    RETURN QUERY 
		SELECT (matviewname)::varchar 
		FROM pg_matviews
		WHERE matviewname like 'eez_v12_zoom_%';
END;
$$;