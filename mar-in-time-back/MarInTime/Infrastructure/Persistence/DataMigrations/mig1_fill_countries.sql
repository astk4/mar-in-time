DO $$

BEGIN
	
IF (SELECT COUNT("IsoId") FROM "Countries") > 0
	THEN
		RAISE NOTICE 'Countries table already filled';
		RETURN;
	END IF;

INSERT INTO "Countries" ("IsoId", "Name") VALUES ('NL', 'Netherlands');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('BE', 'Belgium');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('DE', 'Germany');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('GB', 'United Kingdom');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('FR', 'France');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('DK', 'Denmark');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('SE', 'Sweden');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('NO', 'Norway');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('RU', 'Russia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('PL', 'Poland');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('LV', 'Latvia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('LT', 'Lithuania');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('EE', 'Estonia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('FI', 'Finland');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('IT', 'Italy');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('ES', 'Spain');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('GR', 'Greece');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('TR', 'Turkey');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('UA', 'Ukraine');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('RO', 'Romania');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('BG', 'Bulgaria');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('CN', 'China');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('HK', 'Hong Kong');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('KR', 'South Korea');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('TW', 'Taiwan');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('JP', 'Japan');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('SG', 'Singapore');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('MY', 'Malaysia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('TH', 'Thailand');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('VN', 'Vietnam');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('IN', 'India');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('LK', 'Sri Lanka');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('PK', 'Pakistan');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('AE', 'United Arab Emirates');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('OM', 'Oman');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('QA', 'Qatar');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('SA', 'Saudi Arabia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('IR', 'Iran');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('US', 'United States');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('CA', 'Canada');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('MX', 'Mexico');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('PA', 'Panama');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('BR', 'Brazil');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('AR', 'Argentina');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('PE', 'Peru');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('CO', 'Colombia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('EC', 'Ecuador');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('CL', 'Chile');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('EG', 'Egypt');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('DJ', 'Djibouti');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('KE', 'Kenya');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('TZ', 'Tanzania');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('MZ', 'Mozambique');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('ZA', 'South Africa');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('NA', 'Namibia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('NG', 'Nigeria');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('CI', 'Côte d''Ivoire');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('GH', 'Ghana');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('AU', 'Australia');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('NZ', 'New Zealand');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('PG', 'Papua New Guinea');
INSERT INTO "Countries" ("IsoId", "Name") VALUES ('FJ', 'Fiji');

END $$;
