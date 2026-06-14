CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Account` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Country` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `HealthScore` int NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `IsWorkflowLane` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Account` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `AuditLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EntityType` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `EntityId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Action` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `OldValues` longtext CHARACTER SET utf8mb4 NULL,
        `NewValues` longtext CHARACTER SET utf8mb4 NULL,
        `UserId` char(36) COLLATE ascii_general_ci NULL,
        `ImpersonatorUserId` char(36) COLLATE ascii_general_ci NULL,
        `IPAddress` varchar(45) CHARACTER SET utf8mb4 NULL,
        `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CommunicationLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `RelatedEntityId` char(36) COLLATE ascii_general_ci NULL,
        `RelatedEntityType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Recipient` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `RecipientName` varchar(256) CHARACTER SET utf8mb4 NULL,
        `Subject` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Body` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Channel` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Direction` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SentAt` datetime(6) NULL,
        `IsSent` tinyint(1) NOT NULL,
        `ErrorMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `SentByUserId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CommunicationLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Communications` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Recipient` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `Subject` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Body` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Channel` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Direction` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SentAt` datetime(6) NULL,
        `IsSent` tinyint(1) NOT NULL,
        `ErrorMessage` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Communications` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CommunicationTemplates` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `SubjectTemplate` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `BodyTemplate` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Channel` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CommunicationTemplates` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Documents` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `FileName` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `ContentType` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `FileSize` bigint NOT NULL,
        `EntityType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EntityId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Documents` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `FeatureFlags` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `IsEnabled` tinyint(1) NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_FeatureFlags` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `PayGrades` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Level` int NOT NULL,
        `BasicSalaryBand` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PayGrades` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `PayrollPeriods` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PayrollPeriods` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Permissions` (
        `Code` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Entity` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Action` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Permissions` PRIMARY KEY (`Code`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Roles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Code` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Scope` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `ScheduledReports` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReportName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `CronExpression` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Recipients` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
        `Format` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `LastRunAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ScheduledReports` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `SystemSettings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Key` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Value` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_SystemSettings` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `WebhookSubscriptions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Url` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Secret` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `EventTypes` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_WebhookSubscriptions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Taxpayers` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AccountId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FirstName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `MiddleName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `LastName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `AltPhone` varchar(30) CHARACTER SET utf8mb4 NULL,
        `Gender` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Nin` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Bvn` varchar(50) CHARACTER SET utf8mb4 NULL,
        `TaxId` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(500) CHARACTER SET utf8mb4 NULL,
        `City` varchar(100) CHARACTER SET utf8mb4 NULL,
        `StateId` char(36) COLLATE ascii_general_ci NULL,
        `CountryId` char(36) COLLATE ascii_general_ci NULL,
        `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `IsVerified` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Taxpayers` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Taxpayers_Account_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `DocumentVersions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DocumentId` char(36) COLLATE ascii_general_ci NOT NULL,
        `VersionNumber` int NOT NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `FileSize` bigint NOT NULL,
        `UploadedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DocumentVersions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DocumentVersions_Documents_DocumentId` FOREIGN KEY (`DocumentId`) REFERENCES `Documents` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `RolePermissions` (
        `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PermissionCode` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_RolePermissions` PRIMARY KEY (`RoleId`, `PermissionCode`),
        CONSTRAINT `FK_RolePermissions_Permissions_PermissionCode` FOREIGN KEY (`PermissionCode`) REFERENCES `Permissions` (`Code`) ON DELETE CASCADE,
        CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `WebhookDeliveries` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `SubscriptionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventType` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Payload` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Signature` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `SentAt` datetime(6) NOT NULL,
        `HttpStatusCode` int NULL,
        `ResponsePayload` longtext CHARACTER SET utf8mb4 NULL,
        `AttemptCount` int NOT NULL,
        `IsSuccess` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_WebhookDeliveries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_WebhookDeliveries_WebhookSubscriptions_SubscriptionId` FOREIGN KEY (`SubscriptionId`) REFERENCES `WebhookSubscriptions` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `TaxpayerAddresses` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TaxpayerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AddressLine1` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `AddressLine2` varchar(256) CHARACTER SET utf8mb4 NULL,
        `City` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `State` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PostalCode` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Country` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_TaxpayerAddresses` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TaxpayerAddresses_Taxpayers_TaxpayerId` FOREIGN KEY (`TaxpayerId`) REFERENCES `Taxpayers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `TaxpayerContactDetails` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TaxpayerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PrimaryEmail` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `PrimaryPhone` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `AlternativePhone` varchar(30) CHARACTER SET utf8mb4 NULL,
        `PreferredContactMethod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_TaxpayerContactDetails` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TaxpayerContactDetails_Taxpayers_TaxpayerId` FOREIGN KEY (`TaxpayerId`) REFERENCES `Taxpayers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `AppealGroundPoints` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AppealId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OrderIndex` int NOT NULL,
        `GroundTitle` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `GroundDetail` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `OfficerResponse` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AppealGroundPoints` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Appeals` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Reason` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ReviewedByUserId` char(36) COLLATE ascii_general_ci NULL,
        `ReviewNote` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `ReviewedAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Appeals` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `AppealStatusHistories` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AppealId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OldStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `NewStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ChangedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TransitionedAt` datetime(6) NOT NULL,
        `Reason` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_AppealStatusHistories` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AppealStatusHistories_Appeals_AppealId` FOREIGN KEY (`AppealId`) REFERENCES `Appeals` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Appointments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `StartTime` datetime(6) NOT NULL,
        `EndTime` datetime(6) NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `OfficerId` char(36) COLLATE ascii_general_ci NULL,
        `TaxpayerId` char(36) COLLATE ascii_general_ci NULL,
        `Location` varchar(200) CHARACTER SET utf8mb4 NULL,
        `MeetingUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Appointments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Appointments_Taxpayers_TaxpayerId` FOREIGN KEY (`TaxpayerId`) REFERENCES `Taxpayers` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseCommunicationLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Sender` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `Recipient` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `Direction` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Subject` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Body` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `SentAt` datetime(6) NOT NULL,
        `Channel` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseCommunicationLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseFindings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Description` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseFindings` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseMilestones` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `TargetDate` datetime(6) NULL,
        `CompletedAt` datetime(6) NULL,
        `IsCompleted` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseMilestones` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseNotes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AuthorId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Content` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `IsInternal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseNotes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseRecommendations` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RecommendationText` varchar(4000) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ApprovedByUserId` char(36) COLLATE ascii_general_ci NULL,
        `Notes` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseRecommendations` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Cases` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ComplaintId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Subject` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Summary` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `Priority` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CurrentStage` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AssignedOfficerId` char(36) COLLATE ascii_general_ci NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `AccountId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DueDate` datetime(6) NULL,
        `ClosedAt` datetime(6) NULL,
        `Outcome` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `FindingsSummary` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Cases` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Cases_Account_AccountId` FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `CaseStatusHistories` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OldStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `NewStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ChangedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TransitionedAt` datetime(6) NOT NULL,
        `Reason` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_CaseStatusHistories` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CaseStatusHistories_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `ComplaintLinks` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `SourceComplaintId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TargetComplaintId` char(36) COLLATE ascii_general_ci NOT NULL,
        `LinkType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `LinkedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_ComplaintLinks` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `ComplaintNotes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ComplaintId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AuthorUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Visibility` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ComplaintNotes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Complaints` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReferenceNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Subject` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(5000) CHARACTER SET utf8mb4 NOT NULL,
        `WhyOtoHandle` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `TaxpayerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TaxType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TaxPeriod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ComplaintCategory` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TaxOfficeRef` varchar(100) CHARACTER SET utf8mb4 NULL,
        `TinNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Priority` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CurrentStage` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `AssignedOfficerId` char(36) COLLATE ascii_general_ci NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `RequiresApprovalToClose` tinyint(1) NOT NULL,
        `ClosedAt` datetime(6) NULL,
        `WithdrawalReason` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `ClosureReason` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Complaints` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Complaints_Taxpayers_TaxpayerId` FOREIGN KEY (`TaxpayerId`) REFERENCES `Taxpayers` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `ComplaintStatusHistory` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ComplaintId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OldStatus` int NOT NULL,
        `NewStatus` int NOT NULL,
        `ChangedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TransitionedAt` datetime(6) NOT NULL,
        `Reason` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_ComplaintStatusHistory` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ComplaintStatusHistory_Complaints_ComplaintId` FOREIGN KEY (`ComplaintId`) REFERENCES `Complaints` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Departments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `HeadUserId` char(36) COLLATE ascii_general_ci NULL,
        `RoutingMode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Departments` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Users` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Email` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `Username` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
        `PasswordHash` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
        `FirstName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `LastName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(30) CHARACTER SET utf8mb4 NULL,
        `AltPhone` varchar(30) CHARACTER SET utf8mb4 NULL,
        `JobTitle` varchar(200) CHARACTER SET utf8mb4 NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `EmploymentType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `PayGradeId` char(36) COLLATE ascii_general_ci NULL,
        `Status` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `CanSignIn` tinyint(1) NOT NULL,
        `EmailVerified` tinyint(1) NOT NULL,
        `EmailVerificationToken` longtext CHARACTER SET utf8mb4 NULL,
        `EmailVerificationTokenExpiresAt` datetime(6) NULL,
        `PasswordResetToken` longtext CHARACTER SET utf8mb4 NULL,
        `PasswordResetTokenExpiresAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Users_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `EmployeeWallets` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BalanceNgn` decimal(18,2) NOT NULL,
        `LedgerVersion` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_EmployeeWallets` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EmployeeWallets_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `EwaRequests` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `DisbursedAt` datetime(6) NULL,
        `RecoveredInPeriodId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_EwaRequests` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EwaRequests_PayrollPeriods_RecoveredInPeriodId` FOREIGN KEY (`RecoveredInPeriodId`) REFERENCES `PayrollPeriods` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_EwaRequests_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `LeaveRequests` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `LeaveType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `Days` int NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ApproverUserId` char(36) COLLATE ascii_general_ci NULL,
        `SupervisorNote` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_LeaveRequests` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LeaveRequests_Users_ApproverUserId` FOREIGN KEY (`ApproverUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_LeaveRequests_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `LoanRequests` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `TermMonths` int NOT NULL,
        `Purpose` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ApprovalChain` longtext CHARACTER SET utf8mb4 NULL,
        `RepaymentSchedule` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_LoanRequests` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LoanRequests_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `MfaTokens` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `SecretKey` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsEnabled` tinyint(1) NOT NULL,
        `BackupCodesHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_MfaTokens` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MfaTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Notifications` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Message` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        `ReadAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `OfficerProfiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Specialisation` varchar(100) CHARACTER SET utf8mb4 NULL,
        `MaxCaseload` int NOT NULL,
        `CurrentCaseload` int NOT NULL,
        `IsAvailable` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_OfficerProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OfficerProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Officers` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `MaxCaseload` int NOT NULL,
        `IsAvailable` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Officers` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Officers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `PayrollRuns` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `PeriodId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RunType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TotalGross` decimal(18,2) NOT NULL,
        `TotalNet` decimal(18,2) NOT NULL,
        `TotalStatutory` decimal(18,2) NOT NULL,
        `ApprovedBy` char(36) COLLATE ascii_general_ci NULL,
        `ApprovedAt` datetime(6) NULL,
        `PostedAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PayrollRuns` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PayrollRuns_PayrollPeriods_PeriodId` FOREIGN KEY (`PeriodId`) REFERENCES `PayrollPeriods` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_PayrollRuns_Users_ApprovedBy` FOREIGN KEY (`ApprovedBy`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `RefreshTokens` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Token` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
        `ExpiresAt` datetime(6) NOT NULL,
        `RevokedAt` datetime(6) NULL,
        `ReplacedByToken` varchar(512) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_RefreshTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `SalaryProfiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Basic` decimal(18,2) NOT NULL,
        `Allowances` longtext CHARACTER SET utf8mb4 NULL,
        `Deductions` longtext CHARACTER SET utf8mb4 NULL,
        `EffectiveFrom` datetime(6) NOT NULL,
        `EffectiveTo` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_SalaryProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_SalaryProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `StaffProfiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `HireDate` datetime(6) NOT NULL,
        `EmploymentStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `DateOfBirth` datetime(6) NOT NULL,
        `Nationality` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `MaritalStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EmergencyContact` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `BankAccountNo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `BankId` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `NextOfKin` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_StaffProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StaffProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `TaxpayerProfiles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TaxpayerType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TinNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Nin` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Bvn` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Gender` varchar(20) CHARACTER SET utf8mb4 NULL,
        `DateOfBirth` datetime(6) NULL,
        `CompanyName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `RcNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(500) CHARACTER SET utf8mb4 NULL,
        `City` varchar(100) CHARACTER SET utf8mb4 NULL,
        `State` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsVerified` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_TaxpayerProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TaxpayerProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `UserPermissionOverrides` (
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `PermissionCode` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Mode` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_UserPermissionOverrides` PRIMARY KEY (`UserId`, `PermissionCode`),
        CONSTRAINT `FK_UserPermissionOverrides_Permissions_PermissionCode` FOREIGN KEY (`PermissionCode`) REFERENCES `Permissions` (`Code`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserPermissionOverrides_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `UserRoles` (
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RoleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ScopeQualifier` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_UserRoles` PRIMARY KEY (`UserId`, `RoleId`),
        CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `WalletTransactions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `WalletId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Reference` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_WalletTransactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_WalletTransactions_EmployeeWallets_WalletId` FOREIGN KEY (`WalletId`) REFERENCES `EmployeeWallets` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `OfficerCaseloads` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `OfficerProfileId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseId` char(36) COLLATE ascii_general_ci NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `AssignedAt` datetime(6) NOT NULL,
        `CompletedAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_OfficerCaseloads` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OfficerCaseloads_OfficerProfiles_OfficerProfileId` FOREIGN KEY (`OfficerProfileId`) REFERENCES `OfficerProfiles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `OfficerPerformanceRecords` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `OfficerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Month` datetime(6) NOT NULL,
        `CasesResolved` int NOT NULL,
        `CasesAssigned` int NOT NULL,
        `AverageResolutionTimeDays` decimal(10,2) NOT NULL,
        `CsatScore` decimal(5,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_OfficerPerformanceRecords` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OfficerPerformanceRecords_Officers_OfficerId` FOREIGN KEY (`OfficerId`) REFERENCES `Officers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `PayrollEntries` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `RunId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Basic` decimal(18,2) NOT NULL,
        `Allowances` decimal(18,2) NOT NULL,
        `Deductions` decimal(18,2) NOT NULL,
        `Paye` decimal(18,2) NOT NULL,
        `Pension` decimal(18,2) NOT NULL,
        `Nhf` decimal(18,2) NOT NULL,
        `OtherStatutory` decimal(18,2) NOT NULL,
        `Gross` decimal(18,2) NOT NULL,
        `Net` decimal(18,2) NOT NULL,
        `PaymentStatus` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        CONSTRAINT `PK_PayrollEntries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PayrollEntries_PayrollRuns_RunId` FOREIGN KEY (`RunId`) REFERENCES `PayrollRuns` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PayrollEntries_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE TABLE `Remittances` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `RunId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DeductionType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ReferenceNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Remittances` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Remittances_PayrollRuns_RunId` FOREIGN KEY (`RunId`) REFERENCES `PayrollRuns` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_AppealGroundPoints_AppealId` ON `AppealGroundPoints` (`AppealId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Appeals_CaseId` ON `Appeals` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_AppealStatusHistories_AppealId` ON `AppealStatusHistories` (`AppealId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Appointments_OfficerId` ON `Appointments` (`OfficerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Appointments_TaxpayerId` ON `Appointments` (`TaxpayerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseCommunicationLogs_CaseId` ON `CaseCommunicationLogs` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseFindings_CaseId` ON `CaseFindings` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseMilestones_CaseId` ON `CaseMilestones` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseNotes_AuthorId` ON `CaseNotes` (`AuthorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseNotes_CaseId` ON `CaseNotes` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseRecommendations_CaseId` ON `CaseRecommendations` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Cases_AccountId` ON `Cases` (`AccountId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Cases_AssignedOfficerId` ON `Cases` (`AssignedOfficerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Cases_CaseNumber` ON `Cases` (`CaseNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Cases_ComplaintId` ON `Cases` (`ComplaintId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Cases_DepartmentId` ON `Cases` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_CaseStatusHistories_CaseId` ON `CaseStatusHistories` (`CaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_CommunicationTemplates_Name` ON `CommunicationTemplates` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_ComplaintLinks_SourceComplaintId` ON `ComplaintLinks` (`SourceComplaintId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_ComplaintLinks_TargetComplaintId` ON `ComplaintLinks` (`TargetComplaintId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_ComplaintNotes_ComplaintId` ON `ComplaintNotes` (`ComplaintId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Complaints_AssignedOfficerId` ON `Complaints` (`AssignedOfficerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Complaints_DepartmentId` ON `Complaints` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Complaints_ReferenceNumber` ON `Complaints` (`ReferenceNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Complaints_TaxpayerId` ON `Complaints` (`TaxpayerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_ComplaintStatusHistory_ComplaintId` ON `ComplaintStatusHistory` (`ComplaintId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Departments_HeadUserId` ON `Departments` (`HeadUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_DocumentVersions_DocumentId` ON `DocumentVersions` (`DocumentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_EmployeeWallets_UserId` ON `EmployeeWallets` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_EwaRequests_RecoveredInPeriodId` ON `EwaRequests` (`RecoveredInPeriodId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_EwaRequests_UserId` ON `EwaRequests` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_FeatureFlags_Name` ON `FeatureFlags` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_LeaveRequests_ApproverUserId` ON `LeaveRequests` (`ApproverUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_LeaveRequests_UserId` ON `LeaveRequests` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_LoanRequests_UserId` ON `LoanRequests` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_MfaTokens_UserId` ON `MfaTokens` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Notifications_UserId` ON `Notifications` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_OfficerCaseloads_OfficerProfileId` ON `OfficerCaseloads` (`OfficerProfileId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_OfficerPerformanceRecords_OfficerId` ON `OfficerPerformanceRecords` (`OfficerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_OfficerProfiles_UserId` ON `OfficerProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Officers_UserId` ON `Officers` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_PayrollEntries_RunId` ON `PayrollEntries` (`RunId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_PayrollEntries_UserId` ON `PayrollEntries` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_PayrollRuns_ApprovedBy` ON `PayrollRuns` (`ApprovedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_PayrollRuns_PeriodId` ON `PayrollRuns` (`PeriodId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_RefreshTokens_Token` ON `RefreshTokens` (`Token`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_RefreshTokens_UserId` ON `RefreshTokens` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Remittances_RunId` ON `Remittances` (`RunId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_RolePermissions_PermissionCode` ON `RolePermissions` (`PermissionCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Roles_Name` ON `Roles` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_SalaryProfiles_UserId` ON `SalaryProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_StaffProfiles_UserId` ON `StaffProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_SystemSettings_Key` ON `SystemSettings` (`Key`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_TaxpayerAddresses_TaxpayerId` ON `TaxpayerAddresses` (`TaxpayerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_TaxpayerContactDetails_TaxpayerId` ON `TaxpayerContactDetails` (`TaxpayerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_TaxpayerProfiles_UserId` ON `TaxpayerProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Taxpayers_AccountId` ON `Taxpayers` (`AccountId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Taxpayers_Email` ON `Taxpayers` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_UserPermissionOverrides_PermissionCode` ON `UserPermissionOverrides` (`PermissionCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_UserRoles_RoleId` ON `UserRoles` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_Users_DepartmentId` ON `Users` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_WalletTransactions_WalletId` ON `WalletTransactions` (`WalletId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    CREATE INDEX `IX_WebhookDeliveries_SubscriptionId` ON `WebhookDeliveries` (`SubscriptionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `AppealGroundPoints` ADD CONSTRAINT `FK_AppealGroundPoints_Appeals_AppealId` FOREIGN KEY (`AppealId`) REFERENCES `Appeals` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Appeals` ADD CONSTRAINT `FK_Appeals_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Appointments` ADD CONSTRAINT `FK_Appointments_Officers_OfficerId` FOREIGN KEY (`OfficerId`) REFERENCES `Officers` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseCommunicationLogs` ADD CONSTRAINT `FK_CaseCommunicationLogs_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseFindings` ADD CONSTRAINT `FK_CaseFindings_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseMilestones` ADD CONSTRAINT `FK_CaseMilestones_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseNotes` ADD CONSTRAINT `FK_CaseNotes_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseNotes` ADD CONSTRAINT `FK_CaseNotes_Users_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `CaseRecommendations` ADD CONSTRAINT `FK_CaseRecommendations_Cases_CaseId` FOREIGN KEY (`CaseId`) REFERENCES `Cases` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Cases` ADD CONSTRAINT `FK_Cases_Complaints_ComplaintId` FOREIGN KEY (`ComplaintId`) REFERENCES `Complaints` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Cases` ADD CONSTRAINT `FK_Cases_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Cases` ADD CONSTRAINT `FK_Cases_Officers_AssignedOfficerId` FOREIGN KEY (`AssignedOfficerId`) REFERENCES `Officers` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `ComplaintLinks` ADD CONSTRAINT `FK_ComplaintLinks_Complaints_SourceComplaintId` FOREIGN KEY (`SourceComplaintId`) REFERENCES `Complaints` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `ComplaintLinks` ADD CONSTRAINT `FK_ComplaintLinks_Complaints_TargetComplaintId` FOREIGN KEY (`TargetComplaintId`) REFERENCES `Complaints` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `ComplaintNotes` ADD CONSTRAINT `FK_ComplaintNotes_Complaints_ComplaintId` FOREIGN KEY (`ComplaintId`) REFERENCES `Complaints` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Complaints` ADD CONSTRAINT `FK_Complaints_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Complaints` ADD CONSTRAINT `FK_Complaints_Officers_AssignedOfficerId` FOREIGN KEY (`AssignedOfficerId`) REFERENCES `Officers` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    ALTER TABLE `Departments` ADD CONSTRAINT `FK_Departments_Users_HeadUserId` FOREIGN KEY (`HeadUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611124525_InitialMySql') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260611124525_InitialMySql', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    ALTER TABLE `LeaveRequests` DROP FOREIGN KEY `FK_LeaveRequests_Users_ApproverUserId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    ALTER TABLE `Documents` ADD `Classification` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `Contracts` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `ContractNumber` longtext CHARACTER SET utf8mb4 NULL,
        `Title` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Contracts` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `InventoryItems` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `Name` longtext CHARACTER SET utf8mb4 NULL,
        `SKU` longtext CHARACTER SET utf8mb4 NULL,
        `Quantity` int NOT NULL,
        CONSTRAINT `PK_InventoryItems` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `Invoices` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `InvoiceNumber` longtext CHARACTER SET utf8mb4 NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Invoices` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `NotificationPreferences` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EmailEnabled` tinyint(1) NOT NULL,
        `SmsEnabled` tinyint(1) NOT NULL,
        `InAppEnabled` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_NotificationPreferences` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_NotificationPreferences_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `Projects` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `Name` longtext CHARACTER SET utf8mb4 NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Projects` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `Quotes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `QuoteNumber` longtext CHARACTER SET utf8mb4 NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_Quotes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `VendorContacts` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `Name` longtext CHARACTER SET utf8mb4 NULL,
        `Company` longtext CHARACTER SET utf8mb4 NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_VendorContacts` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `InvoiceItems` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `InvoiceId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Amount` decimal(65,30) NOT NULL,
        CONSTRAINT `PK_InvoiceItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InvoiceItems_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE TABLE `ProjectTasks` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `ProjectId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_ProjectTasks` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProjectTasks_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE INDEX `IX_InvoiceItems_InvoiceId` ON `InvoiceItems` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE INDEX `IX_NotificationPreferences_UserId` ON `NotificationPreferences` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    CREATE INDEX `IX_ProjectTasks_ProjectId` ON `ProjectTasks` (`ProjectId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    ALTER TABLE `LeaveRequests` ADD CONSTRAINT `FK_LeaveRequests_Users_ApproverUserId` FOREIGN KEY (`ApproverUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611225544_Phase1_and_Phase2_OperationsFinance') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260611225544_Phase1_and_Phase2_OperationsFinance', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `LoanRequests` ADD `ActionNote` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `LoanRequests` ADD `DisburseTo` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `LoanRequests` ADD `PayoutReference` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `AccountId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `ContractId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `Currency` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `DiscountAmount` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `DueDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `InvoiceTitle` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `IssuedDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `ParentType` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `Invoices` ADD `TaxAmount` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InvoiceItems` ADD `ItemName` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InvoiceItems` ADD `Quantity` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InvoiceItems` ADD `UnitPrice` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `AssignedUserId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Category` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `DepartmentId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Description` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `ImageUrl` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Location` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Mode` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Note` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `SerialNumber` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    ALTER TABLE `InventoryItems` ADD `Status` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    CREATE TABLE `AgentChats` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Topic` longtext CHARACTER SET utf8mb4 NULL,
        `IsGroupChat` tinyint(1) NOT NULL,
        `ParticipantIds` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AgentChats` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    CREATE TABLE `Announcements` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Message` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Scope` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `TargetRole` longtext CHARACTER SET utf8mb4 NULL,
        `ExpiresAt` datetime(6) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `IsPinned` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Announcements` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    CREATE TABLE `CalendarEvents` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EventType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `StartTime` datetime(6) NOT NULL,
        `EndTime` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `OwnerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `Location` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `IsPublic` tinyint(1) NOT NULL,
        `ReminderMinutes` longtext CHARACTER SET utf8mb4 NULL,
        `AttendeesList` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CalendarEvents` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    CREATE TABLE `AgentChatMessages` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AgentChatId` char(36) COLLATE ascii_general_ci NOT NULL,
        `SenderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsPinned` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AgentChatMessages` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AgentChatMessages_AgentChats_AgentChatId` FOREIGN KEY (`AgentChatId`) REFERENCES `AgentChats` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    CREATE INDEX `IX_AgentChatMessages_AgentChatId` ON `AgentChatMessages` (`AgentChatId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260611233013_Phase4_UI_Gap_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260611233013_Phase4_UI_Gap_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` DROP COLUMN `EmergencyContact`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` DROP COLUMN `NextOfKin`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `AddressLine1` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `AddressLine2` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `City` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `Country` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `EducationDetails` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `EducationLevel` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `EmergencyContactName` varchar(200) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `EmergencyContactPhone` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `EmployeeCode` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `NextOfKinAddress` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `NextOfKinName` varchar(200) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `NextOfKinPhone` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `NextOfKinRelationship` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `State` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `SupervisorId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    ALTER TABLE `StaffProfiles` ADD `Title` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE TABLE `DepartmentMovement` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `StaffProfileId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FromDepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `ToDepartmentId` char(36) COLLATE ascii_general_ci NOT NULL,
        `MovementDate` datetime(6) NOT NULL,
        `Reason` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DepartmentMovement` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DepartmentMovement_StaffProfiles_StaffProfileId` FOREIGN KEY (`StaffProfileId`) REFERENCES `StaffProfiles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE TABLE `StaffDocument` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `StaffProfileId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FileName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FileUrl` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DocumentType` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_StaffDocument` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StaffDocument_StaffProfiles_StaffProfileId` FOREIGN KEY (`StaffProfileId`) REFERENCES `StaffProfiles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE TABLE `StaffNote` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `StaffProfileId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Note` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AddedByUserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_StaffNote` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StaffNote_StaffProfiles_StaffProfileId` FOREIGN KEY (`StaffProfileId`) REFERENCES `StaffProfiles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE INDEX `IX_DepartmentMovement_StaffProfileId` ON `DepartmentMovement` (`StaffProfileId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE INDEX `IX_StaffDocument_StaffProfileId` ON `StaffDocument` (`StaffProfileId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    CREATE INDEX `IX_StaffNote_StaffProfileId` ON `StaffNote` (`StaffProfileId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612093119_Phase5_HR_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612093119_Phase5_HR_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `AnnouncementReadReceipts` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AnnouncementId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReadAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AnnouncementReadReceipts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AnnouncementReadReceipts_Announcements_AnnouncementId` FOREIGN KEY (`AnnouncementId`) REFERENCES `Announcements` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `DashboardWidgets` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ComponentName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RequiredPermission` longtext CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DashboardWidgets` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `MailboxMessages` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `SenderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `BodyText` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsDraft` tinyint(1) NOT NULL,
        `Category` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_MailboxMessages` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `UserDashboardLayouts` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `LayoutJson` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_UserDashboardLayouts` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `MailboxAttachments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `MessageId` char(36) COLLATE ascii_general_ci NOT NULL,
        `FileName` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FileUrl` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FileSize` bigint NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_MailboxAttachments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MailboxAttachments_MailboxMessages_MessageId` FOREIGN KEY (`MessageId`) REFERENCES `MailboxMessages` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE TABLE `MailboxRecipients` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `MessageId` char(36) COLLATE ascii_general_ci NOT NULL,
        `RecipientId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Folder` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        `IsStarred` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_MailboxRecipients` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MailboxRecipients_MailboxMessages_MessageId` FOREIGN KEY (`MessageId`) REFERENCES `MailboxMessages` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE INDEX `IX_AnnouncementReadReceipts_AnnouncementId` ON `AnnouncementReadReceipts` (`AnnouncementId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE INDEX `IX_MailboxAttachments_MessageId` ON `MailboxAttachments` (`MessageId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    CREATE INDEX `IX_MailboxRecipients_MessageId` ON `MailboxRecipients` (`MessageId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612095821_Phase6_Comms_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612095821_Phase6_Comms_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `BenefitTypes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Code` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Category` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AffectsPayroll` tinyint(1) NOT NULL,
        `IsTaxable` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_BenefitTypes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `DisciplinaryCases` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `CaseReference` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `HrOfficerId` char(36) COLLATE ascii_general_ci NULL,
        `IncidentType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IncidentDate` datetime(6) NOT NULL,
        `HearingDate` datetime(6) NULL,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ActionTaken` longtext CHARACTER SET utf8mb4 NULL,
        `Outcome` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsConfidential` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DisciplinaryCases` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `ExitRecords` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ExitType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `NoticeDate` datetime(6) NOT NULL,
        `LastWorkingDate` datetime(6) NULL,
        `ExitDate` datetime(6) NULL,
        `Reason` longtext CHARACTER SET utf8mb4 NOT NULL,
        `HandoverToEmployeeId` char(36) COLLATE ascii_general_ci NULL,
        `HandoverNotes` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ApprovedById` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ExitRecords` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `PerformanceCycles` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PerformanceCycles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `EmployeeBenefits` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `BenefitTypeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NULL,
        `AmountOrValue` decimal(65,30) NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_EmployeeBenefits` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EmployeeBenefits_BenefitTypes_BenefitTypeId` FOREIGN KEY (`BenefitTypeId`) REFERENCES `BenefitTypes` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `PerformanceGoals` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CycleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ProgressPercentage` int NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PerformanceGoals` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PerformanceGoals_PerformanceCycles_CycleId` FOREIGN KEY (`CycleId`) REFERENCES `PerformanceCycles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE TABLE `PerformanceReviews` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReviewerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CycleId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Score` decimal(65,30) NOT NULL,
        `ReviewerNotes` longtext CHARACTER SET utf8mb4 NULL,
        `EmployeeComments` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PerformanceReviews` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PerformanceReviews_PerformanceCycles_CycleId` FOREIGN KEY (`CycleId`) REFERENCES `PerformanceCycles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE INDEX `IX_EmployeeBenefits_BenefitTypeId` ON `EmployeeBenefits` (`BenefitTypeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE INDEX `IX_PerformanceGoals_CycleId` ON `PerformanceGoals` (`CycleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    CREATE INDEX `IX_PerformanceReviews_CycleId` ON `PerformanceReviews` (`CycleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612104220_Phase7_PeopleOps_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612104220_Phase7_PeopleOps_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    CREATE TABLE `AttendanceLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Date` datetime(6) NOT NULL,
        `ClockInTime` time(6) NULL,
        `ClockOutTime` time(6) NULL,
        `WorkedHours` decimal(65,30) NOT NULL,
        `OvertimeHours` decimal(65,30) NOT NULL,
        `LateMinutes` int NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Source` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AttendanceLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    CREATE TABLE `Holidays` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Mode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `AppliesTo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Holidays` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    CREATE TABLE `LeaveTypeEntities` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DefaultDays` int NOT NULL,
        `IsPaid` tinyint(1) NOT NULL,
        `RequiresApproval` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_LeaveTypeEntities` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    CREATE TABLE `LeaveBalances` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmployeeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `LeaveTypeEntityId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Year` int NOT NULL,
        `AllottedDays` int NOT NULL,
        `UsedDays` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_LeaveBalances` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LeaveBalances_LeaveTypeEntities_LeaveTypeEntityId` FOREIGN KEY (`LeaveTypeEntityId`) REFERENCES `LeaveTypeEntities` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    CREATE INDEX `IX_LeaveBalances_LeaveTypeEntityId` ON `LeaveBalances` (`LeaveTypeEntityId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612110505_Phase8_TimeManagement_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612110505_Phase8_TimeManagement_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `SalaryProfiles` ADD `Currency` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `SalaryProfiles` ADD `PayGradeId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `SalaryProfiles` ADD `Status` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Remittances` ADD `EmployeeTotal` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Remittances` ADD `EmployerTotal` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Remittances` ADD `TotalPayable` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `CreatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `Deadline` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `DeletedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `OwnerId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `StartDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `Projects` ADD `UpdatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `PayoutProviders` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ProviderCode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_PayoutProviders` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `ProjectMembers` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ProjectId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ProjectMembers` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ProjectMembers_Projects_ProjectId` FOREIGN KEY (`ProjectId`) REFERENCES `Projects` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `StatutoryDeductions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Code` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Country` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EmployeePercentage` decimal(65,30) NOT NULL,
        `EmployerPercentage` decimal(65,30) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_StatutoryDeductions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `Tickets` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TicketNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `SenderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `SenderDepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `AssignedDepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `DestinationUserId` char(36) COLLATE ascii_general_ci NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Priority` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Tickets` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `Visitors` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        `VisitorCode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `HostId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ExpectedArrival` datetime(6) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RequestedById` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Visitors` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE TABLE `StatutoryRules` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DeductionId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AppliesTo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Basis` longtext CHARACTER SET utf8mb4 NOT NULL,
        `RateOrAmount` decimal(65,30) NOT NULL,
        `EffectiveDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_StatutoryRules` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StatutoryRules_StatutoryDeductions_DeductionId` FOREIGN KEY (`DeductionId`) REFERENCES `StatutoryDeductions` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE INDEX `IX_SalaryProfiles_PayGradeId` ON `SalaryProfiles` (`PayGradeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE INDEX `IX_ProjectMembers_ProjectId` ON `ProjectMembers` (`ProjectId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    CREATE INDEX `IX_StatutoryRules_DeductionId` ON `StatutoryRules` (`DeductionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    ALTER TABLE `SalaryProfiles` ADD CONSTRAINT `FK_SalaryProfiles_PayGrades_PayGradeId` FOREIGN KEY (`PayGradeId`) REFERENCES `PayGrades` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612114512_Phase9_Operations_Expansion') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612114512_Phase9_Operations_Expansion', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `CreatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `Currency` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `DeletedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `DiscountAmount` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `ExpiryDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `IssuedDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `ParentId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `ParentType` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `Subtotal` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `TaxAmount` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `Title` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Quotes` ADD `UpdatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `AssignedAgentId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `CreatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `DeletedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `EndDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `ParentId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `ParentType` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `ReminderCycleDays` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `RenewalDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `SourceQuoteId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `StartDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    ALTER TABLE `Contracts` ADD `UpdatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    CREATE TABLE `ContractReviews` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ContractId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReviewTicketId` char(36) COLLATE ascii_general_ci NULL,
        `ReviewDepartmentId` char(36) COLLATE ascii_general_ci NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ContractReviews` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ContractReviews_Contracts_ContractId` FOREIGN KEY (`ContractId`) REFERENCES `Contracts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    CREATE TABLE `QuoteItems` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `QuoteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Quantity` decimal(65,30) NOT NULL,
        `UnitPrice` decimal(65,30) NOT NULL,
        `LineTotal` decimal(65,30) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_QuoteItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_QuoteItems_Quotes_QuoteId` FOREIGN KEY (`QuoteId`) REFERENCES `Quotes` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    CREATE INDEX `IX_ContractReviews_ContractId` ON `ContractReviews` (`ContractId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    CREATE INDEX `IX_QuoteItems_QuoteId` ON `QuoteItems` (`QuoteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612121009_Phase10_Finance_Quotes_Contracts') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612121009_Phase10_Finance_Quotes_Contracts', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Calls` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CallerType` longtext CHARACTER SET utf8mb4 NULL,
        `CallerMethod` longtext CHARACTER SET utf8mb4 NULL,
        `CallerIdentifier` longtext CHARACTER SET utf8mb4 NULL,
        `CalleeMethod` longtext CHARACTER SET utf8mb4 NULL,
        `CalleeIdentifier` longtext CHARACTER SET utf8mb4 NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `LinkedToId` char(36) COLLATE ascii_general_ci NULL,
        `AgentId` char(36) COLLATE ascii_general_ci NULL,
        `StartAt` datetime(6) NULL,
        `EndAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Calls` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `CaseTasks` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Priority` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DueAt` datetime(6) NULL,
        `AssignedToId` char(36) COLLATE ascii_general_ci NULL,
        `LinkedCaseId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseTasks` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CaseTasks_Cases_LinkedCaseId` FOREIGN KEY (`LinkedCaseId`) REFERENCES `Cases` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Interactions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Channel` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Outcome` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `RelatedToId` char(36) COLLATE ascii_general_ci NULL,
        `LoggedById` char(36) COLLATE ascii_general_ci NULL,
        `OccurredAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Interactions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Organizations` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `PrimaryTaxPayerId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Organizations` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE INDEX `IX_CaseTasks_LinkedCaseId` ON `CaseTasks` (`LinkedCaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143629_Phase11_and_12_Crm_and_Tasks') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612143629_Phase11_and_12_Crm_and_Tasks', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Calls` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CallerType` longtext CHARACTER SET utf8mb4 NULL,
        `CallerMethod` longtext CHARACTER SET utf8mb4 NULL,
        `CallerIdentifier` longtext CHARACTER SET utf8mb4 NULL,
        `CalleeMethod` longtext CHARACTER SET utf8mb4 NULL,
        `CalleeIdentifier` longtext CHARACTER SET utf8mb4 NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `LinkedToId` char(36) COLLATE ascii_general_ci NULL,
        `AgentId` char(36) COLLATE ascii_general_ci NULL,
        `StartAt` datetime(6) NULL,
        `EndAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Calls` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `CaseTasks` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Priority` longtext CHARACTER SET utf8mb4 NOT NULL,
        `DueAt` datetime(6) NULL,
        `AssignedToId` char(36) COLLATE ascii_general_ci NULL,
        `LinkedCaseId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_CaseTasks` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CaseTasks_Cases_LinkedCaseId` FOREIGN KEY (`LinkedCaseId`) REFERENCES `Cases` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Interactions` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Subject` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Channel` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Outcome` longtext CHARACTER SET utf8mb4 NULL,
        `Notes` longtext CHARACTER SET utf8mb4 NULL,
        `RelatedToId` char(36) COLLATE ascii_general_ci NULL,
        `LoggedById` char(36) COLLATE ascii_general_ci NULL,
        `OccurredAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Interactions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE TABLE `Organizations` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Phone` longtext CHARACTER SET utf8mb4 NULL,
        `Email` longtext CHARACTER SET utf8mb4 NULL,
        `PrimaryTaxPayerId` char(36) COLLATE ascii_general_ci NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_Organizations` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    CREATE INDEX `IX_CaseTasks_LinkedCaseId` ON `CaseTasks` (`LinkedCaseId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612143721_Phase11_and_12_Crm_and_Tasks') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612143721_Phase11_and_12_Crm_and_Tasks', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `CreatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `DeletedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Designation` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Scope` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `ScopeTarget` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `UpdatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    CREATE TABLE `SmsMessages` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Provider` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SenderId` longtext CHARACTER SET utf8mb4 NULL,
        `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ScheduledAt` datetime(6) NULL,
        `RecipientType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `PhoneNumbers` longtext CHARACTER SET utf8mb4 NULL,
        `Mode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_SmsMessages` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170439_Phase13_Sms_Vendor') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612170439_Phase13_Sms_Vendor', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `CreatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `DeletedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Designation` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Notes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `Scope` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `ScopeTarget` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    ALTER TABLE `VendorContacts` ADD `UpdatedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    CREATE TABLE `SmsMessages` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Provider` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SenderId` longtext CHARACTER SET utf8mb4 NULL,
        `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ScheduledAt` datetime(6) NULL,
        `RecipientType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `PhoneNumbers` longtext CHARACTER SET utf8mb4 NULL,
        `Mode` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_SmsMessages` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612170446_Phase13_Sms_Vendor') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612170446_Phase13_Sms_Vendor', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612172629_Phase14_PrivateChats') THEN

    CREATE TABLE `AgentChatPreferences` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DoNotDisturb` tinyint(1) NOT NULL,
        `MarkAsAway` tinyint(1) NOT NULL,
        `PlayNotificationSound` tinyint(1) NOT NULL,
        `ShowBrowserNotifications` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AgentChatPreferences` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612172629_Phase14_PrivateChats') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612172629_Phase14_PrivateChats', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612172637_Phase14_PrivateChats') THEN

    CREATE TABLE `AgentChatPreferences` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DoNotDisturb` tinyint(1) NOT NULL,
        `MarkAsAway` tinyint(1) NOT NULL,
        `PlayNotificationSound` tinyint(1) NOT NULL,
        `ShowBrowserNotifications` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_AgentChatPreferences` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612172637_Phase14_PrivateChats') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612172637_Phase14_PrivateChats', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612181941_Phase15_Reports_TimeLogs') THEN

    CREATE TABLE `TimeLogs` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `TaskId` char(36) COLLATE ascii_general_ci NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `StartTime` datetime(6) NOT NULL,
        `EndTime` datetime(6) NOT NULL,
        `DurationHours` double NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_TimeLogs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_TimeLogs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612181941_Phase15_Reports_TimeLogs') THEN

    CREATE INDEX `IX_TimeLogs_UserId` ON `TimeLogs` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612181941_Phase15_Reports_TimeLogs') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612181941_Phase15_Reports_TimeLogs', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260612182603_Phase15_MySql') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260612182603_Phase15_MySql', '9.0.5');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

