using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.DAL.Entities
{
    class Token : BaseEntity
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}
