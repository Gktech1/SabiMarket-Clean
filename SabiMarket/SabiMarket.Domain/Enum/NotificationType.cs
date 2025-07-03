namespace SabiMarket.Domain.Enum
{
    public enum NotificationType
    {
        General = 0,
        OrderPlaced = 1,
        OrderShipped = 2,
        OrderDelivered = 3,
        PaymentReceived = 4,
        ProductLowStock = 5,
        NewPromotion = 6,
        SystemAlert = 7,
        UserMessage = 8
    }
}
