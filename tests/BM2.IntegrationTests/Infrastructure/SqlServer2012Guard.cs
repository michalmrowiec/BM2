using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BM2.IntegrationTests.Infrastructure;

/// <summary>
/// Fails any command using T-SQL that SQL Server 2012 (production) lacks. Compatibility level 110 on a newer
/// engine does not reject these functions, so tests would pass while production fails.
/// </summary>
public sealed partial class SqlServer2012Guard : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Check(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Check(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Check(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Check(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Check(command);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbCommand command)
    {
        var match = NewerThan2012().Match(command.CommandText);
        if (match.Success)
        {
            throw new InvalidOperationException(
                $"SQL uses '{match.Value}', which SQL Server 2012 does not support:{Environment.NewLine}{command.CommandText}");
        }
    }

    // SQL Server 2016+: JSON functions, STRING_SPLIT, STRING_AGG, CONCAT_WS, TRANSLATE, TRIM, DATE_BUCKET, GREATEST/LEAST.
    [GeneratedRegex(
        @"\b(JSON_VALUE|JSON_QUERY|JSON_MODIFY|JSON_OBJECT|JSON_ARRAY|ISJSON|OPENJSON|STRING_SPLIT|STRING_AGG|CONCAT_WS|TRANSLATE|TRIM|DATE_BUCKET|GREATEST|LEAST)\s*\(",
        RegexOptions.IgnoreCase)]
    private static partial Regex NewerThan2012();
}
