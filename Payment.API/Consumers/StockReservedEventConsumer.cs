using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers;

public class StockReservedEventConsumer(IPublishEndpoint _publishEndpoint) : IConsumer<StockReservedEvent>
{
    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        if (true)
        {
            // Ödeme başarılı
            PaymentCompletedEvent paymentCompletedEvent = new()
            {
                OrderId = context.Message.OrderId,
            };
            await context.Publish(paymentCompletedEvent);
            await Console.Out.WriteLineAsync("Payment completed");
        }
        else
        {
            PaymentFailedEvent paymentFailedEvent = new()
            {
                OrderId = context.Message.OrderId,
                Message = "Yetersiz Bakiye",
                OrderItems = context.Message.OrderItems
            };
            await context.Publish(paymentFailedEvent);
            await Console.Out.WriteLineAsync("Payment not completed");

        }
        
    }
}