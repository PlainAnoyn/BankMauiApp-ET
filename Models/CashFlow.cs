namespace ET.Models
{
    public class CashFlow : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }  // Can be used for both Source and Category
        public string Description { get; set; }
        public bool IsInflow { get; set; }  // Indicates if it's an inflow or outflow
    }
}