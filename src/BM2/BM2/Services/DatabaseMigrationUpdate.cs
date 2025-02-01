using Microsoft.EntityFrameworkCore;

namespace BM2.Services
{
    internal static class DatabaseMigrationUpdate
    {
        internal static void UpdateDatabase(this DbContext dbContext, ILogger logger)
        {
            if (!dbContext.Database.IsRelational())
            {
                logger.LogInformation("Skipping migration; non-relational database in use.");
                return;
            }

            const int maxRetryAttempts = 5;
            var retryCount = 0;
            var delay = 1000;

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    var pendingMigrations = dbContext.Database.GetPendingMigrations();

                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Starting database migration. Pending migrations: {Count}", pendingMigrations.Count());

                        dbContext.Database.Migrate();
                        logger.LogInformation("Database migration completed successfully.");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations.");
                    }

                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    logger.LogError(ex, "An error occurred while migrating the database. Attempt {Attempt} of {MaxAttempts}.", retryCount, maxRetryAttempts);

                    if (retryCount >= maxRetryAttempts)
                    {
                        logger.LogError("Maximum retry attempts reached. Throwing exception.");
                        throw;
                    }

                    Thread.Sleep(delay);
                    delay *= 2;
                }
            }
        }
    }
}
