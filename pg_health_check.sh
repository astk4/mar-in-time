#change username also here if you changed POSTGRES_USER value

pg_isready -U some_pg_user -d marintime || exit 1

marker_table=$(psql -U some_pg_user -d marintime -tAc "select count(tablename) from pg_tables where tablename='docker_import_marker';")
if [[ $marker_table < 1 ]]
then
    echo "Final marker table not created yet"
    exit 1
else
    exit 0
fi

