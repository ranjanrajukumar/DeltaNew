using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Billing students for fee items (billStudents in the Node app's finance/fees.service.js).</summary>
public interface IFeeBilling
{
    /// <summary>Bills the given students for one fee item; a student who already has a fee with the same title is skipped.</summary>
    Task<(int Created, int Skipped)> BillStudentsAsync(IReadOnlyCollection<int> studentIds, string title, decimal amount, DateOnly? dueDate, CancellationToken ct = default);
}
