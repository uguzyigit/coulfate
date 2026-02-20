using Nop.Plugin.Marketplace.Commission.Domain;

namespace Nop.Plugin.Marketplace.Commission.Events;

/// <summary>
/// Event published when an order commission is created
/// </summary>
public class OrderCommissionCreatedEvent
{
    public OrderCommission OrderCommission { get; set; }

    public OrderCommissionCreatedEvent(OrderCommission orderCommission)
    {
        OrderCommission = orderCommission;
    }
}
