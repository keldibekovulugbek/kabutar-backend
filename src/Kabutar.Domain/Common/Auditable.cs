
namespace Kabutar.Domain.Common;

public abstract class Auditable : BaseEntity
{
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime Updated { get; set; }
}
