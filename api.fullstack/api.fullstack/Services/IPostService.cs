using api.fullstack.Dtos;

namespace api.fullstack.Services
{
    public interface IPostService
    {
        Task<List<UserPostsDto>> GetPostsAsync(string? q = null);
    }
}
