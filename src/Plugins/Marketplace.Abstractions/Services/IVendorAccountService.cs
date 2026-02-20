using Marketplace.Abstractions.Domain;
using Nop.Core;

namespace Marketplace.Abstractions.Services;

/// <summary>
/// Vendor current account service interface
/// Manages vendor's financial transactions and balance
/// </summary>
public interface IVendorAccountService
{
    #region Current Account

    /// <summary>
    /// Gets vendor current account by vendor ID
    /// </summary>
    Task<VendorCurrentAccount> GetCurrentAccountAsync(int vendorId);

    /// <summary>
    /// Gets vendor current account by vendor ID, creates if doesn't exist
    /// </summary>
    Task<VendorCurrentAccount> GetOrCreateCurrentAccountAsync(int vendorId);

    /// <summary>
    /// Gets vendor balance
    /// </summary>
    Task<decimal> GetBalanceAsync(int vendorId);

    #endregion

    #region Transactions

    /// <summary>
    /// Adds a single transaction to vendor account
    /// </summary>
    Task<VendorTransaction> AddTransactionAsync(
        int vendorId,
        VendorTransactionType type,
        decimal amount,
        int? orderId = null,
        int? orderItemId = null,
        string description = null,
        string referenceNumber = null);

    /// <summary>
    /// Adds multiple transactions to vendor account (atomic operation)
    /// </summary>
    Task<IList<VendorTransaction>> AddTransactionsAsync(
        int vendorId,
        IList<(VendorTransactionType Type, decimal Amount, int? OrderId, int? OrderItemId, string Description, string ReferenceNumber)> transactions);

    /// <summary>
    /// Gets vendor transactions
    /// </summary>
    Task<IPagedList<VendorTransaction>> GetTransactionsAsync(
        int vendorId,
        int? orderId = null,
        VendorTransactionType? type = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Gets transaction by ID
    /// </summary>
    Task<VendorTransaction> GetTransactionByIdAsync(int transactionId);

    #endregion

    #region Balance Calculations

    /// <summary>
    /// Calculates total credit (earnings) for a period
    /// </summary>
    Task<decimal> GetTotalCreditAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Calculates total debit (costs) for a period
    /// </summary>
    Task<decimal> GetTotalDebitAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets breakdown of debits by type
    /// </summary>
    Task<Dictionary<VendorTransactionType, decimal>> GetDebitBreakdownAsync(
        int vendorId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    #endregion

    #region Order-specific Operations

    /// <summary>
    /// Processes order financial transactions
    /// </summary>
    Task<IList<VendorTransaction>> ProcessOrderFinancialsAsync(int orderId);

    /// <summary>
    /// Processes refund transactions
    /// </summary>
    Task<VendorTransaction> ProcessRefundAsync(int orderId, decimal refundAmount, bool isPartial = false);

    /// <summary>
    /// Processes cancellation
    /// </summary>
    Task<IList<VendorTransaction>> ProcessCancellationAsync(int orderId, bool isByVendor);

    #endregion

    #region Debt Collection

    /// <summary>
    /// Calculates how much to deduct from next payment if vendor is in debt
    /// </summary>
    Task<(decimal DeductionAmount, decimal RemainingDebt)> CalculateDebtDeductionAsync(
        int vendorId,
        decimal upcomingPaymentAmount);

    /// <summary>
    /// Records debt collection from a payment
    /// </summary>
    Task<VendorTransaction> RecordDebtCollectionAsync(
        int vendorId,
        decimal deductedAmount,
        string referenceNumber);

    #endregion
}
