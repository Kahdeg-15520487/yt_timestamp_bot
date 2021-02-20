using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace discordbot.DAL.Implementations
{
    class TimestampRepository : BaseRepository<TimeStamp>, ITimeStampRepository
    {
        public TimestampRepository(LiteDBContextFactory db) : base(db) { }
    }
}
