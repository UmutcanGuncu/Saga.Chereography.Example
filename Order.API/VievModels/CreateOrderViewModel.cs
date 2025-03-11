namespace Order.API.VievModels;

public class CreateOrderViewModel
{
    public string BuyerId { get; set; }
    public List<CreateOrderItemViewModel> OrderItems { get; set; }
}