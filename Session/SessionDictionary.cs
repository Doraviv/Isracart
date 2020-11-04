using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Isracart
{
    public class SessionDictionary
    {
        public List<Assignment> Assignments { get; set; }

        public DateTime SessionExpirationDate { get; set; }

    }
}
