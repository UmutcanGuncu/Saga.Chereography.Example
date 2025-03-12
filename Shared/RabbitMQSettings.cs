namespace Shared;

public static class RabbitMQSettings
{
    public const string Stock_OrderCreatedEventName = "stock-order-created-event-queue";
    public const string Payment_StockReservedEventName = "payment-stock-reserved-event-queue";
    public const string Order_PaymentCompletedEventName = "order-payment-completed-event-queue";
    public const string Order_PaymentFailedEventName = "order-payment-failed-event-queue";
    public const string Stock_PaymentFailedEventName = "stock-payment-failed-event-queue";
    public const string Order_StockNotReservedEventName = "order-stock-not-reserved-event-queue";
}