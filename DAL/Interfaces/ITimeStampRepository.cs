using discordbot.DAL.Entities;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace discordbot.DAL.Interfaces
{
    interface ITimeStampRepository : IBaseRepository<TimeStamp>
    {
    }
}
