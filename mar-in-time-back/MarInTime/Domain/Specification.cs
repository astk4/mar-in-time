namespace MarInTime.Domain
{
    public class Specification<T> : ISpecification<T>
    {
        private readonly Func<T, bool> predicate;

        public Specification(Func<T, bool> predicate) => this.predicate = predicate;
        
        public bool IsSatisfiedBy(T candidate) => predicate(candidate);
    }
}
