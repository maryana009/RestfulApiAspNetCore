using System;
using System.Collections.Generic;
using Library.API.Services;
using Library.API.Models;
using Library.API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Library.API.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BookController: Controller
    {
        private ILibraryRepository _libraryRepository;
        private ILogger<BookController> _logger;

        public BookController(ILibraryRepository libraryRepository, ILogger<BookController> logger)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            try
            {
                var result = _libraryRepository.GetBooksForAuthor(authorId);

                var books = Mapper.Map<IEnumerable<BookDto>>(result);
                return new OkObjectResult(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something unexpected happened");
            }
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var repoBook = _libraryRepository.GetBookForAuthor(authorId, id);

            if (repoBook == null) {
                return NotFound();
            }

            var book = Mapper.Map<BookDto>(repoBook);
            return new OkObjectResult(book);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody]BookForCreationDto book)
        {
            if (book == null)
                return BadRequest();

            //custom checks, not everything can be checked with annotation attributes
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be dofferent from title.");
            }

            //ModelState is a dictionary containing State of the model and model binding validation and collection of error messages
            if (!ModelState.IsValid)
            {
                //return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var entity = Mapper.Map<Entities.Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, entity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed");
                //return new StatusCode(500, "Creating authors failed");
            }

            var bookToReturn = Mapper.Map<BookDto>(entity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId= authorId, id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
                return NotFound();

            _libraryRepository.DeleteBook(book);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} failed on save");
            }

            _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted");
            return NoContent();
        }

        //put always updates the whole things, so if we don't pass description for a book, descr field is updated to null
        //if we decide to update books for authors, put updates the whole collection, so it replaces old collection with a new one
        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody]BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            //custom checks, not everything can be checked with annotation attributes
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be dofferent from title.");
            }

            //ModelState is a dictionary containing State of the model and model binding validation and collection of error messages
            if (!ModelState.IsValid)
            {
                //return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookFromRepo == null)
            {
                return NotFound();          
            }

            //map, apply update, map back to entiity
            Mapper.Map(book, bookFromRepo);

            //Empty method, unnesessaried in this case
            //_libraryRepository.UpdateBookForAuthor(bookFromRepo);

            //after map, it's enough to call repos. safe
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }

        //partially updating book
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody]JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookFromRepo == null)
            {
                //return NotFound();

                // if we want to upsert book
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;


                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                // returns 201 Created
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState); //- works in Pluralsight implementation but not in mine
            //patchDoc.ApplyTo(bookToPatch);

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                //return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(bookToPatch, bookFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save");
            }

            return NoContent();
        }
    }
}
