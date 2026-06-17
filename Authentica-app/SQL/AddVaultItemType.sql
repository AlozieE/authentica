-- Create VaultItemType lookup table
CREATE TABLE VaultItemType (
    VaultItemTypeId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL
);

-- Seed item types
INSERT INTO VaultItemType (Name) VALUES ('Password');
INSERT INTO VaultItemType (Name) VALUES ('CreditCard');
INSERT INTO VaultItemType (Name) VALUES ('SecureNote');

-- Add VaultItemTypeId FK column to VaultItems
ALTER TABLE VaultItems
ADD VaultItemTypeId INT NOT NULL DEFAULT 1;

ALTER TABLE VaultItems
ADD CONSTRAINT FK_VaultItems_VaultItemType
    FOREIGN KEY (VaultItemTypeId) REFERENCES VaultItemType(VaultItemTypeId);

-- Drop old ItemType column
ALTER TABLE VaultItems DROP COLUMN ItemType;
