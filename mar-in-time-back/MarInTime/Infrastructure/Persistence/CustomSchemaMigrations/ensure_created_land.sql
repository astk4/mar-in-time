CREATE TABLE IF NOT EXISTS public.land_polygons
(
    gid integer PRIMARY KEY,
    x integer,
    y integer,
    geom geometry(MultiPolygon,4326)
);

CREATE INDEX IF NOT EXISTS land_polygons_geom_idx
    ON public.land_polygons USING gist
    (geom)
    TABLESPACE pg_default;