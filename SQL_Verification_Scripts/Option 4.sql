USE UserActivityDB;
GO

DECLARE @TargetDate DATE = '2026-02-12';

;WITH DailyActivity AS
(
    SELECT DISTINCT
        a.UserID
    FROM UserActivities a
    WHERE a.ActivityTimestamp >= CAST(@TargetDate AS datetimeoffset)
      AND a.ActivityTimestamp <  DATEADD(DAY, 1, CAST(@TargetDate AS datetimeoffset))
)
SELECT 
    COUNT(*) AS DAU,
    STRING_AGG(u.Username, ', ') AS ActiveUsers
FROM DailyActivity d
JOIN Users u ON u.ID = d.UserID;

