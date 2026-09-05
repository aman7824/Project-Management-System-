SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Accounting_GenerateInvoice]
    @HourlyWorkerID INT,
    @HoursWorked DECIMAL(18,2)
AS
BEGIN
    DECLARE @Rate DECIMAL(18,2);
    
    SELECT @Rate = HourlySalary FROM HourlyEmployee WHERE WorkerID = @HourlyWorkerID;

    IF @Rate IS NULL
    BEGIN
        RAISERROR('There is no hourly employee with this ID.', 16, 1);
        RETURN;
    END

    DECLARE @TotalWage DECIMAL(18,2) = @Rate * @HoursWorked;

    INSERT INTO Invoice (WorkerID, EmployeeWage) 
    VALUES (@HourlyWorkerID, @TotalWage);
    
    PRINT 'Invoice created succesfully.';
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Hiring_AddFullTime]
    @FirstName NVARCHAR(50), @LastName NVARCHAR(50),
    @Username NVARCHAR(50), @Email NVARCHAR(100),
    @Phone NVARCHAR(20), @Address NVARCHAR(255),
    @Salary DECIMAL(18,2), @EntranceDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
            INSERT INTO Employee (FirstName, LastName, Username, Email, PhoneNumber, Address, EmployeeType)
            VALUES (@FirstName, @LastName, @Username, @Email, @Phone, @Address, 'F');
            
            DECLARE @NewWorkerID INT = SCOPE_IDENTITY();

            INSERT INTO FullTimeEmployee (WorkerID, EntranceDate, Salary)
            VALUES (@NewWorkerID, @EntranceDate, @Salary);
        COMMIT TRANSACTION;
        PRINT 'Full-Time Worker added succesfully.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Hiring_AddHourly]
    @FirstName NVARCHAR(50), @LastName NVARCHAR(50),
    @Username NVARCHAR(50), @Email NVARCHAR(100),
    @HourlyRate DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
            INSERT INTO Employee (FirstName, LastName, Username, Email, EmployeeType)
            VALUES (@FirstName, @LastName, @Username, @Email, 'H');
            
            DECLARE @NewWorkerID INT = SCOPE_IDENTITY();

            INSERT INTO HourlyEmployee (WorkerID, HourlySalary)
            VALUES (@NewWorkerID, @HourlyRate);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_HR_RequestLeave]
    @WorkerID INT,
    @LeaveType NVARCHAR(50),
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    IF @EndDate <= @StartDate
    BEGIN
        RAISERROR('Invalid date entered.', 16, 1);
        RETURN;
    END

    INSERT INTO Leave (WorkerID, LeaveType, StartDate, EndDate)
    VALUES (@WorkerID, @LeaveType, @StartDate, @EndDate);
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Performance_LogDailyWork]
    @WorkerID INT,
    @Date DATE,
    @WorkedTime DECIMAL(5,2)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM FullTimeEmployee WHERE WorkerID = @WorkerID)
    BEGIN
        RAISERROR('This proccess for only Full Time Employees.', 16, 1);
        RETURN;
    END

    INSERT INTO DailyPerformance (Date, DailyWorkedTime)
    VALUES (@Date, @WorkedTime); 
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Project_AddExpenses]
    @ProjectID INT,
    @ExpenseAmount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Profit WHERE ProjectID = @ProjectID)
    BEGIN
        INSERT INTO Profit (ProjectID, TotalEarnings, TotalExpenses, CreateDate)
        VALUES (@ProjectID, 0, @ExpenseAmount, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE Profit
        SET TotalExpenses = TotalExpenses + @ExpenseAmount,
            UpdateDate = GETDATE()
        WHERE ProjectID = @ProjectID;
    END
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Project_ApproveDraft]
    @DraftID INT
AS
BEGIN
    UPDATE Draft
    SET DraftStatus = 'Approved',
        UpdateDate = GETDATE()
    WHERE DraftID = @DraftID;
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_Project_Complete]
    @ProjectID INT
AS
BEGIN
    UPDATE Project
    SET ProjectStatus = 'Completed',
        FinishDate = GETDATE(),
        UpdateDate = GETDATE()
    WHERE ProjectID = @ProjectID;
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Team_AddMember]
    @TeamID INT,
    @WorkerID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM TeamMembers WHERE TeamID = @TeamID AND WorkerID = @WorkerID)
    BEGIN
        PRINT 'This employee is already in that team.';
        RETURN;
    END

    INSERT INTO TeamMembers (TeamID, WorkerID)
    VALUES (@TeamID, @WorkerID);
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_Team_CreateNew]
    @TeamName NVARCHAR(100),
    @ManagerID INT
AS
BEGIN
    INSERT INTO Team (TeamName, ManagerID)
    VALUES (@TeamName, @ManagerID);
END;