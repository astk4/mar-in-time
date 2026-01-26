using MarInTime.Infrastructure.TransportModels;

namespace MarInTime.Application.Repositories
{
    public interface IGisRepository
    {
        public IEnumerable<string> GetRelationNames(string funcName);
        public IAsyncEnumerable<SpatialEntityDisplayDto> GetChunksInBounds(string tableName, double xMin, double yMin, double xMax, double yMax, bool orderByArea);
    }
}
