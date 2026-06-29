using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.PythonService
{
    public class InMemoryCacheService : ICacheService
    {
        public IMemoryCache _memoryCashe;

        public InMemoryCacheService(IMemoryCache memoryCashe)
        {
            _memoryCashe = memoryCashe;
        }
        public T?Get<T>(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            _memoryCashe.TryGetValue(key, out T value);
            return value;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("Cache key cannot be null or empty", nameof(key));
      
                _memoryCashe.Remove(key);
        }

        public void Set<T>(string key, T? value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            var options = new MemoryCacheEntryOptions();
            if (absoluteExpiration != null)
                options.AbsoluteExpirationRelativeToNow=absoluteExpiration;
            if(slidingExpiration != null)
                options.SlidingExpiration=slidingExpiration;
            _memoryCashe.Set(key,value, options);
                
        }

        public bool TryGetValue<T>(string key, out T value)
        {
           if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            return _memoryCashe.TryGetValue(key ,out value);
        }
    }
}
