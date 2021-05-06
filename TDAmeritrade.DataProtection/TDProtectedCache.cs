using Microsoft.AspNetCore.DataProtection;

namespace TDAmeritrade
{
    /// <summary>
    /// Saves security token as an PROTECTED FILE
    /// </summary>
    public class TDProtectedCache : ITDPersistentCache
    {
        private IDataProtectionProvider _provider;
        private TDUnprotectedCache _cache;

        public TDProtectedCache(IDataProtectionProvider provider)
        {
            _cache = new TDUnprotectedCache();
            _provider = provider;
        }
        public string Load(string key)
        {
            lock (this)
            {
                var protector = _provider.CreateProtector(key);
                var data = _cache.Load(key);
                var json = protector.Unprotect(data);
                return json;
            }
        }

        public void Save(string key, string value)
        {
            lock (this)
            {
                var protector = _provider.CreateProtector(key);
                var data = protector.Protect(value);
                _cache.Save(key, data);
            }
        }

    }
}
