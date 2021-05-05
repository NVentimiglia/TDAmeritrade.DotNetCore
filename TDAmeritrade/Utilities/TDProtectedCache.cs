//using Microsoft.AspNetCore.DataProtection;
//using System.Text.Json;

//namespace TDAmeritrade
//{
//    /// <summary>
//    /// Saves security token as an PROTECTED FILE
//    /// </summary>
//    public class TDProtectedCache : ITDPersistentCache
//    {
//        private IDataProtectionProvider _provider;
//        private TDUnprotectedCache _cache;

//        public TDProtectedCache(IDataProtectionProvider provider, string key = "")
//        {
//            _cache = new TDUnprotectedCache(key);
//            _provider = provider;
//        }
//        public string Load(string key)
//        {
//            var protector = _provider.CreateProtector(key);
//            var data = _cache.Load(key);
//            return protector.Unprotect(data);
//        }

//        public void Save(string key, string value)
//        {
//            var protector = _provider.CreateProtector(key);
//            var json = JsonSerializer.Serialize(value);
//            var data = protector.Protect(json);
//            _cache.Save(key, data);
//        }
//    }
//}
