DO $$

BEGIN
	
IF (SELECT COUNT("Id") FROM "Routes") > 0
	THEN
		RAISE NOTICE '"Routes" table already filled';
		RETURN;
	END IF;

INSERT INTO "Routes" ("Id", "Name") VALUES (1, 'North Atlantic Route (Europe-North America)');
INSERT INTO "Routes" ("Id", "Name") VALUES (2, 'Trans-Pacific Route (Asia-North America)');
INSERT INTO "Routes" ("Id", "Name") VALUES (3, 'Europe-Asia Route (via Suez Canal)');
INSERT INTO "Routes" ("Id", "Name") VALUES (4, 'English Channel Route');
INSERT INTO "Routes" ("Id", "Name") VALUES (5, 'Strait of Malacca (Southeast Asia Gateway)');
INSERT INTO "Routes" ("Id", "Name") VALUES (6, 'Strait of Hormuz (Persian Gulf Gateway)');
INSERT INTO "Routes" ("Id", "Name") VALUES (7, 'Panama Canal Route (Pacific-Atlantic Connection)');
INSERT INTO "Routes" ("Id", "Name") VALUES (8, 'Cape of Good Hope Route (Africa Alternative)');
INSERT INTO "Routes" ("Id", "Name") VALUES (9, 'Baltic and North Sea Route');
INSERT INTO "Routes" ("Id", "Name") VALUES (10, 'Black Sea and Mediterranean Route');
INSERT INTO "Routes" ("Id", "Name") VALUES (11, 'Asia-Africa Route');
INSERT INTO "Routes" ("Id", "Name") VALUES (12, 'South American Coastal Route');
INSERT INTO "Routes" ("Id", "Name") VALUES (13, 'Australia-New Zealand-Oceania Route');

END $$;

