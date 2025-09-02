namespace Sky.Template.Backend.Contract.Requests.BuyerAddresses;

public class UpdateBuyerAddressRequest
{
    public Guid Id { get; set; }
    public Guid BuyerId { get; set; }
    public string AddressTitle { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
