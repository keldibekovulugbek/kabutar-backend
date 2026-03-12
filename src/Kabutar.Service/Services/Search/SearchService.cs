using Kabutar.DataAccess.Interfaces;
using Kabutar.Service.DTOs.Search;
using Kabutar.Service.Interfaces.Common;

namespace Kabutar.Service.Services.Search;

public interface ISearchService
{
    Task<SearchResultDTO> SearchAsync(string searchText, long currentUserId);
}

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryption;
    private readonly IOnlineTracker _onlineTracker;

    public SearchService(IUnitOfWork unitOfWork, IEncryptionService encryption, IOnlineTracker onlineTracker)
    {
        _unitOfWork = unitOfWork;
        _encryption = encryption;
        _onlineTracker = onlineTracker;
    }

    public async Task<SearchResultDTO> SearchAsync(string searchText, long currentUserId)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new SearchResultDTO();
        }

        var result = new SearchResultDTO();


        var users = await _unitOfWork.Users.SearchUsersAsync(searchText, currentUserId);
        result.Users = users.Select(u => new UserSearchResultDTO
        {
            Id = u.Id,
            Username = u.Username,
            Firstname = u.FirstName,
            Lastname = u.LastName,
            ProfilePicture = u.ProfilePicture,
            IsOnline = _onlineTracker.IsOnline(u.Id)
        }).ToList();


        var allMessages = await _unitOfWork.Messages.GetAllMessagesForUserAsync(currentUserId);
        var searchLower = searchText.ToLower().Trim();

        var matchedMessages = allMessages
            .Select(m =>
            {
                var decrypted = DecryptSafe(m.Content);
                return (Message: m, DecryptedContent: decrypted);
            })
            .Where(x => x.DecryptedContent.ToLower().Contains(searchLower))
            .OrderByDescending(x => x.Message.Created)
            .Take(20)
            .ToList();

        result.Messages = matchedMessages.Select(x =>
        {
            var m = x.Message;
            var otherUser = m.SenderId == currentUserId ? m.Receiver : m.Sender;
            return new MessageSearchResultDTO
            {
                MessageId = m.Id,
                ChatId = otherUser.Id,
                UserId = otherUser.Id,
                Username = otherUser.Username,
                Firstname = otherUser.FirstName,
                Lastname = otherUser.LastName,
                ProfilePicture = otherUser.ProfilePicture,
                MessageContent = x.DecryptedContent,
                SentAt = m.Created
            };
        }).ToList();

        return result;
    }

    private string DecryptSafe(string? content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        try { return _encryption.Decrypt(content); }
        catch { return content; }
    }
}
