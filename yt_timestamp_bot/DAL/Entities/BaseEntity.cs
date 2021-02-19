using LiteDB;

using System;
using System.Collections.Generic;
using System.Text;

namespace discordbot.DAL.Entities
{
    class BaseEntity
    {
        public ObjectId Id { get; set; }
    }
}
