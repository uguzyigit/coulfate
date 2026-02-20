namespace Nop.Plugin.Accounting.Parasut.Services.Contact;

public interface IParasutContactService
{
    Task<string> EnsureContactExistsAsync(int vendorId);
    Task<string> CreateContactForVendorAsync(int vendorId);
    Task SyncContactAsync(int vendorId);
}
