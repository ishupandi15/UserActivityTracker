USE UserActivityDB;
GO

SELECT 
    u.ID AS UserID,
    u.Username,
    u.Email,
    a.ID AS ActivityID,
    a.ActivityType,
    a.ActivityTimestamp,
    a.SessionID,
    a.Metadata
FROM Users u
LEFT JOIN UserActivities a ON u.ID = a.UserID
ORDER BY a.ActivityTimestamp DESC;
