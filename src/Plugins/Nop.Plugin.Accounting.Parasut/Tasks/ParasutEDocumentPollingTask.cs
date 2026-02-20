using Nop.Services.ScheduleTasks;
using Nop.Plugin.Accounting.Parasut.Services.Invoice;

namespace Nop.Plugin.Accounting.Parasut.Tasks;

public class ParasutEDocumentPollingTask : IScheduleTask
{
    private readonly IParasutInvoiceService _invoiceService;

    public ParasutEDocumentPollingTask(IParasutInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task ExecuteAsync()
    {
        await _invoiceService.ProcessPendingEDocumentsAsync();
    }
}
