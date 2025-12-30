namespace MarInTime.Domain
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T candidate);
    }
}
