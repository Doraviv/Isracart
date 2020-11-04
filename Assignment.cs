using Microsoft.AspNetCore.Http;
using System;
using System.Runtime.CompilerServices;

namespace Isracart
{
    public class Assignment
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string FilePath { get; set; }
    }
}
