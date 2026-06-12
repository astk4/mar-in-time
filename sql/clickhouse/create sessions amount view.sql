CREATE VIEW AIS_SessionsAmount AS 
	SELECT SessionId
	FROM AIS_History
	ORDER BY SessionId DESC
	LIMIT 1;