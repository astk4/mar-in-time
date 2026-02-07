using MarInTime.Domain.Entities;
using NetTopologySuite.Geometries;

namespace MarInTime.Domain
{
    public class GidSpecification : Specification<SpatialEntity>
    {
        public GidSpecification(int gid) : base((sp) => sp.GID == gid)
        {
        }
    }
}
