namespace StreamAIS.Models
{
    public class AisMessageWrapper<T> where T : class
    {
        public T Message { get; set; } = null!;
    }
}
