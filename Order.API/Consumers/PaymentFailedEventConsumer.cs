using MassTransit;
using Order.API.Contexts;
using Order.API.Enums;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer(OrderApiDbContext _context) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = await _context.Orders.FindAsync(context.Message.OrderId);
        if(order == null)
            throw new NullReferenceException($"Order with id {context.Message.OrderId} not found");
        order.OrderStatus = OrderStatus.Fail;
        await _context.SaveChangesAsync();
    }
}