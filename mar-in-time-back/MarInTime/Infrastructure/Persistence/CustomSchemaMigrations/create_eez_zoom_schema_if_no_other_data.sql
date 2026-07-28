DO $$

BEGIN
	
IF (SELECT EXISTS (
    SELECT 1
    FROM information_schema.tables
    WHERE table_name LIKE 'eez_v12_zoom_%')
)

	THEN
		RAISE NOTICE 'Zoom tier tables already exist';
		RETURN;
	END IF;

-- creates empty table for all zooms

CREATE TABLE IF NOT EXISTS eez_v12_zoom_0_18
AS 
	SELECT gid,
		   0 AS chunk_id,
		   geom
	FROM eez_v12
	WHERE gid < 0
WITH DATA;

END $$;