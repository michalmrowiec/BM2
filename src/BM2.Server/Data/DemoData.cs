using BM2.Contracts.Accounts;
using BM2.Contracts.Categories;
using BM2.Contracts.Records;
using BM2.Contracts.Recurring;
using BM2.Domain.Accounts;
using BM2.Domain.Budgets;
using BM2.Domain.Categories;
using BM2.Domain.Records;
using BM2.Domain.Recurring;
using BM2.Domain.Tags;
using BM2.Domain.Wallets;
using Record = BM2.Domain.Records.Record;

namespace BM2.Server.Data;

/// <summary>
/// Sample wallet of the demo user: about nine months of records up to today. Monthly bills and salary are recurring
/// rules, so opening the wallet (catch-up) adds their records and keeps them current.
/// </summary>
public static class DemoData
{
    public static async Task SeedAsync(AppDbContext db, Guid userId, TimeProvider time, CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var firstMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-8);
        var random = new Random(2026);

        var wallet = Wallet.Create("Demo household", "PLN", userId, now);
        db.Wallets.Add(wallet);
        var categories = DefaultCategories.For(wallet.Id).ToList();
        db.Categories.AddRange(categories);
        Category Cat(string name) => categories.Single(c => c.Name == name);

        var bank = Account.Create(wallet.Id, "Bank account", "PLN", 6_800m, now);
        var cash = Account.Create(wallet.Id, "Cash", "PLN", 250m, now);
        var euro = Account.Create(wallet.Id, "Travel card", "EUR", 120m, now);
        var fund = Account.Create(wallet.Id, "Index fund", "PLN", 0m, now, AccountKind.Investment);
        db.Accounts.AddRange(bank, cash, euro, fund);

        var vacation = Tag.Create(wallet.Id, "Vacation", "teal");
        var kids = Tag.Create(wallet.Id, "Kids", "amber");
        db.Tags.AddRange(vacation, kids);

        db.Budgets.AddRange(
            Budget.Create(wallet.Id, Cat("Groceries").Id, 1_500m),
            Budget.Create(wallet.Id, Cat("Eating out").Id, 450m),
            Budget.Create(wallet.Id, Cat("Entertainment").Id, 250m),
            Budget.Create(wallet.Id, Cat("Shopping").Id, 600m));

        void Rule(string name, RecordType type, Category category, decimal amount, int day)
        {
            var record = new SaveRecordRequest(
                type, RecordStatus.Completed, firstMonth, bank.Id, amount, bank.Currency, amount, category.Id, null, null, name, []);
            var schedule = new RecurrenceSchedule(
                RecurrenceFrequency.Monthly, 1, firstMonth.AddDays(day - 1), RecurrenceEnd.Never, null, null);
            db.RecurringRules.Add(RecurringRule.Create(
                wallet.Id, new SaveRecurringRuleRequest(record, schedule, RecurringMode.Auto, false, null), userId, now, null));
        }

        Rule("Salary", RecordType.Income, Cat("Salary"), 9_400m, 10);
        Rule("Rent", RecordType.Expense, Cat("Home"), 2_700m, 1);
        Rule("Electricity and internet", RecordType.Expense, Cat("Bills"), 330m, 15);
        Rule("Gym", RecordType.Expense, Cat("Health"), 139m, 3);
        Rule("Streaming", RecordType.Expense, Cat("Entertainment"), 49m, 5);

        AccountRef Ref(Account account) => new(account.Id, wallet.Id, account.Currency, false);

        void Add(
            RecordType type, DateOnly date, Account account, decimal amount, Category? category = null, string? name = null,
            Tag? tag = null, Account? to = null, decimal? toAmount = null, RecordStatus status = RecordStatus.Completed)
        {
            if (date > today && status != RecordStatus.Planned)
            {
                return;
            }

            var draft = new RecordDraft(
                type, status, date, account.Id, amount, account.Currency, amount, category?.Id, to?.Id,
                to is null ? null : toAmount ?? amount, name, tag is null ? [] : [tag.Id]);
            var context = new RecordContext(
                wallet.Id, Ref(account), to is null ? null : Ref(to),
                category is null ? null : new CategoryRef(category.Id, wallet.Id, category.Type, false),
                tag is null ? [] : [new TagRef(tag.Id, wallet.Id)]);
            db.Records.Add(Record.Create(draft, context, userId, now).Value);
        }

        decimal Amount(int min, int max) => Math.Round(min + (decimal)random.NextDouble() * (max - min), 2);

        var paidIn = 0m;
        for (var month = firstMonth; month <= today; month = month.AddMonths(1))
        {
            DateOnly Day(int day) => month.AddDays(day - 1);
            var days = DateTime.DaysInMonth(month.Year, month.Month);

            for (var day = 1 + random.Next(3); day <= days; day += 2 + random.Next(3))
            {
                Add(RecordType.Expense, Day(day), bank, Amount(35, 240), Cat("Groceries"), random.Next(3) == 0 ? "Market" : null);
            }

            for (var day = 2 + random.Next(5); day <= days; day += 6 + random.Next(4))
            {
                Add(RecordType.Expense, Day(day), random.Next(3) == 0 ? cash : bank, Amount(28, 150), Cat("Eating out"));
            }

            Add(RecordType.Expense, Day(6), bank, Amount(180, 260), Cat("Transport"), "Fuel");
            Add(RecordType.Expense, Day(21), bank, Amount(170, 250), Cat("Transport"), "Fuel");
            Add(RecordType.Expense, Day(9 + random.Next(10)), bank, Amount(60, 380), Cat("Shopping"), tag: random.Next(2) == 0 ? kids : null);
            Add(RecordType.Expense, Day(17 + random.Next(8)), bank, Amount(30, 120), Cat("Entertainment"), "Cinema");
            Add(RecordType.Transfer, Day(4), bank, 300m, name: "ATM", to: cash);
            Add(RecordType.Expense, Day(13), cash, Amount(20, 90), Cat("Other"));

            if (Day(12) <= today)
            {
                Add(RecordType.Transfer, Day(12), bank, 800m, name: "Monthly investment", to: fund);
                paidIn += 800m;
                var valuedOn = month.AddMonths(1).AddDays(-1) < today ? month.AddMonths(1).AddDays(-1) : today;
                var value = Math.Round(paidIn * (1.01m + (decimal)(month.DayNumber - firstMonth.DayNumber) / 6_000m), 2);
                db.AccountValuations.Add(AccountValuation.Create(fund.Id, valuedOn, value, now));
            }
        }

        // A summer-style trip in EUR four months ago, plus a refund and a planned bill for the forecast.
        var trip = firstMonth.AddMonths(4);
        Add(RecordType.Transfer, trip.AddDays(2), bank, 2_150m, name: "Exchange", to: euro, toAmount: 500m);
        Add(RecordType.Expense, trip.AddDays(5), euro, 280m, Cat("Home"), "Hotel", vacation);
        Add(RecordType.Expense, trip.AddDays(6), euro, 64.5m, Cat("Eating out"), "Trattoria", vacation);
        Add(RecordType.Expense, trip.AddDays(7), euro, 38m, Cat("Transport"), "Train", vacation);
        Add(RecordType.Expense, trip.AddDays(8), euro, 52.9m, Cat("Entertainment"), "Museum", vacation);
        Add(RecordType.Income, today.AddDays(-6), bank, 129.99m, Cat("Shopping"), "Returned shoes");
        Add(RecordType.Expense, today.AddDays(10), bank, 1_150m, Cat("Bills"), "Car insurance", status: RecordStatus.Planned);

        await db.SaveChangesAsync(ct);
    }
}
