namespace GraphSharp
{
    public sealed class Box<T>
    {
        public T Value;

        public Box(T value)
        {
            Value = value;
        }

    }
}