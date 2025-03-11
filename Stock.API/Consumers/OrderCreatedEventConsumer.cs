using System.Collections.ObjectModel;
using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly MongoDbService _mongoDbService;

    public OrderCreatedEventConsumer(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResults = new List<bool>();
        IMongoCollection<Models.Stock>  collection = _mongoDbService.GetCollection<Models.Stock>();
        foreach (var item in context.Message.OrderItemMessages)
        {
            stockResults.Add( await (await collection.FindAsync(x => x.ProductId == item.ProductId && x.Count >= item.Count)).AnyAsync());
            
        }

        if (stockResults.TrueForAll(x => x.Equals(true)))
        {
            // Stok güncelle
            //paymenti uygulayacak event fırlatılacak
        }
        else
        {
            //Order'ı uyaracak event fırlat
        }
    }
}