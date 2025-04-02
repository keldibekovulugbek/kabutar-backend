namespace Kabutar.Service.DTOs.Common;

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = default!;
}
