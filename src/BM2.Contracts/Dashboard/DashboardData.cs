using BM2.Contracts.Budgets;
using BM2.Contracts.Records;
using BM2.Contracts.Recurring;
using BM2.Contracts.Templates;

namespace BM2.Contracts.Dashboard;

/// <summary>Everything Home shows besides the wallet snapshot, in one call.</summary>
public sealed record DashboardData(
    RecordSummary Month,
    IReadOnlyList<RecordItem> Pending,
    IReadOnlyList<RecordItem> Planned,
    IReadOnlyList<TemplateItem> PinnedTemplates,
    BudgetOverview Budgets,
    IReadOnlyList<RecurringRuleItem> Recurring,
    IReadOnlyList<RecordItem> Recent);
