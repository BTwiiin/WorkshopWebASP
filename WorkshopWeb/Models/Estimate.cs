namespace WorkshopWeb.Models
{
    public class Estimate
    {
        public int EstimateId { get; set; }

        public string Description { get; set; }

        public decimal ExpectedCostOfService { get; set; }

        public bool IsAcceptedByClient { get; set; }
    }
}
