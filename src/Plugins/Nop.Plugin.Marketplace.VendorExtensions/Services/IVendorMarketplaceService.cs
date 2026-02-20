using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Marketplace.VendorExtensions.Domain;

namespace Nop.Plugin.Marketplace.VendorExtensions.Services;

/// <summary>
/// Vendor marketplace service interface
/// </summary>
public partial interface IVendorMarketplaceService
{
    #region Vendor Settings

    /// <summary>
    /// Gets vendor marketplace settings by vendor identifier
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <returns>Vendor marketplace settings</returns>
    Task<VendorMarketplaceSettings> GetVendorSettingsByVendorIdAsync(int vendorId);

    /// <summary>
    /// Gets vendor marketplace settings by code
    /// </summary>
    /// <param name="code">Vendor code</param>
    /// <returns>Vendor marketplace settings</returns>
    Task<VendorMarketplaceSettings> GetVendorSettingsByCodeAsync(string code);

    /// <summary>
    /// Inserts vendor marketplace settings
    /// </summary>
    /// <param name="settings">Vendor marketplace settings</param>
    Task InsertVendorSettingsAsync(VendorMarketplaceSettings settings);

    /// <summary>
    /// Updates vendor marketplace settings
    /// </summary>
    /// <param name="settings">Vendor marketplace settings</param>
    Task UpdateVendorSettingsAsync(VendorMarketplaceSettings settings);

    /// <summary>
    /// Deletes vendor marketplace settings
    /// </summary>
    /// <param name="settings">Vendor marketplace settings</param>
    Task DeleteVendorSettingsAsync(VendorMarketplaceSettings settings);

    #endregion

    #region Vendor Balance

    /// <summary>
    /// Gets vendor balance by vendor identifier
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <returns>Vendor balance</returns>
    Task<VendorBalance> GetVendorBalanceAsync(int vendorId);

    /// <summary>
    /// Creates or updates vendor balance
    /// </summary>
    /// <param name="balance">Vendor balance</param>
    Task SaveVendorBalanceAsync(VendorBalance balance);

    /// <summary>
    /// Updates vendor balance (adds to current balance)
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="amount">Amount to add</param>
    /// <param name="isPending">Whether amount is pending</param>
    Task UpdateVendorBalanceAsync(int vendorId, decimal amount, bool isPending = false);

    #endregion

    #region Vendor Payouts

    /// <summary>
    /// Gets vendor payout by identifier
    /// </summary>
    /// <param name="payoutId">Payout identifier</param>
    /// <returns>Vendor payout</returns>
    Task<VendorPayout> GetVendorPayoutByIdAsync(int payoutId);

    /// <summary>
    /// Gets all vendor payouts
    /// </summary>
    /// <param name="vendorId">Vendor identifier; null to load all</param>
    /// <param name="status">Payout status; null to load all</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Vendor payouts</returns>
    Task<IPagedList<VendorPayout>> GetAllVendorPayoutsAsync(
        int? vendorId = null,
        VendorPayoutStatus? status = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a vendor payout
    /// </summary>
    /// <param name="payout">Vendor payout</param>
    Task InsertVendorPayoutAsync(VendorPayout payout);

    /// <summary>
    /// Updates a vendor payout
    /// </summary>
    /// <param name="payout">Vendor payout</param>
    Task UpdateVendorPayoutAsync(VendorPayout payout);

    /// <summary>
    /// Deletes a vendor payout
    /// </summary>
    /// <param name="payout">Vendor payout</param>
    Task DeleteVendorPayoutAsync(VendorPayout payout);

    /// <summary>
    /// Process payout request
    /// </summary>
    /// <param name="vendorId">Vendor identifier</param>
    /// <param name="amount">Amount to payout</param>
    /// <param name="paymentMethod">Payment method</param>
    /// <returns>Created payout</returns>
    Task<VendorPayout> RequestPayoutAsync(int vendorId, decimal amount, string paymentMethod);

    /// <summary>
    /// Approve payout
    /// </summary>
    /// <param name="payoutId">Payout identifier</param>
    Task ApprovePayoutAsync(int payoutId);

    /// <summary>
    /// Complete payout (mark as processed)
    /// </summary>
    /// <param name="payoutId">Payout identifier</param>
    /// <param name="externalReference">External transaction reference</param>
    Task CompletePayoutAsync(int payoutId, string externalReference = null);

    #endregion

    #region Order Commissions

    /// <summary>
    /// Gets vendor order commission by identifier
    /// </summary>
    /// <param name="commissionId">Commission identifier</param>
    /// <returns>Vendor order commission</returns>
    Task<VendorOrderCommission> GetVendorOrderCommissionByIdAsync(int commissionId);

    /// <summary>
    /// Gets all vendor order commissions
    /// </summary>
    /// <param name="vendorId">Vendor identifier; null to load all</param>
    /// <param name="orderId">Order identifier; null to load all</param>
    /// <param name="isPaid">Paid status; null to load all</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Vendor order commissions</returns>
    Task<IPagedList<VendorOrderCommission>> GetAllVendorOrderCommissionsAsync(
        int? vendorId = null,
        int? orderId = null,
        bool? isPaid = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue);

    /// <summary>
    /// Inserts a vendor order commission
    /// </summary>
    /// <param name="commission">Vendor order commission</param>
    Task InsertVendorOrderCommissionAsync(VendorOrderCommission commission);

    /// <summary>
    /// Updates a vendor order commission
    /// </summary>
    /// <param name="commission">Vendor order commission</param>
    Task UpdateVendorOrderCommissionAsync(VendorOrderCommission commission);

    /// <summary>
    /// Calculate and create commission for order item
    /// </summary>
    /// <param name="orderItemId">Order item identifier</param>
    Task ProcessOrderItemCommissionAsync(int orderItemId);

    #endregion
}
