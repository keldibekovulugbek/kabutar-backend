using Kabutar.DataAccess.Interfaces;
using Kabutar.Service.DTOs.Search;

namespace Kabutar.Service.Services.Search;

public interface ISearchService
{
    Task<SearchResultDTO> SearchAsync(string searchText, long currentUserId);
}

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SearchResultDTO> SearchAsync(string searchText, long currentUserId)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new SearchResultDTO();
        }

        var result = new SearchResultDTO();

        // 1. Avval userlarni qidirish (username, firstname, lastname)
        var users = await _unitOfWork.Users.SearchUsersAsync(searchText, currentUserId);
        result.Users = users.Select(u => new UserSearchResultDTO
        {
            Id = u.Id,
            Username = u.Username,
            Firstname = u.FirstName,
            Lastname = u.LastName,
            ProfilePicture = u.ProfilePicture,
            IsOnline = u.LastActive.HasValue && u.LastActive.Value > DateTime.UtcNow.AddMinutes(-5)
        }).ToList();

        // 2. Keyin messagelarni qidirish (shu user bilan bog'liq)
        var messages = await _unitOfWork.Messages.SearchMessagesAsync(searchText, currentUserId);
        result.Messages = messages.Select(m =>
        {
            var otherUser = m.SenderId == currentUserId ? m.Receiver : m.Sender;
            return new MessageSearchResultDTO
            {
                MessageId = m.Id,
                ChatId = otherUser.Id, // Chat ID = other user ID (for direct messages)
                UserId = otherUser.Id,
                Username = otherUser.Username,
                Firstname = otherUser.FirstName,
                Lastname = otherUser.LastName,
                ProfilePicture = otherUser.ProfilePicture,
                MessageContent = m.Content ?? string.Empty,
                SentAt = m.Created
            };
        }).ToList();

        return result;
    }
}
