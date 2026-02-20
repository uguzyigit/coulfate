using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Payments.Iyzico.Domain;

namespace Nop.Plugin.Payments.Iyzico.Services;

public interface IIyzicoTransactionService
{
    Task<IyzicoPaymentTransaction?> GetByIdAsync(int id);
    Task<IyzicoPaymentTransaction?> GetByPaymentTransactionIdAsync(string paymentTransactionId);
    Task<IList<IyzicoPaymentTransaction>> GetByOrderIdAsync(int orderId);
    Task<IList<IyzicoPaymentTransaction>> GetByOrderItemIdAsync(int orderItemId);
    Task<bool> IsOrderApprovedAsync(int orderId);
    Task InsertAsync(IyzicoPaymentTransaction transaction);
    Task UpdateAsync(IyzicoPaymentTransaction transaction);
    Task DeleteAsync(IyzicoPaymentTransaction transaction);
    Task<IPagedList<IyzicoPaymentTransaction>> SearchTransactionsAsync(
        int? orderId = null,
        int? vendorId = null,
        bool? isApproved = null,
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);
}

public class IyzicoTransactionService : IIyzicoTransactionService
{
    private readonly IRepository<IyzicoPaymentTransaction> _transactionRepository;

    public IyzicoTransactionService(IRepository<IyzicoPaymentTransaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IyzicoPaymentTransaction?> GetByIdAsync(int id)
    {
        return await _transactionRepository.GetByIdAsync(id);
    }

    public async Task<IyzicoPaymentTransaction?> GetByPaymentTransactionIdAsync(string paymentTransactionId)
    {
        if (string.IsNullOrEmpty(paymentTransactionId))
            return null;

        var query = _transactionRepository.Table
            .Where(t => t.PaymentTransactionId == paymentTransactionId);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<IyzicoPaymentTransaction>> GetByOrderIdAsync(int orderId)
    {
        if (orderId == 0)
            return new List<IyzicoPaymentTransaction>();

        var query = _transactionRepository.Table
            .Where(t => t.OrderId == orderId)
            .OrderBy(t => t.CreatedOnUtc);

        return await query.ToListAsync();
    }

    public async Task<IList<IyzicoPaymentTransaction>> GetByOrderItemIdAsync(int orderItemId)
    {
        if (orderItemId == 0)
            return new List<IyzicoPaymentTransaction>();

        var query = _transactionRepository.Table
            .Where(t => t.OrderItemId == orderItemId);

        return await query.ToListAsync();
    }

    public async Task<bool> IsOrderApprovedAsync(int orderId)
    {
        if (orderId == 0)
            return false;

        var transactions = await GetByOrderIdAsync(orderId);
        return transactions.Any() && transactions.All(t => t.IsApproved);
    }

    public async Task InsertAsync(IyzicoPaymentTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await _transactionRepository.InsertAsync(transaction);
    }

    public async Task UpdateAsync(IyzicoPaymentTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await _transactionRepository.UpdateAsync(transaction);
    }

    public async Task DeleteAsync(IyzicoPaymentTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await _transactionRepository.DeleteAsync(transaction);
    }

    public async Task<IPagedList<IyzicoPaymentTransaction>> SearchTransactionsAsync(
        int? orderId = null,
        int? vendorId = null,
        bool? isApproved = null,
        DateTime? createdFromUtc = null,
        DateTime? createdToUtc = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = _transactionRepository.Table;

        if (orderId.HasValue && orderId.Value > 0)
            query = query.Where(t => t.OrderId == orderId.Value);

        if (vendorId.HasValue && vendorId.Value > 0)
            query = query.Where(t => t.VendorId == vendorId.Value);

        if (isApproved.HasValue)
            query = query.Where(t => t.IsApproved == isApproved.Value);

        if (createdFromUtc.HasValue)
            query = query.Where(t => t.CreatedOnUtc >= createdFromUtc.Value);

        if (createdToUtc.HasValue)
            query = query.Where(t => t.CreatedOnUtc <= createdToUtc.Value);

        query = query.OrderByDescending(t => t.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }
}