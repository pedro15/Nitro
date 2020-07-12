namespace Nitro.Data.Serialization
{
    public interface IJSONProvider
    {
        string ToJSON(object obj);

        T ToObject<T>(string json);
    }
}
