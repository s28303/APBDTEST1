using ApbdTest1.Models.DTOs;

namespace ApbdTest1.Repositories
{
    public interface IBooksRepository
    {
        Task<bool> DoesBookExist(int id);
        Task<bool> DoesPublisherExist(int id);
        Task<List<BookEditionsDTO>> GetAllBooksEditionsAsync(int id);
        Task AddNewBookWithEditions(NewBookWithEdition newBookWithEdition);
    }
}
