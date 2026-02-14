namespace StreamAIS.Models
{
    public record MessageKitDto(byte[] MessageBuffer, byte[] TypeBuffer, int BytesCount)
    {
    }
}
