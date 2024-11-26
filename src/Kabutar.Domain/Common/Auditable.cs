
namespace Kabutar.Domain.Common;

public class Auditable : BaseEntity
{
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; }
}
