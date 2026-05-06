-- ============================================================================
-- FinanceSap.Enterprise - Complete Database Schema
-- Target: Azure MySQL Database
-- Generated: 2026-04-22
-- ============================================================================

-- Create database if not exists (optional - Azure may create this for you)
-- CREATE DATABASE IF NOT EXISTS financesap_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
-- USE financesap_db;

-- ============================================================================
-- MIGRATION HISTORY TABLE
-- ============================================================================

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- DOMAIN TABLES
-- ============================================================================

-- Customers Table (Aggregate Root)
CREATE TABLE IF NOT EXISTS `customers` (
    `Id` char(36) NOT NULL,
    `document` VARCHAR(11) NOT NULL,
    `full_name` VARCHAR(150) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_customers_document` (`document`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Accounts Table
CREATE TABLE IF NOT EXISTS `accounts` (
    `Id` char(36) NOT NULL,
    `CustomerId` char(36) NOT NULL,
    `AccountNumber` VARCHAR(10) NOT NULL,
    `Balance` decimal(18,2) NOT NULL DEFAULT 0.00,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `IX_accounts_account_number` (`AccountNumber`),
    UNIQUE INDEX `IX_accounts_customer_id` (`CustomerId`),
    CONSTRAINT `FK_accounts_customers_CustomerId` 
        FOREIGN KEY (`CustomerId`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Loan Applications Table
CREATE TABLE IF NOT EXISTS `loan_applications` (
    `Id` char(36) NOT NULL,
    `CustomerId` char(36) NOT NULL,
    `Amount` decimal(18,2) NOT NULL,
    `TermInMonths` int NOT NULL,
    `Status` longtext NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_loan_applications_CustomerId` (`CustomerId`),
    CONSTRAINT `FK_loan_applications_customers_CustomerId` 
        FOREIGN KEY (`CustomerId`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Loans Table (NEW - Loan Module)
CREATE TABLE IF NOT EXISTS `loans` (
    `Id` char(36) NOT NULL,
    `CustomerId` char(36) NOT NULL,
    `PrincipalAmount` decimal(18,2) NOT NULL,
    `InterestRate` decimal(5,4) NOT NULL,
    `Installments` int NOT NULL,
    `MonthlyPaymentAmount` decimal(18,2) NOT NULL,
    `TotalToPay` decimal(18,2) NOT NULL,
    `Status` varchar(50) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_loans_CustomerId` (`CustomerId`),
    INDEX `IX_loans_Status` (`Status`),
    INDEX `IX_loans_CreatedAt` (`CreatedAt`),
    CONSTRAINT `FK_loans_customers_CustomerId` 
        FOREIGN KEY (`CustomerId`) 
        REFERENCES `customers` (`Id`) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- IDENTITY TABLES (ASP.NET Core Identity)
-- ============================================================================

-- Users Table
CREATE TABLE IF NOT EXISTS `AspNetUsers` (
    `Id` char(36) NOT NULL,
    `CustomerId` char(36) NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` tinyint(1) NOT NULL DEFAULT 0,
    `PasswordHash` longtext NULL,
    `SecurityStamp` longtext NULL,
    `ConcurrencyStamp` longtext NULL,
    `PhoneNumber` longtext NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL DEFAULT 0,
    `TwoFactorEnabled` tinyint(1) NOT NULL DEFAULT 0,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL DEFAULT 0,
    `AccessFailedCount` int NOT NULL DEFAULT 0,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `UserNameIndex` (`NormalizedUserName`),
    INDEX `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Roles Table
CREATE TABLE IF NOT EXISTS `AspNetRoles` (
    `Id` char(36) NOT NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` longtext NULL,
    PRIMARY KEY (`Id`),
    UNIQUE INDEX `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Claims Table
CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` char(36) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Logins Table
CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext NULL,
    `UserId` char(36) NOT NULL,
    PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    INDEX `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Roles Table
CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
    `UserId` char(36) NOT NULL,
    `RoleId` char(36) NOT NULL,
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- User Tokens Table
CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
    `UserId` char(36) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext NULL,
    PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` 
        FOREIGN KEY (`UserId`) 
        REFERENCES `AspNetUsers` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Role Claims Table
CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` char(36) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` 
        FOREIGN KEY (`RoleId`) 
        REFERENCES `AspNetRoles` (`Id`) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- MIGRATION HISTORY RECORDS
-- ============================================================================

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES 
    ('20260420050010_InitialCreate', '9.0.0'),
    ('20260420052513_PromoteCustomerToAggregate', '9.0.0'),
    ('20260422001932_AddIdentityAndAccount', '9.0.0')
ON DUPLICATE KEY UPDATE `ProductVersion` = VALUES(`ProductVersion`);

-- ============================================================================
-- SEED DATA (Optional - for testing)
-- ============================================================================

-- Example: Insert a test customer
-- INSERT INTO `customers` (`Id`, `document`, `full_name`)
-- VALUES (UUID(), '12345678901', 'Test Customer')
-- ON DUPLICATE KEY UPDATE `full_name` = VALUES(`full_name`);

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Verify all tables were created
-- SELECT TABLE_NAME FROM information_schema.TABLES 
-- WHERE TABLE_SCHEMA = DATABASE() 
-- ORDER BY TABLE_NAME;

-- Check migration history
-- SELECT * FROM `__EFMigrationsHistory` ORDER BY `MigrationId`;

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================
