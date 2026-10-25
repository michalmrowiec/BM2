using System.Globalization;
using System.Text;
using BM2.Contracts.Records;
using BM2.Domain.Common;
using BM2.Server.Data;
using BM2.Server.Features.Wallets;
using BM2.Server.Infrastructure;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BM2.Server.Features.Records;

/// <summary>The list's filtered records as CSV (UTF-8 with BOM, ";") or XLSX, newest first, without paging.</summary>
internal static class ExportRecords
{
    private const int MaxRows = 50_000;

    private static readonly string[] Header =
        ["Date", "Type", "Name", "Category", "Account", "Tags", "Amount", "Currency", "Amount in account currency", "Status"];

    private sealed record Row(
        DateOnly Date, RecordType Type, string? Name, string? Category, string Account, string? TransferAccount,
        string Tags, decimal Amount, string Currency, decimal AccountAmount, RecordStatus Status);

    public static void Map(RouteGroupBuilder records) => records.MapGet("/export", HandleAsync);

    private static async Task<Results<FileContentHttpResult, ProblemHttpResult>> HandleAsync(
        [AsParameters] RecordFilter filter, CurrentWallet current, AppDbContext db, TimeProvider time, CancellationToken ct,
        string format = "csv")
    {
        var xlsx = string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase);
        if (!xlsx && !string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation("format", "Use csv or xlsx.").ToProblem();
        }

        if (RecordQueries.Filtered(db, current, filter) is not { } query)
        {
            return Error.NotFound().ToProblem();
        }

        var walletId = current.Id;
        var tagNames = await db.Tags.Where(t => t.WalletId == walletId).ToDictionaryAsync(t => t.Id, t => t.Name, ct);
        var records = await query
            .OrderByDescending(r => r.OccurredOn).ThenByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .Take(MaxRows)
            .Select(r => new
            {
                r.OccurredOn, r.Type, r.Name, Category = r.Category != null ? r.Category.Name : null, Account = r.Account.Name,
                TransferAccount = r.TransferAccount != null ? r.TransferAccount.Name : null,
                TagIds = r.Tags.Select(t => t.TagId).ToList(), r.Amount, r.Currency, r.AccountAmount, r.Status,
            })
            .ToListAsync(ct);

        // Expenses and transfers leave the account: negative.
        var rows = records.Select(r =>
        {
            var sign = r.Type == RecordType.Income ? 1 : -1;
            var tags = string.Join(", ", r.TagIds.Select(id => tagNames.GetValueOrDefault(id)).Where(n => n is not null));
            return new Row(r.OccurredOn, r.Type, r.Name, r.Category, r.Account, r.TransferAccount, tags,
                sign * r.Amount, r.Currency, sign * r.AccountAmount, r.Status);
        }).ToList();

        var name = $"bm2-records-{DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime):yyyy-MM-dd}";
        return xlsx
            ? TypedResults.File(Xlsx(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name + ".xlsx")
            : TypedResults.File(Csv(rows), "text/csv; charset=utf-8", name + ".csv");
    }

    private static string AccountText(Row row) => row.TransferAccount is null ? row.Account : $"{row.Account} → {row.TransferAccount}";

    private static byte[] Csv(List<Row> rows)
    {
        var csv = new StringBuilder();
        csv.AppendJoin(';', Header).Append("\r\n");
        foreach (var row in rows)
        {
            string[] fields =
            [
                row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                row.Type.ToString(),
                Text(row.Name),
                Text(row.Category),
                Text(AccountText(row)),
                Text(row.Tags),
                row.Amount.ToString("0.00##", CultureInfo.InvariantCulture),
                row.Currency,
                row.AccountAmount.ToString("0.00##", CultureInfo.InvariantCulture),
                row.Status.ToString(),
            ];
            csv.AppendJoin(';', fields).Append("\r\n");
        }

        return [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(csv.ToString())];
    }

    // Quotes when needed; a leading formula character is escaped so spreadsheets do not run it.
    private static string Text(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value[0] is '=' or '+' or '-' or '@')
        {
            value = "'" + value;
        }

        return value.IndexOfAny([';', '"', '\r', '\n']) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }

    private static byte[] Xlsx(List<Row> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Records");
        for (var c = 0; c < Header.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = Header[c];
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var r = i + 2;
            sheet.Cell(r, 1).Value = row.Date.ToDateTime(TimeOnly.MinValue);
            sheet.Cell(r, 2).Value = row.Type.ToString();
            sheet.Cell(r, 3).Value = row.Name;
            sheet.Cell(r, 4).Value = row.Category;
            sheet.Cell(r, 5).Value = AccountText(row);
            sheet.Cell(r, 6).Value = row.Tags;
            sheet.Cell(r, 7).Value = row.Amount;
            sheet.Cell(r, 8).Value = row.Currency;
            sheet.Cell(r, 9).Value = row.AccountAmount;
            sheet.Cell(r, 10).Value = row.Status.ToString();
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Column(1).Style.NumberFormat.Format = "yyyy-mm-dd";
        sheet.Column(7).Style.NumberFormat.Format = "#,##0.00";
        sheet.Column(9).Style.NumberFormat.Format = "#,##0.00";
        int[] widths = [12, 10, 28, 20, 24, 20, 14, 9, 14, 11];
        for (var c = 0; c < widths.Length; c++)
        {
            sheet.Column(c + 1).Width = widths[c];
        }

        sheet.SheetView.FreezeRows(1);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
