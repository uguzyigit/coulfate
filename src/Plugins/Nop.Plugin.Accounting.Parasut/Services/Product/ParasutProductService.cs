using Nop.Data;
using Nop.Plugin.Accounting.Parasut.Domain;
using Nop.Plugin.Accounting.Parasut.Models.Parasut;
using Nop.Plugin.Accounting.Parasut.Services.Api;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Accounting.Parasut.Services.Product;

public class ParasutProductService : IParasutProductService
{
    private readonly IRepository<ParasutProductMapping> _mappingRepository;
    private readonly IParasutApiClient _apiClient;
    private readonly ISettingService _settingService;
    private readonly ILogger _logger;

    public ParasutProductService(
        IRepository<ParasutProductMapping> mappingRepository,
        IParasutApiClient apiClient,
        ISettingService settingService,
        ILogger logger)
    {
        _mappingRepository = mappingRepository;
        _apiClient = apiClient;
        _settingService = settingService;
        _logger = logger;
    }

    public async Task<ProductMappings> EnsureProductsExistAsync()
    {
        var commission = await EnsureProductExistsAsync(ChargeType.Commission, "COMM-001", "Marketplace Commission");
        var shipping = await EnsureProductExistsAsync(ChargeType.Shipping, "SHIP-001", "Shipping Fee");
        var penalty = await EnsureProductExistsAsync(ChargeType.Penalty, "PEN-001", "Penalty Fee");

        return new ProductMappings(commission, shipping, penalty);
    }

    public async Task<string> GetProductIdForChargeTypeAsync(int chargeType)
    {
        var mapping = await _mappingRepository.Table
            .FirstOrDefaultAsync(m => m.ChargeType == chargeType && m.IsActive);

        return mapping?.ParasutProductId;
    }

    private async Task<string> EnsureProductExistsAsync(ChargeType chargeType, string code, string name)
    {
        var mapping = await _mappingRepository.Table
            .FirstOrDefaultAsync(m => m.ChargeType == (int)chargeType && m.IsActive);

        if (mapping != null)
            return mapping.ParasutProductId;

        var settings = await _settingService.LoadSettingAsync<ParasutSettings>();

        var request = new ParasutProductRequest
        {
            Data = new ParasutProductRequest.ProductData
            {
                Attributes = new ParasutProductRequest.ProductAttributes
                {
                    Code = code,
                    Name = name,
                    VatRate = settings.DefaultVatRate,
                    Unit = "Adet",
                    Currency = "TRL",
                    ListPrice = 0,
                    Archived = false,
                    InventoryTracking = false
                }
            }
        };

        var productId = await _apiClient.CreateProductAsync(request);

        var newMapping = new ParasutProductMapping
        {
            ChargeType = (int)chargeType,
            ParasutProductId = productId,
            ProductName = name,
            ProductCode = code,
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _mappingRepository.InsertAsync(newMapping);
        await _logger.InformationAsync($"Created Paraşüt product {productId} for {chargeType}");

        return productId;
    }
}
