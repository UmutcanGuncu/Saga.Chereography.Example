using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItemMessages { get; set; }
}