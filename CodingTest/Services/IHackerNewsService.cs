using CodingTest.Models;
namespace CodingTest.Services
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<Story>>GetBestStoriesAsync(int n);
    }
}
