CREATE INDEX IF NOT EXISTS eez_geom_indx
    ON public.eez_v12 USING gist
    (geom)
    TABLESPACE pg_default;