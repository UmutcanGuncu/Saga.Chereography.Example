using MassTransit;
using Order.API.Contexts;
using Order.API.Enums;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer(OrderApiDbContext _context): IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = await _context.Orders.FindAsync(context.Message.OrderId);
        if(order == null)
            throw new Exception($"Order with id {context.Message.OrderId} not found");
        order.OrderStatus = OrderStatus.Complete;
        await _context.SaveChangesAsync();
    }
}