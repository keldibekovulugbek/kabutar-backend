using Kabutar.Domain.Common;

namespace Kabutar.Domain.Entities.Attachments;

public class Attachment : Auditable
{
    public long MessageId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty; // ✅ qo‘shamiz
}
