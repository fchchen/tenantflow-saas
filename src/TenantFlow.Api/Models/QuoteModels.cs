using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Api.Models;

public sealed record CreateQuoteRequest(
    [param: Required, MaxLength(160)] string CustomerName,
    [param: Range(typeof(decimal), "0.01", "1000000000")] decimal Premium);

public sealed record QuoteResponse(Guid Id, string QuoteNumber, string CustomerName, decimal Premium, DateTime CreatedUtc);
