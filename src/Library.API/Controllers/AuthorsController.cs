using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Library.API.Services;
using Library.API.Entities;
using Library.API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Routing;
using Library.API.Helpers; 

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController: Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;
        public PropertyMappingService _propertyMappingService;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, PropertyMappingService propertyMappingService)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet("all")]
        public IActionResult GetAllAuthors()
        {
            try
            {
                //throw new Exception("Random Exception for testing");

                var result = _libraryRepository.GetAuthors();

                var authors = Mapper.Map<IEnumerable<AuthorDto>>(result);
                return new JsonResult(authors);
            }
            catch (Exception ex) {
                return StatusCode (500, "Something unexpected happened");
            }
        }

        [HttpGet(Name = "GetAuthors")]
        //It's ok, but having this parameters in different class is better
        //public IActionResult GetAuthors([FromQuery(Name = "page")]int pageNumber = 1, [FromQuery] int pageSize = 10)
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (! _propertyMappingService.ValidateMappingPropertyExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            try
            {
                //throw new Exception("Random Exception for testing");

                var result = _libraryRepository.GetAuthors(authorsResourceParameters);

                var previousPageLink = result.HasPrevious ? CreatAuthorsResourcUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;
                var nextPageLink = result.HasNext ? CreatAuthorsResourcUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = result.TotalCount,
                    pageSize = result.PageSize,
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink

                };

                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                var authors = Mapper.Map<IEnumerable<AuthorDto>>(result);

                //return new OkObjectResult(authors);
                return new OkObjectResult(authors.ShapeData(authorsResourceParameters.Fields));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something unexpected happened");
            }
        }

        private string CreatAuthorsResourcUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize,
                            genre = authorsResourceParameters.Genre,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            orderBy = authorsResourceParameters.OrderBy
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize,
                            genre = authorsResourceParameters.Genre,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            orderBy = authorsResourceParameters.OrderBy
                        });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            orderBy = authorsResourceParameters.OrderBy
                        });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        //public IActionResult GetAuthor([FromRoute] Guid id)
        public IActionResult GetAuthor(Guid id)
        {
            var result = _libraryRepository.GetAuthor(id);
            if (result == null)
                return NotFound();

            var author = Mapper.Map<AuthorDto>(result);
            return new JsonResult(author);
        }


        //POST api/authors - {author} may return 201 {author}, 404
        //POST api/authors/{authorId} can never be successfull, returns 404 (not found) or 409 (conflict)
        [HttpPost()]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
                return BadRequest();

            var entity = Mapper.Map<Entities.Author>(author);

            _libraryRepository.AddAuthor(entity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating authors failed");
                //return new StatusCode(500, "Creating authors failed");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(entity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _libraryRepository.GetAuthor(id);

            if (author == null)
                return NotFound();

            _libraryRepository.DeleteAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting authors {id} failed on save");
                //return new StatusCode(500, "Creating authors failed");
            }

            return NoContent();
        }
    }
}
