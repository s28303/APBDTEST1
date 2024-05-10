
using ApbdTest1.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace ApbdTest1.Repositories
{
    public class BooksRepository : IBooksRepository
    {
        private readonly IConfiguration _configuration;
        public BooksRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public async Task<bool> DoesBookExist(int id)
        {
            var query = "SELECT 1 FROM [books] WHERE PK = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var res = await command.ExecuteScalarAsync();

            return res is not null;
        }

        public async Task<bool> DoesPublisherExist(int id)
        {
            var query = "SELECT 1 FROM [publishing_houses] WHERE PK = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var res = await command.ExecuteScalarAsync();

            return res is not null;
        }

        public async Task<List<BookEditionsDTO>> GetAllBooksEditionsAsync(int id)
        {
            var query = @"SELECT
							books.PK AS id,
							books.title AS bookTitle,
							books_editions.edition_title AS editionTitle,
                            publishing_houses.name AS publishingHouseName,
                            books_editions.release_date AS releaseDate
						FROM books
						JOIN books_editions ON books_editions.FK_book = books.PK
						JOIN publishing_houses ON books_editions.FK_publishing_house = publishing_houses.PK
						WHERE books.PK = @ID";


            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var reader = await command.ExecuteReaderAsync();

            var bookIdOrdinal = reader.GetOrdinal("id");
            var bookTitleOrdinal = reader.GetOrdinal("bookTitle");
            var bookEditionTitle = reader.GetOrdinal("editionTitle");
            var bookPublishingHouseName = reader.GetOrdinal("publishingHouseName");
            var bookPublishmentReleaseDate = reader.GetOrdinal("releaseDate");

            List<BookEditionsDTO> bookEditionsDTOs = [];


            while (await reader.ReadAsync())
            {
                bookEditionsDTOs.Add(new BookEditionsDTO
                {
                    id = reader.GetInt32(bookIdOrdinal),
                    bookTitle = reader.GetString(bookTitleOrdinal),
                    editionTitle = reader.GetString(bookEditionTitle),
                    publishingHouseName = reader.GetString(bookPublishingHouseName),
                    releaseDate = reader.GetDateTime(bookPublishmentReleaseDate)
                });
            }

            if (bookEditionsDTOs.IsNullOrEmpty()) throw new Exception();

            return bookEditionsDTOs;
        }

        public async Task AddNewBookWithEditions(NewBookWithEdition newBookWithEdition)
        {
            var insert = @"INSERT INTO Books VALUES(@Title);
					   SELECT @@IDENTITY AS PK;";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = insert;

            command.Parameters.AddWithValue("@Title", newBookWithEdition.bookTitle);

            var id = await command.ExecuteScalarAsync();

            command.Parameters.Clear();

            command.CommandText = "INSERT INTO books_editions VALUES(@PublisherId, @BookId, @editionTitle, @relaseDate)";
            command.Parameters.AddWithValue("@PublisherId", newBookWithEdition.publishingHouseId);
            command.Parameters.AddWithValue("@BookId", id);
            command.Parameters.AddWithValue("@editionTitle", newBookWithEdition.editionTitle);
            command.Parameters.AddWithValue("@relaseDate", newBookWithEdition.releaseDate);

            await command.ExecuteNonQueryAsync();
        }
    }
}
