using MassTransit;
using Shared;
using MongoDB.Driver;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();
    configure.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventName, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventName, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});
builder.Services.AddSingleton<MongoDbService>();
var app = builder.Build();
using IServiceScope serviceScope = app.Services.CreateScope();
MongoDbService mongoDbService =  serviceScope.ServiceProvider.GetRequiredService<MongoDbService>();
var stockCollection = mongoDbService.GetCollection<Stock.API.Models.Stock>();
if (!stockCollection.FindSync(session => true).Any())
{
    await stockCollection.InsertOneAsync(new()
    {
        ProductId = "38fa4244-1af6-4568-b60c-98e9055d2a0c",
        Count = 100,
        CreatedDate = DateTime.UtcNow,
        Id = Guid.NewGuid()
    });
    await stockCollection.InsertOneAsync(new()
    {
        ProductId = "f6a27618-bd26-406d-8326-e15b52e1f84a",
        Count = 200,
        CreatedDate = DateTime.UtcNow,
        Id = Guid.NewGuid()
    });
    await stockCollection.InsertOneAsync(new()
    {
        ProductId = "45f0dccf-4057-4097-99c1-ed1762943dc6",
        Count = 300,
        CreatedDate = DateTime.UtcNow,
        Id = Guid.NewGuid()
    });
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();
