USE UserActivityDB;
GO

SET DATEFIRST 1;

DECLARE @InputDate DATE = '2026-02-14';  -- <-- replace as needed

DECLARE @WeekStart DATE = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @InputDate), @InputDate);
DECLARE @WeekEnd   DATE = DATEADD(DAY, 6, @WeekStart);

DECLARE @WeekStartDO DATETIMEOFFSET = CAST(@WeekStart AS DATETIMEOFFSET);
DECLARE @WeekEndDO   DATETIMEOFFSET = DATEADD(DAY, 1, CAST(@WeekEnd AS DATETIMEOFFSET));

SELECT
    CONCAT('From ', CONVERT(VARCHAR(10), @WeekStart, 120),
           ' to ', CONVERT(VARCHAR(10), @WeekEnd, 120),
           ' : ', COUNT(DISTINCT UserID), ' users') AS Summary
FROM UserActivities
WHERE ActivityTimestamp >= @WeekStartDO
  AND ActivityTimestamp <  @WeekEndDO;