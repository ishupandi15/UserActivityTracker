SELECT TOP (10)
    SessionID,
    COUNT(*) AS ActivityCount,
    MIN(ActivityTimestamp) AS StartTime,
    MAX(ActivityTimestamp) AS EndTime,
    DATEDIFF(SECOND, MIN(ActivityTimestamp), MAX(ActivityTimestamp)) AS DurationInSeconds,
    DATEADD(SECOND,
            DATEDIFF(SECOND, MIN(ActivityTimestamp), MAX(ActivityTimestamp)),
            '00:00:00') AS DurationFormatted
FROM UserActivities
GROUP BY SessionID
ORDER BY MAX(ActivityTimestamp) DESC;
