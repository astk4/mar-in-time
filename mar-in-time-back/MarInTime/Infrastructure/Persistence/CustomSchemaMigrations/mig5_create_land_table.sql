CREATE TABLE IF NOT EXISTS public.land_polygons
(
    gid integer PRIMARY KEY,
    x integer,
    y integer,
    geom geometry(MultiPolygon,4326)
);