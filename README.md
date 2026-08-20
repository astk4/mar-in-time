# MarInTime

Geoinformation system for monitoring and visualization of global vessel traffic in real time from a stream of [AIS](https://en.wikipedia.org/wiki/Automatic_identification_system) data (maritime navigation protocol). ~18-25 thousand vessels, at the same time. Not more not because it can't withstand more load, but because I haven't found more free AIS data :) Heavily inspired by Flightradar24. Developed most actively in Q1-Q2 2026 and is very much usable now, but far from closed and more planned features will be added.

[![.NET Build (Backend)](https://github.com/astk4/mar-in-time/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/astk4/mar-in-time/actions/workflows/dotnet-build.yml) 
[![Angular Build (Frontend)](https://github.com/astk4/mar-in-time/actions/workflows/angular-build.yml/badge.svg)](https://github.com/astk4/mar-in-time/actions/workflows/angular-build.yml) 
![Status](https://img.shields.io/badge/status-stable-green)

<details> 
    <summary>Table of contents</summary>
    <ol>
        <li>
            <a href="#dependencies">Dependencies</a>
        </li>
        <li>
            <a href="#getting-started">Getting started</a>
        </li>
        <li>
            <a href="#features">Features</a>
        </li>
        <li>
            <a href="#tech-stack">Tech stack</a>
        </li>
        <li>
            <a href="#folders-structure">Folders structure</a>
        </li>
        <li>
            <a href="#future-plans">Future plans</a>
        </li>
        <li>
            <a href="#static-data-sources">Static data sources</a>
        </li>
    </ol>
</details> 

## Dependencies
*These version numbers are not strictly required, you can try other versions, but with these ones the project was made, so they are the most reliable.*
- **<u>AIS data source</u>:** [aisstream.io](https://aisstream.io) *(public API, not a library)*
- <u>Backend</u>: .NET 8.0 SDK
- <u>Data persistence</u>:
    - PostgreSQL 18
    - PostGIS 3.6
    - Redis 8.4
    - ClickHouse 26.3
- <u>Frontend application:</u>
    - Node.js 20.x
    - Angular CLI 20.3
- <u>Frontend libraries **(located in project as assets)**:</u>
    - SignalR 9.0.6 [cdn](https://cdnjs.com/libraries/microsoft-signalr/9.0.6)
    - Leaflet.js 1.9.4 [official website](https://leafletjs.com/download.html)
    - leaflet.glify 3.3.1min [cdn](https://www.jsdelivr.com/package/npm/leaflet.glify)
- *The rest of frontend libraries - MessagePack and TailwindCSS are specified in package.json and therefore can be replenished with npm install*

## Getting started

**Prerequisites:**
- Git
- GitHub account
- Docker

1. Clone the repository
```
git clone https://github.com/astk4/mar-in-time.git
```
2. Obtain an API key from [aisstream.io](https://aisstream.io), the data source:
    - 2.1. Go to "Get started"
    - 2.2. Sign up with Github
    - 2.3. Go to "API Keys"
    - 2.4. "Create API Key" and copy it

3. Paste the API key in stream microservice config: 
    - 3.1. In the repository, go to `mar-in-time-back/StreamAIS/secrets.template.json`
    - 3.2. Paste the key into empty slot "AIS" -> "ApiKey"
    - 3.3. Rename the file to `secrets.json`

4. Download .dump file with data for database from the latest [release](https://github.com/astk4/mar-in-time/releases/tag/assets) tagged ***assets***
5. Put the .dump file into `data_to_import` folder. <details><summary>spoiler regarding last 2 steps</summary>
If you forget steps 4-5, the .dump file will be fetched during docker-compose deployment, but it will take an obscenely long time. So please ensure that the data file is present in repo folder before moving to Docker.
</details>

6. Ensure Docker is running by now
7. Launch the project (~3 minutes)
```
docker compose up
```
- If launching the project in Docker for the first time, wait also for data import to complete (~3-4 minutes)
8. Go to project frontend at http://localhost:4300

---
<details>
    <summary>If you happen to have ALL <a href="#dependencies">dependencies</a> locally and for some reason don't want to use Docker Compose in principle... </summary>

⚠️ Warning - in case of launching this alternative way the solution may work unstably, especially gRPC stability is not guaranteed

**1-5 steps - same as above** 

6. Setup PostgreSQL database with geodata, <u>if the DB and/or data in it doesn't exist</u>:
    - 6.1 Create database `marintime` in PostgreSQL with your preferred tools, if it doesn't exist yet. For example, CLI command is:
    ```
    psql -U <your_username> -c "CREATE DATABASE marintime"
    ```
    - 6.2. Import geodata from .dump file into database (run this in repository root):
    ```
    pg_restore --no-owner --no-privileges -U <your_username> -d marintime -v data_to_import/marintime_geodata.dump
    ```

7. Configure settings for main backend:
    - 7.1. Go to file `mar-in-time-back/MarInTime/secrets.template.json`
    - 7.2. Insert your connection string for PostgreSQL into slot "Data" -> "Main". Usually it looks like this (port 5432 is default for postgresql):
    ```
    User ID=<your_username>;Password=<your_pwd>;Host=localhost;Port=5432;Database=marintime
    ```
    - 7.3. Insert your CORS settings into slots of "CORS_Settings" -> "Main". Minimal CORS for this project are:
    ```json
    "AllowedHosts": [ "localhost", "127.0.0.1" ],
    "AllowedMethods": [ "GET" ],
    "AllowedPorts": [ 5254, 7120, 4300, 5000 ]
    ```
    - explanation why such values:
        - AllowedHosts - for local launch
        - AllowedMethods - only GET works just fine because so far only data retrieval operations are in API
        - AllowedPorts - backend ports from main backend launch settings + frontend port + sometimes .net release run defaults to listening on port 5000
    - 7.4. Insert your Redis <u>hostname</u> into empty slot "Redis" -> "Host" -> "Main". Usually it is ```localhost``` (even if Redis is running in a Docker container <u>without compose</u>)
    - 7.5. Insert your Redis <u>port number</u> into empty slot "Redis" -> "Port" -> "Main". Default port for it is ```6379```.
    - 7.6. Rename the file to just `secrets.json`

8. Configure data access setting for stream microservice:
    - 8.1. Go to file `mar-in-time-back/StreamAIS/secrets.json` (as it was already renamed in previous steps)
    - 8.2. Insert your connection string for ClickHouse into slot "Clickhouse" -> "ConnectionString" -> "Main". Usually it looks like this (port 8123 is one of the default ones for clickhouse):
    ```
    Host=localhost;Port=8123;Username=<your_username>;Password=<your_pwd>
    ```
9. Run main backend:
    - 9.1. Go to `mar-in-time-back/MarInTime/`
    - 9.2. Run it: 
    ``` 
    dotnet run 
    ```
    - 9.3. <u>If running the solution this way for the first time and gRPC connection wasn't attempted yet</u>: look in the end of console output/logs for section(s) **info: Now listening on: (some url)** and copy the URL if there's only one. If there's 2 of them, copy one beginning with *https*, though if it won't work, you can try the *http* one too.

10. Configure gRPC setting for stream microservice, **if this setting is empty:**
    - 10.1. Go to file `mar-in-time-back/StreamAIS/secrets.json`
    - 10.2. Paste the URL copied in previous step into slot "Grpc" -> "TargetUrl" -> "Main"

11. Run AIS stream microservice:
    - 11.1. Ensure you are in `mar-in-time-back/StreamAIS/`
    - 11.2. Run it: 
    ``` 
    dotnet run 
    ```
12. Run frontend:
    - 12.1. Go to `mar-in-time-frontend/`
    - 12.1. Run it:
    ```
    ng serve --port 4300
    ```
13. Go to project frontend at http://localhost:4300

</details>

## Features
- Near real time streaming of vessel positions from AIS message stream to map
    - Markers are color coded by [vessel types](https://api.vtexplorer.com/docs/ref-aistypes.html) according to best practices in maritime visualization [(approximate example)](https://datadocked.com/vessel-types)
    - For moving vessels, marker rotation repeats heading angle

- Storing all obtained AIS data into historical database, organized into usage sessions and time-series structure

- GeoJSON map layer of [exclusive economic zones (EEZ)](https://en.wikipedia.org/wiki/Exclusive_economic_zone):
    - 7 tiers of simplification for different zoom levels
    - Loaded in small chunks for effectiveness

- Collapsible side view with some information about a clicked point:
    - Coordinates in decimal and NMEA 1308 formats
    - Whether a point is in someone's EEZ/neutral waters/on land
    - If a point is within a EEZ - EEZ kind (200 miles/overlapping claim/joint regime) and country(-ies)

- Markers of important ports

## Tech stack
- Backend [![.NET 8 | ASP.NET Core](https://img.shields.io/badge/.NET%208.0-ASP.NET%20Core-blueviolet?logo=dotnet)](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)

- Data persistence [![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?logo=postgresql&logoColor=white)](https://www.postgresql.org/) [![PostGIS](https://img.shields.io/badge/PostGIS-5b7b9f)](https://postgis.net/) [![EF Core](https://img.shields.io/badge/EF%20Core-5C2D91?logo=.net&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/) [![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?&logo=redis&logoColor=white)](https://redis.io/solutions/caching/) [![ClickHouse](https://img.shields.io/badge/ClickHouse-FFCC01?&logo=clickhouse&logoColor=white)](https://clickhouse.com/)

- Data transmission [![gRPC](https://img.shields.io/badge/gRPC-2da6b0)](https://grpc.io/) [![WebSocket](https://img.shields.io/badge/WebSocket-010101?logo=socketdotio&logoColor=white)](https://websockets.spec.whatwg.org/) [![SignalR](https://img.shields.io/badge/SignalR-blue)](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-8.0) [![MessagePack](https://img.shields.io/badge/MessagePack-gray)](https://msgpack.org/)

- Frontend [![Angular](https://img.shields.io/badge/Angular-%23DD0031.svg?logo=angular&logoColor=white)](https://angular.dev/overview) [![TailwindCSS](https://img.shields.io/badge/TailwindCSS-%2338B2AC.svg?logo=tailwind-css&logoColor=white)](https://tailwindcss.com/) [![LeafletJS](https://img.shields.io/badge/LeafletJS-199900)](https://leafletjs.com/) [![OpenStreetMap](https://img.shields.io/badge/OpenStreetMap-7EBC6F?logo=OpenStreetMap&logoColor=white)](https://www.openstreetmap.org/)

- Graphics optimization [![WebGL](https://img.shields.io/badge/WebGL-990000?logo=webgl&logoColor=white)](https://www.khronos.org/webgl/)
[![Leaflet.glify](https://img.shields.io/badge/Leaflet.glify-F06896)](https://github.com/robertleeplummerjr/Leaflet.glify)


- CI/CD [![Docker Compose](https://img.shields.io/badge/Docker-Compose-061D2F?logo=docker&logoColor=white&labelColor=230db7)](https://docs.docker.com/compose/)
[![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-333333?style=flat&logo=github-actions)](https://github.com/astk4/mar-in-time/actions)

## Folders structure
<details>
    <summary>expand for folders structure</summary>

```bash
mar-in-time
├───.github
│   └───workflows # github actions pipelines that build backend and frontend
├───data_to_import # .dump file with geojsons to import into db should be put here
├───mar-in-time-back # Backend projects
│   ├───AisCommunication # Project of shared library for backend parts communication
│   │   └───Protos # gRPC contracts for AIS information of types relevant for the solution
│   ├───MarInTime # Project of main backend API, organized in Clean Architecture
│   │   ├───Application # business rules
│   │   │   ├───Mappers # extension methods to convert DbContext level entities into view models used in web requests/responses, and vice versa
│   │   │   ├───Repositories # data retrieval interfaces
│   │   │   └───Services # gis logic interface
│   │   ├───Domain # business logic types
│   │   │   ├───DTOs # some of domain types, presentable for responses
│   │   │   └───Entities # all domain types, mapped to DB tables, directly used by Entity Framework ORM
│   │   ├───Infrastructure # interface implementations involving external dependencies
│   │   │   ├───Converters # 1 converter that serializes spatial objects (e.g. EEZ chunks) into GeoJSON features
│   │   │   ├───Persistence # things related directly to DB, including DbContext
│   │   │   │   ├───CustomSchemaMigrations # sql scripts to create stored functions, indexes and only schemas for tables which contain geodata (and therefore will be filled with imported .dump file) 
│   │   │   │   ├───DataMigrations # sql scripts to fill tables of simple non-geo data
│   │   │   │   └───Migrations # usual Entity Framework migrations to create schemas for tables of simple non-geo data
│   │   │   ├───Repositories # data retrieval implementations
│   │   │   ├───Services # gis and AIS related implementations (both getting processed AIS from microservice via gRPC and sending batches further via signalR are here)
│   │   │   └───TransportModels # types used to be sent to frontend beyond http rest - ship marker model for msgpack, spatial object model for ndjson streaming
│   │   ├───Presentation # layer dealing with http request/responses
│   │   │   ├───Controllers # http endpoints, which give out static data, GeoJSON data, geo query results 
│   │   │   └───ViewModels # http request/response models 
│   │   └───Properties #default launchsettings here; http and https ports agreed for main backend are taken from here
│   └───StreamAIS #Project of microservice for listening to AIS input stream
│       ├───Models # various helper types to organize websocket connection and deserialization of AIS traffic with source-generated code
│       │   ├───Abstract #base types with repeated members used in concrete types below
│       │   └───AIS #types matched exactly to AIS message bodies of types relevant for the solution, to deserialize arriving bytes into
│       ├───Properties #default launchsettings here
│       └───sql # sql scripts with schema migrations for clickhouse - AIS history table, view of all usage session IDs
├───mar-in-time-frontend
│   ├───nginx # proxy configurations for with and without docker
│   ├───public # favicons (usual one and for docker)
│   └───src # actual frontend development here
│       ├───app # Angular components and services
│       │   ├───coordinates-display # element with 2 coordinates (for selected point) in 2 formats
│       │   │   └───direction-span # indivisible element with direction letter and 1 coordinate
│       │   ├───main-map # wrapper around LeafletJS map
│       │   │   └───leaflet-logic # actual map layers logic, written in pure JS
│       │   │       └───web-workers # 1 web worker that turns marker data batches into ready GeoJSON features
│       │   ├───models # 1 model, for custom config data (the config regrds whether app is working in container or no)
│       │   ├───point-summary # display for text info about selected point
│       │   └───side-info # collapsible side panel in general, fetching point info
│       └───assets # Frontend libs not replenishable by npm install (= download them yourself via links in Dependencies section, and put in folders as shown below); and static images
│           ├───ui-images # static images of markers for the map (this folder is present in repo)
│           ├---signalr # lib for client server persistent connection to pass data in real time
│           ├---leaflet # lib for web map
│           └---leaflet-glify # lib to draw GeoJSON shapes with WebGL
└───sql # non-migrations sql scripts that were once used during development
    └───derived tables # variations of some of geo data
        └───eez # sql scripts to create variations of EEZ with different detailing for different zoom tiers
```

</details>

## Future plans

- Replay of stored AIS history
- Lines of common maritime routes
- Joining together chunks of same EEZ
- Clickable markers with ships info display
- Filtering AIS traffic by ship IMO, type, country, etc
- Weather layer on the map
- More types of areas in water - no-go winds/waves/ice, piracy, military exclusions, ecologically significant, etc
- Displaying recent path of a ship

### Static data sources
---
- Exclusive economic zones (EEZ) - [MarineRegions](https://www.marineregions.org/downloads.php) Maritime Boundaries (latest version) - World EEZ v12 - Shapefile
- Landmass - [OpenStreetMap](https://osmdata.openstreetmap.de/data/land-polygons.html) Land polygons - WGS84, Large polygons are split