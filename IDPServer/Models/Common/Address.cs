namespace IDPServer.Models;

public partial class Address
{
    public long Id { get; set; }

    public int AddressTypeId { get; set; }

    public int CountryId { get; set; }

    public int ProvinceId { get; set; }

    public int CountyId { get; set; }

    public string Address1 { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string Telephone { get; set; }

    public string PostalCode { get; set; }

    public int? Plaque { get; set; }

    public string Name { get; set; }

    public string CreatedByUserId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string ModifiedByUserId { get; set; }

    public DateTime? ModifiedDateTime { get; set; }

    public Guid BusinessId { get; set; }
}