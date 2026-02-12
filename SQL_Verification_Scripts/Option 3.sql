USE UserActivityDB;
GO

SELECT 
    AVG(CAST(SessionCount AS FLOAT)) AS AverageActivitiesPerSession
FROM (
    SELECT 
        SessionID,
        COUNT(*) AS SessionCount
    FROM UserActivities
    GROUP BY SessionID
) AS SessionCounts;
