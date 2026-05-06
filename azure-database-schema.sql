-- ============================================================================
-- FinanceSap.Enterprise - Azure MySQL Database Schema
-- Gerado em: 2025-01-20
-- Versão: 1.0
-- ============================================================================
-- Este script cria todas as tabelas necessárias para o FinanceSap.Enterprise
-- Baseado nas migrations do EF Core (Customers, Accounts, Loans, Identity)
-- ============================================================================

-- Configurações de charset e collation
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- ============================================================================
-- 1. TABELA: customers
-- ============================================================================
CREATE TABLE IF NOT EXISTS `customers` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `full_name` VARCHAR(150) NOT NULL,
    `document` VARCHAR(11) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_customers_document` (`document`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================================
-- 2. TABELA: accounts
-- ============================================================================
CREATE TABLE IF NOT EXISTS `accounts` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `AccountNumber` VARCHAR(10) NOT NULL,
    `Balance` DECIMAL(18,2) NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL,
    `CustomerId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_accounts_account_number` (`AccountNumber`),
    UNIQUE INDEX `IX_accounts_customer_id` (`CustomerId`),
    CONSTRAINT `FK_accounts_customers_CustomerId` 
        FOREIGN KEY (`CustomerId`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================================
-- 3. TABELA: loan_applications
-- ============================================================================
CREATE TABLE IF NOT EXISTS `loan_applications` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `CustomerId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `Amount` DECIMAL(18,2) NOT NULL,
    `TermInMonths` INT NOT NULL,
    `Status` LONGTEXT NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_loan_applications_CustomerId` (`CustomerId`),
    CONSTRAINT `FK_loan_applications_customers_CustomerId` 
        FOREIGN KEY (`CustomerId`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================================
-- 4. TABELA: loans
-- ============================================================================
CREATE TABLE IF NOT EXISTS `loans` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `customer_id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `principal_amount` DECIMAL(18,2) NOT NULL,
    `interest_rate` DECIMAL(5,2) NOT NULL,
    `installments` INT NOT NULL,
    `monthly_payment_amount` DECIMAL(18,2) NOT NULL,
    `total_to_pay` DECIMAL(18,2) NOT NULL,
    `status` VARCHAR(20) NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_loans_customer_id` (`customer_id`),
    INDEX `IX_loans_status` (`status`),
    CONSTRAINT `FK_loans_customers_customer_id` 
        FOREIGN KEY (`customer_id`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================================
-- 5. ASP.NET CORE IDENTITY - TABELAS
-- ============================================================================

-- 5.1 AspNetRoles
CREATE TABLE IF NOT EXISTS `AspNetRoles` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `Name` VARCHAR(256) NULL,
    `NormalizedName` VARCHAR(256) NULL,
    `ConcurrencyStamp` LONGTEXT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.2 AspNetUsers
CREATE TABLE IF NOT EXISTS `AspNetUsers` (
    `Id` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `CustomerId` CHAR(36) NULL COLLATE ascii_general_ci,
    `UserName` VARCHAR(256) NULL,
    `NormalizedUserName` VARCHAR(256) NULL,
    `Email` VARCHAR(256) NULL,
    `NormalizedEmail` VARCHAR(256) NULL,
    `EmailConfirmed` TINYINT(1) NOT NULL,
    `PasswordHash` LONGTEXT NULL,
    `SecurityStamp` LONGTEXT NULL,
    `ConcurrencyStamp` LONGTEXT NULL,
    `PhoneNumber` LONGTEXT NULL,
    `PhoneNumberConfirmed` TINYINT(1) NOT NULL,
    `TwoFactorEnabled` TINYINT(1) NOT NULL,
    `LockoutEnd` DATETIME(6) NULL,
    `LockoutEnabled` TINYINT(1) NOT NULL,
    `AccessFailedCount` INT NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `EmailIndex` (`NormalizedEmail`),
    UNIQUE INDEX `UserNameIndex` (`NormalizedUserName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.3 AspNetRoleClaims
CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `RoleId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `ClaimType` LONGTEXT NULL,
    `ClaimValue` LONGTEXT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` 
        FOREIGN KEY (`RoleId`) 
        REFERENCES `AspNetRoles` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.4 AspNetUserClaims
CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `ClaimType` LONGTEXT NULL,
    `ClaimValue` LONGTEXT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.5 AspNetUserLogins
CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
    `LoginProvider` VARCHAR(255) NOT NULL,
    `ProviderKey` VARCHAR(255) NOT NULL,
    `ProviderDisplayName` LONGTEXT NULL,
    `UserId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    INDEX `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.6 AspNetUserRoles
CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
    `UserId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `RoleId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    PRIMARY KEY (`UserId`, `RoleId`),
    INDEX `IX_AspNetUserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` 
        FOREIGN KEY (`RoleId`) 
        REFERENCES `AspNetRoles` (`Id`) 
        ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.7 AspNetUserTokens
CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
    `UserId` CHAR(36) NOT NULL COLLATE ascii_general_ci,
    `LoginProvider` VARCHAR(255) NOT NULL,
    `Name` VARCHAR(255) NOT NULL,
    `Value` LONGTEXT NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================================
-- 6. TABELA DE CONTROLE DE MIGRATIONS (EF Core)
-- ============================================================================
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Registra as migrations aplicadas
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES 
    ('20260420050010_InitialCreate', '9.0.0'),
    ('20260420052513_PromoteCustomerToAggregate', '9.0.0'),
    ('20260422001932_AddIdentityAndAccount', '9.0.0')
ON DUPLICATE KEY UPDATE `ProductVersion` = VALUES(`ProductVersion`);

-- ============================================================================
-- FIM DO SCRIPT
-- ============================================================================
-- Para executar este script no Azure MySQL:
-- 1. Conecte-se ao seu servidor MySQL no Azure
-- 2. Execute: mysql -h <seu-servidor>.mysql.database.azure.com -u <usuario> -p <database> < azure-database-schema.sql
-- 3. Configure a variável de ambiente MYSQL_CONNECTION_STRING no Azure App Service
-- ============================================================================
