using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Abstractions.Domain;
using Marketplace.Abstractions.Services;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Orders;
using Nop.Services.Catalog;

namespace Nop.Plugin.Marketplace.VendorExtensions.Services;

public partial class VendorAccountService : IVendorAccountService
{
    private readonly IRepository<VendorCurrentAccount> _accountRepository;
    private readonly IRepository<VendorTransaction> _transactionRepository;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;

    public VendorAccountService(
        IRepository<VendorCurrentAccount> accountRepository,
        IRepository<VendorTransaction> transactionRepository,
        IOrderService orderService,
        IProductService productService)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _orderService = orderService;
        _productService = productService;
    }

    public virtual async Task<VendorCurrentAccount> GetCurrentAccountAsync(int vendorId)
    {
        if (vendorId == 0)
            return null;

        var accounts = await _accountRepository.GetAllAsync(
    query => query.Where(a => a.VendorId == vendorId));
return accounts.FirstOrDefault();
    }

    public virtual async Task<VendorCurrentAccount> GetOrCreateCurrentAccountAsync(int vendorId)
    {
        var account = await GetCurrentAccountAsync(vendorId);
        
        if (account == null)
        {
            account = new VendorCurrentAccount
            {
                VendorId = vendorId,
                Balance = 0,
                TotalCredit = 0,
                TotalDebit = 0,
                LastUpdatedUtc = DateTime.UtcNow
            };
            
            await _accountRepository.InsertAsync(account);
        }

        return account;
    }

    public virtual async Task<decimal> GetBalanceAsync(int vendorId)
    {
        var account = await GetCurrentAccountAsync(vendorId);
        return account?.Balance ?? 0;
    }

    public virtual async Task<VendorTransaction> AddTransactionAsync(
        int vendorId,
        VendorTransactionType type,
        decimal amount,
        int? orderId = null,
        int? orderItemId = null,
        string description = null,
        string referenceNumber = null)
    {
        var account = await GetOrCreateCurrentAccountAsync(vendorId);
        account.Balance += amount;
        
        if (amount > 0)
            account.TotalCredit += amount;
        else
            account.TotalDebit += Math.Abs(amount);
        
        account.LastUpdatedUtc = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account);

        var transaction = new VendorTransaction
        {
            VendorId = vendorId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            Type = type,
            Amount = amount,
            BalanceAfter = account.Balance,
            Description = description,
            ReferenceNumber = referenceNumber,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _transactionRepository.InsertAsync(transaction);
        return transaction;
    }

    public virtual async Task<IList<VendorTransaction>> AddTransactionsAsync(
        int vendorId,
        IList<(VendorTransactionType Type, decimal Amount, int? OrderId, int? OrderItemId, string Description, string ReferenceNumber)> transactions)
    {
        if (!transactions.Any())
            return new List<VendorTransaction>();

        var account = await GetOrCreateCurrentAccountAsync(vendorId);
        var createdTransactions = new List<VendorTransaction>();

        foreach (var txn in transactions)
        {
            account.Balance += txn.Amount;
            
            if (txn.Amount > 0)
                account.TotalCredit += txn.Amount;
            else
                account.TotalDebit += Math.Abs(txn.Amount);

            var transaction = new VendorTransaction
            {
                VendorId = vendorId,
                OrderId = txn.OrderId,
                OrderItemId = txn.OrderItemId,
                Type = txn.Type,
                Amount = txn.Amount,
                BalanceAfter = account.Balance,
                Description = txn.Description,
                ReferenceNumber = txn.ReferenceNumber,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _transactionRepository.InsertAsync(transaction);
            createdTransactions.Add(transaction);
        }

        account.LastUpdatedUtc = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account);

        return createdTransactions;
    }

    public virtual async Task<IPagedList<VendorTransaction>> GetTransactionsAsync(
    int vendorId,
    int? orderId = null,
    VendorTransactionType? type = null,
    int pageIndex = 0,
    int pageSize = int.MaxValue)
{
    var allTransactions = await _transactionRepository.GetAllAsync(
        query =>
        {
            query = query.Where(t => t.VendorId == vendorId);

            if (orderId.HasValue)
                query = query.Where(t => t.OrderId == orderId.Value);

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            return query.OrderByDescending(t => t.CreatedOnUtc);
        });

    // Manual paging
    var pagedList = new PagedList<VendorTransaction>(
        allTransactions.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
        pageIndex,
        pageSize,
        allTransactions.Count);

    return pagedList;
}


    public virtual async Task<VendorTransaction> GetTransactionByIdAsync(int transactionId)
    {
        return await _transactionRepository.GetByIdAsync(transactionId);
    }

public virtual async Task<decimal> GetTotalCreditAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null)
{
    var transactions = await _transactionRepository.GetAllAsync(
        query =>
        {
            query = query.Where(t => t.VendorId == vendorId && t.Amount > 0);

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc <= endDate.Value);

            return query;
        });

    return transactions.Sum(t => t.Amount);
}

   public virtual async Task<decimal> GetTotalDebitAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null)
{
    var transactions = await _transactionRepository.GetAllAsync(
        query =>
        {
            query = query.Where(t => t.VendorId == vendorId && t.Amount < 0);

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc <= endDate.Value);

            return query;
        });

    return Math.Abs(transactions.Sum(t => t.Amount));
}

    public virtual async Task<Dictionary<VendorTransactionType, decimal>> GetDebitBreakdownAsync(
    int vendorId, 
    DateTime? startDate = null, 
    DateTime? endDate = null)
{
    var transactions = await _transactionRepository.GetAllAsync(
        query =>
        {
            query = query.Where(t => t.VendorId == vendorId && t.Amount < 0);

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedOnUtc <= endDate.Value);

            return query;
        });

    var breakdown = transactions
        .GroupBy(t => t.Type)
        .ToDictionary(g => g.Key, g => Math.Abs(g.Sum(t => t.Amount)));

    return breakdown;
}

    public virtual async Task<IList<VendorTransaction>> ProcessOrderFinancialsAsync(int orderId)
    {
        return new List<VendorTransaction>();
    }

    public virtual async Task<VendorTransaction> ProcessRefundAsync(int orderId, decimal refundAmount, bool isPartial = false)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException("Order not found", nameof(orderId));

        var orderItem = (await _orderService.GetOrderItemsAsync(orderId)).FirstOrDefault();
        if (orderItem == null)
            return null;

        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
        if (product?.VendorId == 0)
            return null;

        var description = isPartial 
            ? $"Partial refund - Order #{orderId}" 
            : $"Full refund - Order #{orderId}";

        return await AddTransactionAsync(
            product.VendorId,
            VendorTransactionType.Refund,
            -refundAmount,
            orderId,
            orderItem.Id,
            description,
            order.CustomOrderNumber
        );
    }

    public virtual async Task<IList<VendorTransaction>> ProcessCancellationAsync(int orderId, bool isByVendor)
    {
        var transactions = new List<VendorTransaction>();
        var order = await _orderService.GetOrderByIdAsync(orderId);
        
        if (order == null)
            return transactions;

        var orderItems = await _orderService.GetOrderItemsAsync(orderId);

        foreach (var orderItem in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product?.VendorId == 0)
                continue;

            var revenueTransaction = await AddTransactionAsync(
                product.VendorId,
                VendorTransactionType.ManualAdjustment,
                -orderItem.PriceInclTax,
                orderId,
                orderItem.Id,
                $"Order cancelled - #{orderId}",
                order.CustomOrderNumber
            );

            transactions.Add(revenueTransaction);

            if (isByVendor)
            {
                var penaltyTransaction = await AddTransactionAsync(
                    product.VendorId,
                    VendorTransactionType.CancellationPenalty,
                    -50.00m,
                    orderId,
                    orderItem.Id,
                    $"Vendor cancellation penalty - #{orderId}",
                    order.CustomOrderNumber
                );

                transactions.Add(penaltyTransaction);
            }
        }

        return transactions;
    }

    public virtual async Task<(decimal DeductionAmount, decimal RemainingDebt)> CalculateDebtDeductionAsync(
        int vendorId, 
        decimal upcomingPaymentAmount)
    {
        var balance = await GetBalanceAsync(vendorId);

        if (balance >= 0)
            return (0, 0);

        var debt = Math.Abs(balance);
        var deduction = Math.Min(debt, upcomingPaymentAmount);
        var remaining = debt - deduction;

        return (deduction, remaining);
    }

    public virtual async Task<VendorTransaction> RecordDebtCollectionAsync(
        int vendorId, 
        decimal deductedAmount, 
        string referenceNumber)
    {
        return await AddTransactionAsync(
            vendorId,
            VendorTransactionType.ManualAdjustment,
            deductedAmount,
            null,
            null,
            $"Debt collection from payment",
            referenceNumber
        );
    }
}
