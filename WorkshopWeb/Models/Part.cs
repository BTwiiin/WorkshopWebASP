namespace WorkshopWeb.Models
{
    public class Part
    {
        public int PartId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Amount * UnitPrice;
    }
}
