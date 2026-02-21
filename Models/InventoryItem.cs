namespace InventoryManager.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public decimal PricePerItem { get; set; }

        public decimal TotalValue => QuantityOnHand * PricePerItem;
    }
}
