USE UserActivityDB;
GO

SELECT TOP 10
    u.ID AS UserID,
    u.Username,
    COUNT(a.ID) AS ActivityCount
FROM Users u
LEFT JOIN UserActivities a ON u.ID = a.UserID
GROUP BY u.ID, u.Username
ORDER BY ActivityCount DESC;
