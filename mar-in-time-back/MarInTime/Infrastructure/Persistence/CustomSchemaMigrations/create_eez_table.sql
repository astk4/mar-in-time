CREATE TABLE IF NOT EXISTS public.eez_v12
(
    gid integer PRIMARY KEY,
    mrgid double precision,
    geoname character varying(100) COLLATE pg_catalog."default",
    mrgid_ter1 double precision,
    pol_type character varying(80) COLLATE pg_catalog."default",
    mrgid_sov1 double precision,
    territory1 character varying(80) COLLATE pg_catalog."default",
    iso_ter1 character varying(80) COLLATE pg_catalog."default",
    sovereign1 character varying(80) COLLATE pg_catalog."default",
    mrgid_ter2 double precision,
    mrgid_sov2 double precision,
    territory2 character varying(80) COLLATE pg_catalog."default",
    iso_ter2 character varying(80) COLLATE pg_catalog."default",
    sovereign2 character varying(80) COLLATE pg_catalog."default",
    mrgid_ter3 double precision,
    mrgid_sov3 double precision,
    territory3 character varying(80) COLLATE pg_catalog."default",
    iso_ter3 character varying(80) COLLATE pg_catalog."default",
    sovereign3 character varying(80) COLLATE pg_catalog."default",
    x_1 numeric,
    y_1 numeric,
    mrgid_eez double precision,
    area_km2 double precision,
    iso_sov1 character varying(80) COLLATE pg_catalog."default",
    iso_sov2 character varying(80) COLLATE pg_catalog."default",
    iso_sov3 character varying(80) COLLATE pg_catalog."default",
    un_sov1 double precision,
    un_sov2 double precision,
    un_sov3 double precision,
    un_ter1 double precision,
    un_ter2 double precision,
    un_ter3 double precision,
    geom geometry(MultiPolygon,4326)
);

CREATE INDEX IF NOT EXISTS eez_geom_indx
    ON public.eez_v12 USING gist
    (geom)
    TABLESPACE pg_default;

-- creates empty matview for all zooms
-- thats temporary untill figured out what to do with geojsons

CREATE MATERIALIZED VIEW IF NOT EXISTS eez_v12_zoom_0_18
AS 
	SELECT gid,
		   0 AS chunk_id,
		   geom
	FROM eez_v12
	WHERE gid < 0
WITH DATA;