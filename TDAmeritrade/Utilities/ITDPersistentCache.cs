namespace TDAmeritrade
{

    /// <summary>
    /// Abstraction for saving persistent data
    /// </summary>
    public interface ITDPersistentCache
    {
        void Save(string key, string value);
        string Load(string key);
    }
}
