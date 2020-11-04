using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Isracart.ViewModel
{
    public class AssignmentViewModel
    {
        public string Description { get; set; }

        public IFormFile File { get; set; }
    }
}
