namespace AdServer.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? NextReportScheduled { get; set; }
        public string? ReportMail { get; set; }

        public Campaign(string name)
        {
            Name = name;
        }
    }
}
