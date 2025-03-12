using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Contexts;
using Order.API.Enums;
using Order.API.VievModels;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<PaymentCompletedEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();
    configure.AddConsumer<StockNotReservedEventConsumer>();
    configure.UsingRabbitMq((context, _configure) =>
    {
        _configure.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventName, e=> e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventName, e=> e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventName, e=> e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});
builder.Services.AddDbContext<OrderApiDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order", async (CreateOrderViewModel model, OrderApiDbContext context, IPublishEndpoint publishEndpoint) =>
{
    Order.API.Models.Order order = new Order.API.Models.Order()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid id) ? id : Guid.NewGuid(),
        Items = model.OrderItems.Select(x => new Order.API.Models.OrderItem()
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = Guid.Parse(x.ProductId),
        }).ToList(),
        OrderStatus = OrderStatus.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(x => x.Price * x.Count)
    };
     await context.Orders.AddAsync(order);
     await context.SaveChangesAsync();
     OrderCreatedEvent orderCreatedEvent = new()
     {
         BuyerId = order.BuyerId,
         OrderId = order.Id,
         TotalPrice = order.TotalPrice,
         OrderItemMessages = order.Items.Select(x=> new Shared.Messages.OrderItemMessage()
         {
             Count = x.Count,
             Price = x.Price,
             ProductId = x.ProductId,
         }).ToList(),
     };
     await publishEndpoint.Publish(orderCreatedEvent);
});
app.UseHttpsRedirection();
app.Run();

