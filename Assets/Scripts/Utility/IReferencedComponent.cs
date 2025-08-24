namespace Slates.Utility
{
    public interface IReferencedComponent<T>
    {
        public T Owner { get; }
        public void Init(T owner);
    }
}
