namespace MarInTime.Domain.DTOs
{
    public record class PortDto (
        string FullId,
        string Name,
        string Country,
        string[] Routes,
        double Latitude,
        double Longitude
    );
}
