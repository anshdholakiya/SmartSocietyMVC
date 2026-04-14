namespace SmartSocietyMVC.Models
{
    public class DashboardStats
    {
        public decimal TotalCollected { get; set; }
        public decimal TotalPending { get; set; }
        public int TotalResidents { get; set; }
        public int ActiveIssues { get; set; }
        
        public decimal MyBalance { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
