using KonyvtarApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KonyvtarApi.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(int oldalSzam = 1, int oldalDb = 10)
        {
            if (oldalSzam <= 0) oldalSzam = 1;
            if (oldalDb <= 0) oldalDb = 10;

            var books = await _context.Books
                .Skip((oldalSzam - 1) * oldalDb)
                .Take(oldalDb)
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            if (book.PublishedYear <= 0 || book.PublishedYear > DateTime.Now.Year)
            {
                return BadRequest(new { message = "A megadott kiadási év nem megfelelő!" });
            }
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Book>> UpdateBook(int id,Book book)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (existingBook != null)
            {
                if (book.Author != null && book.Title != null &&  book.PublishedYear > 0 && book.PublishedYear <= DateTime.Now.Year)
                {
                    existingBook.PublishedYear = book.PublishedYear;
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Genre = book.Genre;
                    existingBook.Price = book.Price;
                    _context.Books.Update(existingBook);
                    await _context.SaveChangesAsync();
                    return Ok(existingBook);
                }
                else
                {
                    return BadRequest(new { message = "A megadott kiadási év nem megfelelő!" });
                }
            }
            return NotFound(new { message= "Nem található ilyen Id-val könyv!" });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Sikeres törlés!" });
            }
            return NotFound();
        }
        [HttpGet("author/{author}")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchByAuthor(string author)
        {
            return await _context.Books.Where(b => b.Author.Contains(author)).ToListAsync();
        }
        [HttpGet("title/{title}")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchByTitle(string title)
        {
            return await _context.Books.Where(b => b.Title.Contains(title)).ToListAsync();
        }
    }
}
