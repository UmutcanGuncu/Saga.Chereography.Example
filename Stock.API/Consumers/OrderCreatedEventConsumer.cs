using System.Collections.ObjectModel;
using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly MongoDbService _mongoDbService;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    public OrderCreatedEventConsumer(MongoDbService mongoDbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _mongoDbService = mongoDbService;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResults = new List<bool>();
        IMongoCollection<Models.Stock>  collection = _mongoDbService.GetCollection<Models.Stock>();
        foreach (var item in context.Message.OrderItemMessages)
        {
            stockResults.Add( await (await collection.FindAsync(x => x.ProductId == item.ProductId.ToString() && x.Count >= (long)item.Count)).AnyAsync());
            
        }

        if (stockResults.TrueForAll(x => x.Equals(true)))
        {
            // Stok güncelle
            foreach (var item in context.Message.OrderItemMessages)
            {
                var stock = await(await collection.FindAsync(x => x.ProductId == item.ProductId.ToString()))
                    .FirstOrDefaultAsync();
                stock.Count -= item.Count;
                await collection.FindOneAndReplaceAsync(x=> x.ProductId == item.ProductId.ToString(), stock);
            }
            //paymenti uygulayacak event fırlatılacak
             var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventName}"));
             StockReservedEvent stockReservedEvent = new()
             {
                BuyerId = context.Message.BuyerId,
                TotalPrice = context.Message.TotalPrice,
                OrderId = context.Message.OrderId,
                OrderItems = context.Message.OrderItemMessages,
             };
             await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            //Order'ı uyaracak event fırlat
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                Message = "Elimizde Yeterince Stok Bulunamamaktadır. "
            };
            await _publishEndpoint.Publish(stockNotReservedEvent);
        }
    }
}