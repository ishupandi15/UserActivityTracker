USE UserActivityDB;
GO

-- replace the session id below with the one you saw
DECLARE @SessionId UNIQUEIDENTIFIER = 'e1f733c3-91d7-40a9-9edc-2cb1d62219e4';

SELECT 
    ID,
    UserID,
    ActivityType,
    ActivityTimestamp
FROM UserActivities
WHERE SessionID = CONVERT(varchar(36), @SessionId)   -- SessionID stored as string in your model
ORDER BY ActivityTimestamp;
