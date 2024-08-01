namespace IDPServer.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string ShortName { get; set; }

    public string NationalId { get; set; }

    public string Photo { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// شناسه سازمان
    /// </summary>
    public int? MinGradeId { get; set; }

    /// <summary>
    /// نوع محاسبه مدت وصول 1-مشتری 2- محاسباتی
    /// </summary>
    public int? CalculationCollectionPeriod { get; set; }

    public string CreatedByUserId { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public string ModifiedByUserId { get; set; }

    public DateTime? ModifiedDateTime { get; set; }

    public Guid BusinessId { get; set; }

    public string Email { get; set; }

    public long? AddressId { get; set; }
}