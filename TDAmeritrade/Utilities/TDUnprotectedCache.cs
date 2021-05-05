using System.IO;

namespace TDAmeritrade
{
    /// <summary>
    /// Saves security token as an UNPROTECTED FILE
    /// DO NOT USE IN PRODUCTION
    /// </summary>
    public class TDUnprotectedCache : ITDPersistentCache
    {
        private string _root;

        public TDUnprotectedCache(string root = "")
        {
            _root = root;
        }

        private void Ensure(string path)
        {
            if (!File.Exists(path))
            {
                using (var s = File.Create(path)) { }
            }
        }
        public string Load(string key)
        {
            lock (this)
            {
                var path = Path.Combine(_root, key);
                Ensure(path);
                return File.ReadAllText(path);
            }
        }
        public void Save(string key, string value)
        {
            lock (this)
            {
                var path = Path.Combine(_root, key);
                Ensure(path);
                File.WriteAllText(key, value);
            }
        }
    }
}
