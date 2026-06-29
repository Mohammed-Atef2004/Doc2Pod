using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(
            string key,
            T? value,
            TimeSpan? absoluteExpiration=null,
            TimeSpan? slidingExpiration=null);
        void Remove(string key);
        bool TryGetValue<T>(string key, out T value);

    }
}
