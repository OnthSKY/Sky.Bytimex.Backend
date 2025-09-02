namespace Sky.Template.Backend.Core.BaseResponse;

public class BaseServiceResponse
{
    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; } = -1;

    public DateTime UpdatedAt { get; set; }

    public int UpdatedBy { get; set; } = -1;
}
