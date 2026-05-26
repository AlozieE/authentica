IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.AspNetUsers') AND name = N'TwoFactorSecret'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers
        ADD TwoFactorSecret   NVARCHAR(MAX) NULL,
            TwoFactorSecretIV NVARCHAR(MAX) NULL;
END
