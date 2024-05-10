using ApbdTest1.Models.DTOs;
using ApbdTest1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApbdTest1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        public BooksController(IBooksRepository booksRepository) 
        {
            _booksRepository = booksRepository;
        }

        [HttpGet("{id}/editions")]
        public async Task<IActionResult> getAllBooksEditions(int id)
        {
            if (!await _booksRepository.DoesBookExist(id))
                return NotFound($"Book with given ID - {id} doesn't exist");

            var booksEditions = await _booksRepository.GetAllBooksEditionsAsync(id);

            return Ok(booksEditions);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(NewBookWithEdition newBookWithEdition)
        {
            if (!await _booksRepository.DoesPublisherExist(newBookWithEdition.publishingHouseId))
                return NotFound($"Publisher with given ID - {newBookWithEdition.publishingHouseId} doesn't exist");

            await _booksRepository.AddNewBookWithEditions(newBookWithEdition);

            return Created(Request.Path.Value ?? "api/Books", newBookWithEdition);
        }

    }
}
