using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    //In this case, it's he sme as dtoForCreation, but in general, in creation could be fields that not supposed to be updated, 
    //that's why different models

    public class BookForUpdateDto: BookForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out the description")]
        public override string Description {
            get {
                return base.Description;
            }
            set {
                base.Description = value;
            }
        }
    }
}
