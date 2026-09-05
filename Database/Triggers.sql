SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[trg_Employee_AutoUpdateDate]
ON [dbo].[Employee]
AFTER UPDATE
AS
BEGIN
    UPDATE Employee
    SET UpdateDate = GETDATE()
    FROM Employee e
    INNER JOIN inserted i ON e.WorkerID = i.WorkerID;
END;
GO
ALTER TABLE [dbo].[Employee] ENABLE TRIGGER [trg_Employee_AutoUpdateDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[trg_CheckBudgetOverflow]
ON [dbo].[Profit]
AFTER UPDATE, INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ProjectID INT;
    DECLARE @TotalExpenses DECIMAL(18,2);
    DECLARE @Budget DECIMAL(18,2);
    DECLARE @ProjectName NVARCHAR(255);

    SELECT @ProjectID = i.ProjectID, @TotalExpenses = i.TotalExpenses FROM inserted i;

    SELECT @Budget = Budget, @ProjectName = ProjectName FROM Project WHERE ProjectID = @ProjectID;

    IF @Budget IS NULL RETURN; 

    IF @TotalExpenses > @Budget
    BEGIN
        DECLARE @ErrorMessage NVARCHAR(500);
        SET @ErrorMessage = 
            'Budget Overflow! Project "' + ISNULL(@ProjectName, 'Unknown') + '" (ID: ' + CAST(@ProjectID AS VARCHAR(10)) + ')' +
            ' - Budget: $' + CAST(@Budget AS VARCHAR(20)) +
            ', Total Expenses: $' + CAST(@TotalExpenses AS VARCHAR(20)) +
            ', Overflow: $' + CAST((@TotalExpenses - @Budget) AS VARCHAR(20));
        
        RAISERROR(@ErrorMessage, 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO
ALTER TABLE [dbo].[Profit] ENABLE TRIGGER [trg_CheckBudgetOverflow]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[trg_PreventDeleteCompletedProject]
ON [dbo].[Project]
INSTEAD OF DELETE
AS
BEGIN
    DECLARE @Status NVARCHAR(20);
    SELECT @Status = ProjectStatus FROM deleted;

    IF @Status = 'Completed'
    BEGIN
        RAISERROR('Completed projects can not be deleted.', 16, 1);
        ROLLBACK TRANSACTION;
    END
    ELSE
    BEGIN
        DELETE FROM Project WHERE ProjectID IN (SELECT ProjectID FROM deleted);
    END
END;
GO
ALTER TABLE [dbo].[Project] ENABLE TRIGGER [trg_PreventDeleteCompletedProject]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_Project_AutoUpdateDate]
ON [dbo].[Project]
AFTER UPDATE
AS
BEGIN
    UPDATE Project
    SET UpdateDate = GETDATE()
    FROM Project p
    INNER JOIN inserted i ON p.ProjectID = i.ProjectID;
END;
GO
ALTER TABLE [dbo].[Project] ENABLE TRIGGER [trg_Project_AutoUpdateDate]
GO