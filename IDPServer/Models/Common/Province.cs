namespace IDPServer.Models;

public partial class Province
{
    public string? Name { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int Id { get; set; }

    public int? CountryId { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string? ModifiedByUserId { get; set; }

    public DateTime? ModifiedDateTime { get; set; }

    public Guid BusinessId { get; set; }
}