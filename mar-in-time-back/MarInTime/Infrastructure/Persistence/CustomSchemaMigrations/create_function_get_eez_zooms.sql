CREATE OR REPLACE FUNCTION get_eez_zoom_names()
RETURNS TABLE(relation_name varchar) 
LANGUAGE plpgsql AS 
$$
BEGIN
    RETURN QUERY 
		SELECT (tablename)::varchar 
		FROM pg_tables
		WHERE tablename like 'eez_v12_zoom_%';
END;
$$;