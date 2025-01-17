namespace ET.Models
{
    public class Debt : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsCleared { get; set; }
    }
}