using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Marketplace.Commission.Infrastructure.EventConsumers;

public class OrderUpdatedDebugConsumer : IConsumer<EntityUpdatedEvent<Order>>
{
    private readonly ILogger _logger;
    
    public OrderUpdatedDebugConsumer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
    {
        var message = $"[DEBUG] Order updated: {eventMessage.Entity.Id}, PaymentStatus={eventMessage.Entity.PaymentStatusId}";
        
        // Console'a yaz
        Console.WriteLine("==========================================");
        Console.WriteLine(message);
        Console.WriteLine("==========================================");
        
        // Log'a da yaz
        await _logger.InformationAsync(message);
    }
}