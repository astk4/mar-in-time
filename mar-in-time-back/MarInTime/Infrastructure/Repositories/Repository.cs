using Microsoft.EntityFrameworkCore;

namespace MarInTime.Infrastructure.Repositories
{
    public abstract class Repository<T>(T context) where T : DbContext
    {
        protected readonly T context = context;
    }
}
