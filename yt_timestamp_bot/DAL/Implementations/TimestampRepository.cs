using discordbot.DAL.Entities;
using discordbot.DAL.Infrastructure.Interfaces;
using discordbot.DAL.Interfaces;

namespace discordbot.DAL.Implementations
{
    class TimestampRepository : BaseRepository<TimeStamp>, ITimeStampRepository
    {
        public TimestampRepository(IDbContext db) : base(db) { }
    }
}
