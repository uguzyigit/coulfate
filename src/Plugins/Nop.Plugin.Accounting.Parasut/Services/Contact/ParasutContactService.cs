using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Parasut;
using Nop.Plugin.Accounting.Parasut.Services.Api;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Vendors;
using Nop.Services.Common;

namespace Nop.Plugin.Accounting.Parasut.Services.Contact;

public class ParasutContactService : IParasutContactService
{
    private readonly IRepository<VendorParasutMapping> _mappingRepository;
    private readonly IVendorService _vendorService;
    private readonly IParasutApiClient _apiClient;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAddressService _addressService;
    private readonly ILogger _logger;

    public ParasutContactService(
        IRepository<VendorParasutMapping> mappingRepository,
        IVendorService vendorService,
        IParasutApiClient apiClient,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IAddressService addressService,
        ILogger logger)
    {
        _mappingRepository = mappingRepository;
        _vendorService = vendorService;
        _apiClient = apiClient;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _addressService = addressService;
        _logger = logger;
    }

    public async Task<string> EnsureContactExistsAsync(int vendorId)
    {
        var mapping = await _mappingRepository.Table
            .FirstOrDefaultAsync(m => m.VendorId == vendorId && m.IsActive);

        if (mapping != null)
            return mapping.ParasutContactId;

        return await CreateContactForVendorAsync(vendorId);
    }

    public async Task<string> CreateContactForVendorAsync(int vendorId)
    {
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null)
            throw new Exception($"Vendor {vendorId} not found");

        // Get vendor address from Address table
        var vendorAddressId = vendor.AddressId;
        var address = vendorAddressId > 0 
            ? await _addressService.GetAddressByIdAsync(vendorAddressId) 
            : null;
        
        var request = new ParasutContactRequest
        {
            Data = new ParasutContactRequest.ContactData
            {
                Attributes = new ParasutContactRequest.ContactAttributes
                {
                    Email = vendor.Email,
                    Name = vendor.Name,
                    ShortName = vendor.Name?.Length > 50 ? vendor.Name.Substring(0, 50) : vendor.Name,
                    ContactType = "company",
                    TaxOffice = address?.Company ?? "Unknown",
                    TaxNumber = vendor.Email?.GetHashCode().ToString() ?? "0000000000",
                    City = address?.City ?? "Istanbul",
                    District = address?.County ?? "",
                    Address = address?.Address1 ?? "",
                    Phone = address?.PhoneNumber ?? ""
                }
            }
        };

        var contactId = await _apiClient.CreateContactAsync(request);

        var mapping = new VendorParasutMapping
        {
            VendorId = vendorId,
            ParasutContactId = contactId,
            ContactName = vendor.Name,
            TaxNumber = request.Data.Attributes.TaxNumber,
            Email = vendor.Email,
            IsActive = true,
            LastSyncedOnUtc = DateTime.UtcNow,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _mappingRepository.InsertAsync(mapping);
        await _logger.InformationAsync($"Created Paraşüt contact {contactId} for vendor {vendorId}");

        return contactId;
    }

    public async Task SyncContactAsync(int vendorId)
    {
        var mapping = await _mappingRepository.Table
            .FirstOrDefaultAsync(m => m.VendorId == vendorId);

        if (mapping == null)
            return;

        mapping.LastSyncedOnUtc = DateTime.UtcNow;
        await _mappingRepository.UpdateAsync(mapping);
    }
}