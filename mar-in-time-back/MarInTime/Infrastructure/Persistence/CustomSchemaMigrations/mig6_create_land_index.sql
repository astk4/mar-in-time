CREATE INDEX IF NOT EXISTS land_polygons_geom_idx
    ON public.land_polygons USING gist
    (geom)
    TABLESPACE pg_default;