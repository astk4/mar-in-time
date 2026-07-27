#!/bin/bash
if [[ -e /usr/data_to_import/marintime_geodata.dump ]]
then
    echo "geojson data dump file already exists"
else
    wget -O /usr/data_to_import/marintime_geodata.dump "https://github.com/astk4/mar-in-time/releases/download/assets/marintime_geodata.dump"
fi

pg_restore --no-owner --no-privileges -U some_pg_user -d marintime -v /usr/data_to_import/marintime_geodata.dump
psql -U some_pg_user -d marintime -c "ALTER TABLE eez_v12_copy RENAME TO eez_v12;"
psql -U some_pg_user -d marintime -c "CREATE TABLE IF NOT EXISTS docker_import_marker (completed_at TIMESTAMP DEFAULT NOW());"