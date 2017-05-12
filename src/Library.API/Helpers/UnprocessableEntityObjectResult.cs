using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.API.Helpers
{
    public class UnprocessableEntityObjectResult: ObjectResult
    {
        //will return the whole object with all properties
        //public UnprocessableEntityObjectResult (object error):base(error)
        //{
        //    StatusCode = 422;
        //}

        public UnprocessableEntityObjectResult(ModelStateDictionary modelState) : base(new SerializableError(modelState))
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }
            StatusCode = 422;
        }
    }
}
