using MarInTime.Domain.Entities;
using NetTopologySuite.Geometries;

namespace MarInTime.Domain
{
    public class WithinSpecification<T> : Specification<T> where T : SpatialEntity
    {
        public WithinSpecification(double lng, double lat) : base((sp) => sp.Geometry.Intersects(new Point(lng, lat) { SRID = 4326 }))
        {
        }
    }
}
