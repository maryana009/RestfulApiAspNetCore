using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Services;
using Library.API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
//using System.Web;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionController: Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorCollectionController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost()]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
        {
            if (authorCollection == null)
                return BadRequest();

            var entities = Mapper.Map<IEnumerable<Entities.Author>>(authorCollection);

            foreach (var e in entities)
            {
                _libraryRepository.AddAuthor(e);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating authors collection failed");
                //return new StatusCode(500, "Creating authors failed");
            }

            return Ok();
        }

        //[HttpGet("({ids})")]
        //public IActionResult GetAuthorCollection(
        //    [ModelBinder(BinderType =typeof (ArrayModelBinder)] IEnumerable<Guid> ids)
        //{
        //    try
        //    {
        //        //throw new Exception("Random Exception for testing");

        //        var result = _libraryRepository.GetAuthors();

        //        var authors = Mapper.Map<IEnumerable<AuthorDto>>(result);
        //        return new JsonResult(authors);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Something unexpected happened");
        //    }
        //}
    }
}
