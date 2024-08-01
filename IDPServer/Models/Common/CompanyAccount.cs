namespace IDPServer.Models;

public partial class CompanyAccount
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public int AccountId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime? EndDate { get; set; }
}