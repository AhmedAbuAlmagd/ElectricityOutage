USE [ElectricityOutageDB]
GO
/****** Object:  StoredProcedure [FTA].[SP_BuildHierarchy]    Script Date: 10/8/2025 9:29:56 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. SP_BuildHierarchy: Builds the network hierarchy from STA to FTA
ALTER   PROCEDURE [FTA].[SP_BuildHierarchy]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        -- Ensure Hierarchy Paths exist
        MERGE FTA.Network_Element_Hierarchy_Path AS target
        USING (VALUES (1, 'Governrate -> Sector -> Zone -> City -> Station -> Tower -> Cabin -> Cable -> Block -> Building -> Flat -> Individual Subscription', 'Indiv'),
                     (2, 'Governrate -> Sector -> Zone -> City -> Station -> Tower -> Cabin -> Cable -> Block -> Building -> Corporate Subscription', 'Corp')) 
        AS source (Network_Element_Hierarchy_Path_Key, Netwrok_Element_Hierarchy_Path_Name, Abbreviation)
        ON target.Network_Element_Hierarchy_Path_Key = source.Network_Element_Hierarchy_Path_Key
        WHEN NOT MATCHED THEN 
            INSERT (Network_Element_Hierarchy_Path_Key, Netwrok_Element_Hierarchy_Path_Name, Abbreviation) 
            VALUES (source.Network_Element_Hierarchy_Path_Key, source.Netwrok_Element_Hierarchy_Path_Name, source.Abbreviation);

        -- Clear dependents FIRST (to avoid FK errors)
        DELETE FROM FTA.Cutting_Down_Detail WHERE EXISTS (SELECT 1 FROM FTA.Network_Element ne WHERE ne.Network_Element_Key = FTA.Cutting_Down_Detail.Network_Element_Key);
        DELETE FROM FTA.Cutting_Down_Header;  -- Safe if FK exists

        -- Now clear main tables
        DELETE FROM FTA.Network_Element;
        DELETE FROM FTA.Network_Element_Type;

        -- Insert Types (13 types for both paths)
        INSERT INTO FTA.Network_Element_Type (Network_Element_Type_key, Network_Element_Type_Name, Parent_Network_Element_Type_key, Network_Element_Hierarchy_Path_Key) VALUES
        (1, 'Governrate', NULL, 1),
        (2, 'Sector', 1, 1),
        (3, 'Zone', 2, 1),
        (4, 'City', 3, 1),
        (5, 'Station', 4, 1),
        (6, 'Tower', 5, 1),
        (7, 'Cabin', 6, 1),
        (8, 'Cable', 7, 1),
        (9, 'Block', 8, 1),
        (10, 'Building', 9, 1),
        (11, 'Flat', 10, 1),
        (12, 'Individual Subscription', 11, 1),
        (13, 'Corporate Subscription', 10, 2);

        -- MERGE Governrate
        MERGE FTA.Network_Element AS target
        USING (SELECT Governrate_Key AS k, Governrate_Name AS n FROM STA.Governrate) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 1, NULL);

        -- MERGE Sector
        MERGE FTA.Network_Element AS target
        USING (SELECT Sector_Key AS k, Sector_Name AS n, Governrate_Key AS p FROM STA.Sector) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 2, source.p);

        -- MERGE Zone
        MERGE FTA.Network_Element AS target
        USING (SELECT Zone_Key AS k, Zone_Name AS n, Sector_Key AS p FROM STA.Zone) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 3, source.p);

        -- MERGE City
        MERGE FTA.Network_Element AS target
        USING (SELECT City_Key AS k, City_Name AS n, Zone_Key AS p FROM STA.City) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 4, source.p);

        -- MERGE Station
        MERGE FTA.Network_Element AS target
        USING (SELECT Station_Key AS k, Station_Name AS n, City_Key AS p FROM STA.Station) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 5, source.p);

        -- MERGE Tower
        MERGE FTA.Network_Element AS target
        USING (SELECT Tower_Key AS k, Tower_Name AS n, Station_Key AS p FROM STA.Tower) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 6, source.p);

        -- MERGE Cabin
        MERGE FTA.Network_Element AS target
        USING (SELECT Cabin_Key AS k, Cabin_Name AS n, Tower_Key AS p FROM STA.Cabin) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 7, source.p);

        -- MERGE Cable
        MERGE FTA.Network_Element AS target
        USING (SELECT Cable_Key AS k, Cable_Name AS n, Cabin_Key AS p FROM STA.Cable) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 8, source.p);

        -- MERGE Block
        MERGE FTA.Network_Element AS target
        USING (SELECT Block_Key AS k, Block_Name AS n, Cable_Key AS p FROM STA.Block) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 9, source.p);

        -- MERGE Building
        MERGE FTA.Network_Element AS target
        USING (SELECT Building_Key AS k, Building_Name AS n, Block_Key AS p FROM STA.Building) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 10, source.p);

        -- MERGE Flat
        MERGE FTA.Network_Element AS target
        USING (SELECT Flat_Key AS k, CAST(Flat_Key AS NVARCHAR(100)) AS n, Building_Key AS p FROM STA.Flat) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 11, source.p);

        -- MERGE Subscription (Individual Path, Type=12)
        MERGE FTA.Network_Element AS target
        USING (SELECT Subscription_Key AS k, 'Sub-' + CAST(Subscription_Key AS NVARCHAR(10)) AS n, Flat_Key AS p FROM STA.Subscription) AS source
        ON target.Network_Element_Key = source.k
        WHEN NOT MATCHED THEN INSERT (Network_Element_Key, Network_Element_Name, Network_Element_Type_Key, Parent_Network_Element_Key) VALUES (source.k, source.n, 12, source.p);

        COMMIT TRANSACTION;
        PRINT 'Hierarchy built successfully - No FK errors!';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        PRINT 'Error: ' + ERROR_MESSAGE();
        THROW;  -- Rethrow inside CATCH
    END CATCH
END





USE [ElectricityOutageDB]
GO
/****** Object:  StoredProcedure [FTA].[SP_Close]    Script Date: 10/8/2025 9:30:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [FTA].[SP_Close]
    @ChannelKey INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @ChannelKey = 1 -- Source A (Cabins)
        BEGIN
            -- Update existing headers for Source A
            UPDATE H
            SET
                H.ActualEndDate = A.EndDate,
                H.SynchUpdateDate = GETDATE(),
                H.UpdateSystemUserID = COALESCE(U.User_Key, 3)
            FROM FTA.Cutting_Down_Header H
            INNER JOIN STA.Cutting_Down_A A ON H.Cutting_Down_Incident_ID = A.Cutting_Down_A_Incident_ID
            LEFT JOIN FTA.Users U ON U.Name = A.UpdatedUser
            WHERE H.Channel_Key = 1
            AND A.EndDate IS NOT NULL
            AND H.ActualEndDate IS NULL;

            PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' closed cabin incidents';
        END
        ELSE -- Source B (Cables)
        BEGIN
            -- Update existing headers for Source B
            UPDATE H
            SET
                H.ActualEndDate = B.EndDate,
                H.SynchUpdateDate = GETDATE(),
                H.UpdateSystemUserID = COALESCE(U.User_Key, 4)
            FROM FTA.Cutting_Down_Header H
            INNER JOIN STA.Cutting_Down_B B ON H.Cutting_Down_Incident_ID = B.Cutting_Down_B_Incident_ID
            LEFT JOIN FTA.Users U ON U.Name = B.UpdatedUser
            WHERE H.Channel_Key = 2
            AND B.EndDate IS NOT NULL
            AND H.ActualEndDate IS NULL;

            PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' closed cable incidents';
        END



        COMMIT TRANSACTION;

        PRINT 'Closed incidents updated!';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT 'Error in SP_Close: ' + ISNULL(@Err, 'Unknown error');
        THROW;
    END CATCH
END



USE [ElectricityOutageDB]
GO
/****** Object:  StoredProcedure [FTA].[SP_Create]    Script Date: 10/8/2025 9:30:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [FTA].[SP_Create]
    @ChannelKey INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @UserKey INT = CASE WHEN @ChannelKey = 1 THEN 3 ELSE 4 END;
        DECLARE @MaxKey INT;
        SELECT @MaxKey = ISNULL(MAX(Cutting_Down_Key), 0) FROM FTA.Cutting_Down_Header;

        IF @ChannelKey = 1 -- Source A (Cabins)
        BEGIN
            DECLARE cur_A CURSOR LOCAL FOR
                SELECT 
                    Cutting_Down_A_Incident_ID,
                    Problem_Type_Key,
                    CreateDate,
                    IsPlanned,
                    IsGlobal,
                    PlannedStartDTS,
                    PlannedEndDTS,
                    IsActive,
                    CreatedUser,
                    Cutting_Down_Cabin_Name
                FROM STA.Cutting_Down_A
                WHERE EndDate IS NULL 
                AND IsActive = 1
                AND Cutting_Down_A_Incident_ID NOT IN (
                    SELECT ISNULL(Cutting_Down_Incident_ID, 0)
                    FROM FTA.Cutting_Down_Header 
                    WHERE Channel_Key = 1
                );

            OPEN cur_A;

            DECLARE @IncID INT, @ProbKey INT, @CreateDt DATETIME, @IsPlan BIT, @IsGlob BIT,
                    @PlanStart DATETIME, @PlanEnd DATETIME, @IsAct BIT, @CreatedUsr NVARCHAR(200),
                    @ElementName NVARCHAR(200), @NetKey INT, @HeaderKey INT;

            FETCH NEXT FROM cur_A INTO @IncID, @ProbKey, @CreateDt, @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @CreatedUsr, @ElementName;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                PRINT 'Processing Cabin Incident - ID: ' + CAST(@IncID AS NVARCHAR) + ', Problem_Type: ' + CAST(@ProbKey AS NVARCHAR) + ', Element: ' + ISNULL(@ElementName, 'NULL');

                SELECT @NetKey = Network_Element_Key FROM FTA.Network_Element WHERE Network_Element_Name = @ElementName;
                PRINT 'Found Network_Element_Key: ' + ISNULL(CAST(@NetKey AS NVARCHAR), 'NULL');

                SET @MaxKey = @MaxKey + 1;
                SET @HeaderKey = @MaxKey;

                INSERT INTO FTA.Cutting_Down_Header
                    (Cutting_Down_Key, Cutting_Down_Incident_ID, Channel_Key, Cutting_Down_Problem_Type_Key, 
                     ActualCreateDate, SynchCreateDate, IsPlanned, IsGlobal, PlannedStartDTS, PlannedEndDTS, IsActive, CreateSystemUserID)
                VALUES
                    (@HeaderKey, @IncID, @ChannelKey, @ProbKey, @CreateDt, GETDATE(), @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @UserKey);

                IF @NetKey IS NOT NULL
                BEGIN
                    PRINT 'Inserting into Detail table';
                    INSERT INTO FTA.Cutting_Down_Detail
                        (Cutting_Down_Key, Network_Element_Key, ActualCreateDate, ImpactedCustomers)
                    VALUES
                        (@HeaderKey, @NetKey, @CreateDt, 100);
                END
                ELSE
                BEGIN
                    PRINT 'Inserting into Ignored table';
                    INSERT INTO FTA.Cutting_Down_Ignored
                        (Cutting_Down_Incident_ID, ActualCreateDate, SynchCreateDate, Cabin_Name, CreatedUser)
                    VALUES
                        (@IncID, @CreateDt, GETDATE(), @ElementName, @CreatedUsr);
                END

                FETCH NEXT FROM cur_A INTO @IncID, @ProbKey, @CreateDt, @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @CreatedUsr, @ElementName;
            END

            CLOSE cur_A;
            DEALLOCATE cur_A;
        END
        ELSE -- Source B (Cables)
        BEGIN
            DECLARE cur_B CURSOR LOCAL FOR
                SELECT 
                    Cutting_Down_B_Incident_ID,
                    Problem_Type_Key,
                    CreateDate,
                    IsPlanned,
                    IsGlobal,
                    PlannedStartDTS,
                    PlannedEndDTS,
                    IsActive,
                    CreatedUser,
                    Cutting_Down_Cable_Name
                FROM STA.Cutting_Down_B
                WHERE EndDate IS NULL 
                AND IsActive = 1
                AND Cutting_Down_B_Incident_ID NOT IN (
                    SELECT ISNULL(Cutting_Down_Incident_ID, 0)
                    FROM FTA.Cutting_Down_Header 
                    WHERE Channel_Key = 2
                );

            OPEN cur_B;

            FETCH NEXT FROM cur_B INTO @IncID, @ProbKey, @CreateDt, @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @CreatedUsr, @ElementName;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                PRINT 'Processing Cable Incident - ID: ' + CAST(@IncID AS NVARCHAR) + ', Problem_Type: ' + CAST(@ProbKey AS NVARCHAR) + ', Element: ' + ISNULL(@ElementName, 'NULL');

                SELECT @NetKey = Network_Element_Key FROM FTA.Network_Element WHERE Network_Element_Name = @ElementName;
                PRINT 'Found Network_Element_Key: ' + ISNULL(CAST(@NetKey AS NVARCHAR), 'NULL');

                SET @MaxKey = @MaxKey + 1;
                SET @HeaderKey = @MaxKey;

                INSERT INTO FTA.Cutting_Down_Header
                    (Cutting_Down_Key, Cutting_Down_Incident_ID, Channel_Key, Cutting_Down_Problem_Type_Key, 
                     ActualCreateDate, SynchCreateDate, IsPlanned, IsGlobal, PlannedStartDTS, PlannedEndDTS, IsActive, CreateSystemUserID)
                VALUES
                    (@HeaderKey, @IncID, @ChannelKey, @ProbKey, @CreateDt, GETDATE(), @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @UserKey);

                IF @NetKey IS NOT NULL
                BEGIN
                    PRINT 'Inserting into Detail table';
                    INSERT INTO FTA.Cutting_Down_Detail
                        (Cutting_Down_Key, Network_Element_Key, ActualCreateDate, ImpactedCustomers)
                    VALUES
                        (@HeaderKey, @NetKey, @CreateDt, 100);
                END
                ELSE
                BEGIN
                    PRINT 'Inserting into Ignored table';
                    INSERT INTO FTA.Cutting_Down_Ignored
                        (Cutting_Down_Incident_ID, ActualCreateDate, SynchCreateDate, Cabel_Name, CreatedUser)
                    VALUES
                        (@IncID, @CreateDt, GETDATE(), @ElementName, @CreatedUsr);
                END

                FETCH NEXT FROM cur_B INTO @IncID, @ProbKey, @CreateDt, @IsPlan, @IsGlob, @PlanStart, @PlanEnd, @IsAct, @CreatedUsr, @ElementName;
            END

            CLOSE cur_B;
            DEALLOCATE cur_B;
        END

        COMMIT TRANSACTION;
        PRINT 'SP_Create completed successfully!';

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        PRINT 'Error in SP_Create: ' + ISNULL(@ErrMsg, 'Unknown error');
        THROW;
    END CATCH
END




USE [ElectricityOutageDB]
GO
/****** Object:  StoredProcedure [FTA].[SP_GetAllParents]    Script Date: 10/8/2025 9:30:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [FTA].[SP_GetAllParents]
    @NetworkElementKey INT
AS
BEGIN
    SET NOCOUNT ON;
    WITH ParentCTE AS (
        SELECT ne.Network_Element_Key, ne.Network_Element_Name, net.Network_Element_Type_Name, ne.Parent_Network_Element_Key, 0 AS Level
        FROM FTA.Network_Element ne
        INNER JOIN FTA.Network_Element_Type net ON ne.Network_Element_Type_Key = net.Network_Element_Type_key
        WHERE ne.Network_Element_Key = @NetworkElementKey
        UNION ALL
        SELECT ne.Network_Element_Key, ne.Network_Element_Name, net.Network_Element_Type_Name, ne.Parent_Network_Element_Key, p.Level + 1
        FROM FTA.Network_Element ne
        INNER JOIN FTA.Network_Element_Type net ON ne.Network_Element_Type_Key = net.Network_Element_Type_key
        INNER JOIN ParentCTE p ON ne.Network_Element_Key = p.Parent_Network_Element_Key
        WHERE p.Parent_Network_Element_Key IS NOT NULL
    )
    SELECT Network_Element_Key, Network_Element_Name, Network_Element_Type_Name, Level
    FROM ParentCTE
    ORDER BY Level DESC;  -- Root at top
END



--Function 

USE [ElectricityOutageDB]
GO
/****** Object:  UserDefinedFunction [FTA].[FN_CalculateImpactedCustomers]    Script Date: 10/8/2025 9:31:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   FUNCTION [FTA].[FN_CalculateImpactedCustomers] (@NetworkElementKey INT)
RETURNS @Impacted TABLE (TotalImpacted INT)
AS
BEGIN
    WITH DescendantCTE AS (
        SELECT ne.Network_Element_Key, 0 AS Level
        FROM FTA.Network_Element ne
        WHERE ne.Network_Element_Key = @NetworkElementKey
        UNION ALL
        SELECT ne.Network_Element_Key, d.Level + 1
        FROM FTA.Network_Element ne
        INNER JOIN DescendantCTE d ON ne.Parent_Network_Element_Key = d.Network_Element_Key
    )
    INSERT INTO @Impacted
    SELECT COUNT(*) AS TotalImpacted
    FROM DescendantCTE dc
    INNER JOIN FTA.Network_Element ne ON dc.Network_Element_Key = ne.Network_Element_Key
    WHERE ne.Network_Element_Type_Key IN (12, 13);  -- Subscription leaves
    RETURN;
END
