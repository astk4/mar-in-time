pg_restore --no-owner --no-privileges -U some_pg_user -d marintime -v /usr/marintime_geodata.dump
psql -U some_pg_user -d marintime -c "ALTER TABLE eez_v12_copy RENAME TO eez_v12;"