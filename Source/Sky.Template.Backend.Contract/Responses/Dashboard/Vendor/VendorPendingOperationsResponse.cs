
namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class VendorPendingOperationsResponse
{
    public int PendingOrders { get; set; }
    public int PendingKycVerifications { get; set; }
    public int PendingReturnRequests { get; set; }
}
