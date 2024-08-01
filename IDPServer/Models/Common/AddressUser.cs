namespace IDPServer.Models;

public partial class AddressUser
{
    public long Id { get; set; }

    public long AddressId { get; set; }

    public long UserId { get; set; }

    public int? IsDefault { get; set; }

    public int? Status { get; set; }

    public string CreatedByUserId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string ModifiedByUserId { get; set; }

    public DateTime? ModifiedDateTime { get; set; }

    public Guid BusinessId { get; set; }
}