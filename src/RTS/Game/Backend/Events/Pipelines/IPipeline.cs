public interface IPipeline<T> where T : IEndable
{
    public void Run(T e);
}