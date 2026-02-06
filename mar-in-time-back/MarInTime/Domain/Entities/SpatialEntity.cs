using NetTopologySuite.Geometries;

namespace MarInTime.Domain.Entities
{
    public abstract class SpatialEntity
    {
        public virtual int GID { get; set; }

        public virtual required MultiPolygon Geometry { get; set; }
    }
}
