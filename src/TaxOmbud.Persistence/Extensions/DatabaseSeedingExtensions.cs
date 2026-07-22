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

                var schemaUpdates = new[]
                {
                    ("ALTER TABLE Users ADD COLUMN CaldavPassword VARCHAR(256) NULL;", "CaldavPassword in Users"),
                    ("ALTER TABLE EmployeeWallets ADD COLUMN Status VARCHAR(50) NOT NULL DEFAULT 'active';", "Status in EmployeeWallets"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN Status VARCHAR(50) NOT NULL DEFAULT 'pending';", "Status in WalletTransactions"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN ApprovedAt TIMESTAMP NULL;", "ApprovedAt in WalletTransactions"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN PaidAt TIMESTAMP NULL;", "PaidAt in WalletTransactions"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN BankDetail VARCHAR(200) NULL;", "BankDetail in WalletTransactions"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN ProviderRef VARCHAR(100) NULL;", "ProviderRef in WalletTransactions"),
                    ("ALTER TABLE WalletTransactions ADD COLUMN AttemptNumber INT NOT NULL DEFAULT 1;", "AttemptNumber in WalletTransactions"),
                    ("ALTER TABLE LoanRequests ADD COLUMN IsSalaryAdvance BOOLEAN NOT NULL DEFAULT FALSE;", "IsSalaryAdvance in LoanRequests"),
                    ("ALTER TABLE LeaveRequests ADD COLUMN Reason TEXT NULL;", "Reason in LeaveRequests"),
                    ("ALTER TABLE StatutoryDeductions ADD COLUMN IsEmployee BOOLEAN NOT NULL DEFAULT TRUE;", "IsEmployee in StatutoryDeductions"),
                    ("ALTER TABLE StatutoryDeductions ADD COLUMN IsEmployer BOOLEAN NOT NULL DEFAULT FALSE;", "IsEmployer in StatutoryDeductions"),
                    ("ALTER TABLE StatutoryRules ADD COLUMN RateOrAmountStr VARCHAR(100) NULL;", "RateOrAmountStr in StatutoryRules"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN Adapter VARCHAR(50) NOT NULL DEFAULT 'manual';", "Adapter in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN Country VARCHAR(10) NOT NULL DEFAULT 'NG';", "Country in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';", "Currency in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN PublicKey VARCHAR(256) NULL;", "PublicKey in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN SecretKey VARCHAR(256) NULL;", "SecretKey in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN WebhookSecret VARCHAR(256) NULL;", "WebhookSecret in PayoutProviders"),
                    ("ALTER TABLE PayoutProviders ADD COLUMN Notes TEXT NULL;", "Notes in PayoutProviders"),
                    ("ALTER TABLE PayrollPeriods ADD COLUMN Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';", "Currency in PayrollPeriods"),
                    ("ALTER TABLE PayrollRuns ADD COLUMN Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';", "Currency in PayrollRuns"),
                    ("ALTER TABLE PayrollRuns ADD COLUMN EmployeesCount INT NOT NULL DEFAULT 0;", "EmployeesCount in PayrollRuns"),
                    ("ALTER TABLE Remittances ADD COLUMN Reference VARCHAR(100) NULL;", "Reference in Remittances"),
                    ("ALTER TABLE PayGrades ADD COLUMN Currency VARCHAR(10) NOT NULL DEFAULT 'NGN';", "Currency in PayGrades"),
                    ("ALTER TABLE PayGrades ADD COLUMN MinSalary DECIMAL(18,2) NOT NULL DEFAULT 0;", "MinSalary in PayGrades"),
                    ("ALTER TABLE PayGrades ADD COLUMN MaxSalary DECIMAL(18,2) NOT NULL DEFAULT 0;", "MaxSalary in PayGrades"),
                    ("ALTER TABLE PayGrades ADD COLUMN Description TEXT NULL;", "Description in PayGrades"),
                    ("ALTER TABLE Account ADD COLUMN Website VARCHAR(256) NULL;", "Website in Account"),
                    ("ALTER TABLE Account ADD COLUMN AltPhone VARCHAR(50) NULL;", "AltPhone in Account"),
                    ("ALTER TABLE Account ADD COLUMN Address VARCHAR(500) NULL;", "Address in Account"),
                    ("ALTER TABLE Account ADD COLUMN State VARCHAR(100) NULL;", "State in Account"),
                    ("ALTER TABLE Account ADD COLUMN City VARCHAR(100) NULL;", "City in Account"),
                    ("ALTER TABLE Account ADD COLUMN PostalCode VARCHAR(20) NULL;", "PostalCode in Account"),
                    ("ALTER TABLE Account ADD COLUMN Industry VARCHAR(100) NULL;", "Industry in Account"),
                    ("ALTER TABLE TaxpayerProfiles ADD COLUMN AccountId VARCHAR(36) NULL;", "AccountId in TaxpayerProfiles"),
                    ("ALTER TABLE Cases ADD COLUMN CsatRating INT NULL;", "CsatRating in Cases"),
                    ("ALTER TABLE Cases ADD COLUMN NpsScore INT NULL;", "NpsScore in Cases"),
                    ("ALTER TABLE Cases ADD COLUMN CsatComment TEXT NULL;", "CsatComment in Cases")
                };

                foreach (var (sql, desc) in schemaUpdates)
                {
                    try { await context.Database.ExecuteSqlRawAsync(sql); } catch { }
                }

                try { await context.Database.ExecuteSqlRawAsync("CREATE TABLE MailchimpCampaigns (Id VARCHAR(36) PRIMARY KEY, Name VARCHAR(256) NOT NULL, Audience VARCHAR(256) NOT NULL, Subject VARCHAR(256) NOT NULL, Status VARCHAR(50) NOT NULL, UpdatedAt DATETIME NULL, CreatedAt DATETIME NULL);"); } catch { }
                
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE complaints DROP FOREIGN KEY FK_Complaints_Taxpayers_TaxpayerId;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE complaints ADD CONSTRAINT FK_Complaints_TaxpayerProfiles_TaxpayerId FOREIGN KEY (TaxpayerId) REFERENCES taxpayerprofiles (Id) ON DELETE RESTRICT;"); } catch { }

                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE Competencies (Id VARCHAR(36) NOT NULL PRIMARY KEY, Name VARCHAR(200) NOT NULL, Description TEXT NOT NULL, SortOrder INT NOT NULL DEFAULT 1, Status VARCHAR(50) NOT NULL DEFAULT 'Active', CreatedAt DATETIME(6) NOT NULL, LastModifiedAt DATETIME(6) NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedByUserId VARCHAR(36) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0, DeletedAt DATETIME(6) NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE ReviewTemplates (Id VARCHAR(36) NOT NULL PRIMARY KEY, Name VARCHAR(200) NOT NULL, Description TEXT NOT NULL, QuestionCount INT NOT NULL DEFAULT 5, Status VARCHAR(50) NOT NULL DEFAULT 'Active', CreatedAt DATETIME(6) NOT NULL, LastModifiedAt DATETIME(6) NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedByUserId VARCHAR(36) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0, DeletedAt DATETIME(6) NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE ChatbotSessions (Id VARCHAR(36) NOT NULL PRIMARY KEY, VisitorName VARCHAR(256) NOT NULL, VisitorEmail VARCHAR(256) NULL, Platform VARCHAR(100) NOT NULL, Status VARCHAR(50) NOT NULL, Preview VARCHAR(1000) NOT NULL, AssignedAgentId VARCHAR(100) NULL, AssignedAgentName VARCHAR(256) NULL, CreatedAt DATETIME(6) NOT NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedAt DATETIME(6) NULL, LastModifiedByUserId VARCHAR(36) NULL, DeletedAt DATETIME(6) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE ChatbotMessages (Id VARCHAR(36) NOT NULL PRIMARY KEY, SessionId VARCHAR(36) NOT NULL, Sender VARCHAR(50) NOT NULL, Content TEXT NOT NULL, CitationsJson TEXT NULL, IsHandoffTrigger TINYINT(1) NOT NULL DEFAULT 0, CreatedAt DATETIME(6) NOT NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedAt DATETIME(6) NULL, LastModifiedByUserId VARCHAR(36) NULL, DeletedAt DATETIME(6) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0, CONSTRAINT FK_ChatbotMessages_ChatbotSessions_SessionId FOREIGN KEY (SessionId) REFERENCES ChatbotSessions (Id) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE KnowledgeCategories (Id VARCHAR(36) NOT NULL PRIMARY KEY, Name VARCHAR(200) NOT NULL, Slug VARCHAR(250) NOT NULL, Description TEXT NULL, CreatedAt DATETIME(6) NOT NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedAt DATETIME(6) NULL, LastModifiedByUserId VARCHAR(36) NULL, DeletedAt DATETIME(6) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync(@"CREATE TABLE KnowledgeTopics (Id VARCHAR(36) NOT NULL PRIMARY KEY, CategoryId VARCHAR(36) NOT NULL, Title VARCHAR(256) NOT NULL, Body TEXT NOT NULL, TagsJson TEXT NULL, CreatedAt DATETIME(6) NOT NULL, CreatedByUserId VARCHAR(36) NULL, LastModifiedAt DATETIME(6) NULL, LastModifiedByUserId VARCHAR(36) NULL, DeletedAt DATETIME(6) NULL, IsDeleted TINYINT(1) NOT NULL DEFAULT 0, CONSTRAINT FK_KnowledgeTopics_KnowledgeCategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES KnowledgeCategories (Id) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"); } catch { }
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
