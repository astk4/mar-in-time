namespace MarInTime.Infrastructure.TransportModels
{
    public abstract record SpatialEntityDisplayDto(int GID, string Name, string GeometryAsString);
}
