using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Persistence.Data;

namespace TaxOmbud.Persistence.Extensions;

/// <summary>
/// Extension to apply pending migrations and seed reference data on startup.
/// Call app.SeedDatabaseAsync() in Program.cs after building the app.
/// </summary>
public static class DatabaseSeedingExtensions
{
    public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context     = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var logger      = services.GetRequiredService<ILogger<DataSeeder>>();

            var dbProvider = context.Database.ProviderName;
            if (dbProvider != null && dbProvider.Contains("MySql"))
            {
                logger.LogInformation("Applying MySQL migrations...");
                await context.Database.MigrateAsync();
                try
                {
                    logger.LogInformation("Ensuring CaldavPassword column exists in Users table...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Users ADD COLUMN IF NOT EXISTS CaldavPassword VARCHAR(256) NULL;");

                    logger.LogInformation("Ensuring finance schema fields exist in MySQL tables...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE EmployeeWallets ADD COLUMN IF NOT EXISTS Status VARCHAR(50) NOT NULL DEFAULT 'active';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS Status VARCHAR(50) NOT NULL DEFAULT 'pending';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS ApprovedAt TIMESTAMP NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS PaidAt TIMESTAMP NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS BankDetail VARCHAR(200) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS ProviderRef VARCHAR(100) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE WalletTransactions ADD COLUMN IF NOT EXISTS AttemptNumber INT NOT NULL DEFAULT 1;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE LoanRequests ADD COLUMN IF NOT EXISTS IsSalaryAdvance BOOLEAN NOT NULL DEFAULT FALSE;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE LeaveRequests ADD COLUMN IF NOT EXISTS Reason TEXT NULL;");

                    logger.LogInformation("Ensuring payroll schema fields exist in MySQL tables...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE StatutoryDeductions ADD COLUMN IF NOT EXISTS IsEmployee BOOLEAN NOT NULL DEFAULT TRUE;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE StatutoryDeductions ADD COLUMN IF NOT EXISTS IsEmployer BOOLEAN NOT NULL DEFAULT FALSE;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE StatutoryRules ADD COLUMN IF NOT EXISTS RateOrAmountStr VARCHAR(100) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS Adapter VARCHAR(50) NOT NULL DEFAULT 'manual';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS Country VARCHAR(10) NOT NULL DEFAULT 'NG';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS PublicKey VARCHAR(256) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS SecretKey VARCHAR(256) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS WebhookSecret VARCHAR(256) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayoutProviders ADD COLUMN IF NOT EXISTS Notes TEXT NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayrollPeriods ADD COLUMN IF NOT EXISTS Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayrollRuns ADD COLUMN IF NOT EXISTS Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayrollRuns ADD COLUMN IF NOT EXISTS EmployeesCount INT NOT NULL DEFAULT 0;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Remittances ADD COLUMN IF NOT EXISTS Reference VARCHAR(100) NULL;");

                    logger.LogInformation("Ensuring pay grade extensions exist in MySQL tables...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayGrades ADD COLUMN IF NOT EXISTS Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayGrades ADD COLUMN IF NOT EXISTS MinSalary DECIMAL(18,2) NOT NULL DEFAULT 0;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayGrades ADD COLUMN IF NOT EXISTS MaxSalary DECIMAL(18,2) NOT NULL DEFAULT 0;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE PayGrades ADD COLUMN IF NOT EXISTS Description TEXT NULL;");

                    logger.LogInformation("Ensuring Account extensions exist in MySQL tables...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS Website VARCHAR(256) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS AltPhone VARCHAR(50) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS Address VARCHAR(500) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS State VARCHAR(100) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS City VARCHAR(100) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS PostalCode VARCHAR(20) NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Account ADD COLUMN IF NOT EXISTS Industry VARCHAR(100) NULL;");

                    logger.LogInformation("Ensuring AccountId extension exists in TaxpayerProfiles table...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE TaxpayerProfiles ADD COLUMN IF NOT EXISTS AccountId VARCHAR(36) NULL;");

                    logger.LogInformation("Ensuring CSAT columns exist in Cases table...");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Cases ADD COLUMN IF NOT EXISTS CsatRating INT NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Cases ADD COLUMN IF NOT EXISTS NpsScore INT NULL;");
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE Cases ADD COLUMN IF NOT EXISTS CsatComment TEXT NULL;");

                    logger.LogInformation("Ensuring MailchimpCampaigns table exists...");
                    await context.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS MailchimpCampaigns (Id VARCHAR(36) PRIMARY KEY, Name VARCHAR(256) NOT NULL, Audience VARCHAR(256) NOT NULL, Subject VARCHAR(256) NOT NULL, Status VARCHAR(50) NOT NULL, UpdatedAt DATETIME NULL, CreatedAt DATETIME NULL);");

                    logger.LogInformation("Ensuring Complaints table taxpayer foreign key points to TaxpayerProfiles...");
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE complaints DROP FOREIGN KEY FK_Complaints_Taxpayers_TaxpayerId;");
                    }
                    catch (Exception) { }

                    try
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE complaints ADD CONSTRAINT FK_Complaints_TaxpayerProfiles_TaxpayerId FOREIGN KEY (TaxpayerId) REFERENCES taxpayerprofiles (Id) ON DELETE RESTRICT;");
                    }
                    catch (Exception) { }

                    logger.LogInformation("Ensuring performance competency and template tables exist in MySQL...");
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS Competencies (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            Name VARCHAR(200) NOT NULL,
                            Description TEXT NOT NULL,
                            SortOrder INT NOT NULL DEFAULT 1,
                            Status VARCHAR(50) NOT NULL DEFAULT 'Active',
                            CreatedAt DATETIME(6) NOT NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
                            DeletedAt DATETIME(6) NULL
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ReviewTemplates (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            Name VARCHAR(200) NOT NULL,
                            Description TEXT NOT NULL,
                            QuestionCount INT NOT NULL DEFAULT 5,
                            Status VARCHAR(50) NOT NULL DEFAULT 'Active',
                            CreatedAt DATETIME(6) NOT NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
                            DeletedAt DATETIME(6) NULL
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                    logger.LogInformation("Ensuring ChatbotSessions and ChatbotMessages tables exist in MySQL...");
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ChatbotSessions (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            VisitorName VARCHAR(256) NOT NULL,
                            VisitorEmail VARCHAR(256) NULL,
                            Platform VARCHAR(100) NOT NULL,
                            Status VARCHAR(50) NOT NULL,
                            Preview VARCHAR(1000) NOT NULL,
                            AssignedAgentId VARCHAR(100) NULL,
                            AssignedAgentName VARCHAR(256) NULL,
                            CreatedAt DATETIME(6) NOT NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            DeletedAt DATETIME(6) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ChatbotMessages (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            SessionId VARCHAR(36) NOT NULL,
                            Sender VARCHAR(50) NOT NULL,
                            Content TEXT NOT NULL,
                            CitationsJson TEXT NULL,
                            IsHandoffTrigger TINYINT(1) NOT NULL DEFAULT 0,
                            CreatedAt DATETIME(6) NOT NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            DeletedAt DATETIME(6) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
                            CONSTRAINT FK_ChatbotMessages_ChatbotSessions_SessionId FOREIGN KEY (SessionId) REFERENCES ChatbotSessions (Id) ON DELETE CASCADE
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                    logger.LogInformation("Ensuring KnowledgeCategories and KnowledgeTopics tables exist in MySQL...");
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS KnowledgeCategories (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            Name VARCHAR(200) NOT NULL,
                            Slug VARCHAR(250) NOT NULL,
                            Description TEXT NULL,
                            CreatedAt DATETIME(6) NOT NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            DeletedAt DATETIME(6) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS KnowledgeTopics (
                            Id VARCHAR(36) NOT NULL PRIMARY KEY,
                            CategoryId VARCHAR(36) NOT NULL,
                            Title VARCHAR(256) NOT NULL,
                            Body TEXT NOT NULL,
                            TagsJson TEXT NULL,
                            CreatedAt DATETIME(6) NOT NULL,
                            CreatedByUserId VARCHAR(36) NULL,
                            LastModifiedAt DATETIME(6) NULL,
                            LastModifiedByUserId VARCHAR(36) NULL,
                            DeletedAt DATETIME(6) NULL,
                            IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
                            CONSTRAINT FK_KnowledgeTopics_KnowledgeCategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES KnowledgeCategories (Id) ON DELETE CASCADE
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to run alter column query. It may already exist.");
                }
            }
            else
            {
                logger.LogInformation("Applying pending EF migrations...");
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Starting database seeding...");
            var seeder = new DataSeeder(context, userManager, logger);
            await seeder.SeedAllAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<DataSeeder>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw; // Re-throw to prevent app from starting with incomplete seed
        }

        return app;
    }
}
