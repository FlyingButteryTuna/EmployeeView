IF OBJECT_ID('dbo.usp_Statuses_Get','P') IS NOT NULL DROP PROCEDURE dbo.usp_Statuses_Get;
GO
CREATE PROCEDURE dbo.usp_Statuses_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.id, s.[name]
    FROM dbo.[status] s
    ORDER BY s.[name];
END
GO

IF OBJECT_ID('dbo.usp_Posts_Get','P') IS NOT NULL DROP PROCEDURE dbo.usp_Posts_Get;
GO
CREATE PROCEDURE dbo.usp_Posts_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id, p.[name]
    FROM dbo.[posts] p
    ORDER BY p.[name];
END
GO

IF OBJECT_ID('dbo.usp_Dependencies_Get','P') IS NOT NULL DROP PROCEDURE dbo.usp_Dependencies_Get;
GO
CREATE PROCEDURE dbo.usp_Dependencies_Get
AS
BEGIN
    SET NOCOUNT ON;
    SELECT d.id, d.[name]
    FROM dbo.[deps] d
    ORDER BY d.[name];
END
GO

IF OBJECT_ID('dbo.usp_Persons_List_v2','P') IS NOT NULL DROP PROCEDURE dbo.usp_Persons_List_v2;
GO
CREATE PROCEDURE dbo.usp_Persons_List_v2
    @StatusId INT = NULL,
    @DepId INT = NULL,
    @PostId INT = NULL,
    @LastNameLike VARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'last_name',
    @SortDir NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @col NVARCHAR(50) = CASE @SortColumn
        WHEN 'last_name' THEN 'last_name'
        WHEN 'status_name' THEN 'status_name'
        WHEN 'dep_name' THEN 'dep_name'
        WHEN 'post_name' THEN 'post_name'
        WHEN 'date_employ' THEN 'date_employ'
        WHEN 'date_uneploy' THEN 'date_uneploy'
        ELSE 'last_name' END;

    DECLARE @dir NVARCHAR(4) = CASE UPPER(@SortDir) WHEN 'DESC' THEN 'DESC' ELSE 'ASC' END;

    DECLARE @sql NVARCHAR(MAX) = N'
    SELECT pr.id, pr.first_name, pr.second_name, pr.last_name,
           pr.date_employ, pr.date_uneploy,
           s.[name] AS status_name,
           d.[name] AS dep_name,
           po.[name] AS post_name
    FROM dbo.persons pr
    INNER JOIN dbo.[status] s ON pr.[status] = s.id
    INNER JOIN dbo.deps d ON pr.id_dep = d.id
    INNER JOIN dbo.posts po ON pr.id_post = po.id
    WHERE (@StatusId IS NULL OR pr.[status] = @StatusId)
      AND (@DepId IS NULL OR pr.id_dep = @DepId)
      AND (@PostId IS NULL OR pr.id_post = @PostId)
      AND (@LastNameLike IS NULL OR pr.last_name LIKE ''%'' + @LastNameLike + ''%'')
    ORDER BY ' + QUOTENAME(@col) + ' ' + @dir + ';';
    PRINT @sql;
    EXEC sp_executesql @sql,
        N'@StatusId INT, @DepId INT, @PostId INT, @LastNameLike VARCHAR(100)',
        @StatusId=@StatusId, @DepId=@DepId, @PostId=@PostId, @LastNameLike=@LastNameLike;
END
GO


IF OBJECT_ID('dbo.usp_Stats_ByDay','P') IS NOT NULL DROP PROCEDURE dbo.usp_Stats_ByDay;
GO
CREATE PROCEDURE dbo.usp_Stats_ByDay
    @StatusId INT,
    @DateFrom DATE,
    @DateTo DATE,
    @IsHired BIT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH R AS (
        SELECT CONVERT(date, CASE WHEN @IsHired=1 THEN pr.date_employ ELSE pr.date_uneploy END) AS d
        FROM dbo.persons pr
        WHERE pr.[status] = @StatusId
          AND CASE WHEN @IsHired=1 THEN pr.date_employ ELSE pr.date_uneploy END IS NOT NULL
          AND CONVERT(date, CASE WHEN @IsHired=1 THEN pr.date_employ ELSE pr.date_uneploy END) BETWEEN @DateFrom AND @DateTo
    )
    SELECT d AS [Day], COUNT(*) AS Cnt
    FROM R
    GROUP BY d
    ORDER BY d;
END
GO