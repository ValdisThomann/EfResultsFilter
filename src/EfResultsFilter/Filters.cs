using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfResultFilter
{
    #region GlobalFiltersSignature

    public class Filters
    {
        public delegate bool Filter<in T>(T input);

        public delegate Task<bool> AsyncFilter<in T>(T input);

        #endregion

        public void Add<T>(Filter<T> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(T)] =
                item =>
                {
                    try
                    {
                        return Task.FromResult(filter((T) item));
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. T: {typeof(T)}.", exception);
                    }
                };
        }

        public void Add<T>(AsyncFilter<T> filter)
        {
            Guard.AgainstNull(nameof(filter), filter);
            funcs[typeof(T)] =
                async item =>
                {
                    try
                    {
                        return await filter((T) item);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Failed to execute filter. T: {typeof(T)}.", exception);
                    }
                };
        }

        Dictionary<Type, Func<object, Task<bool>>> funcs = new Dictionary<Type, Func<object, Task<bool>>>();

        internal async Task<bool> ShouldInclude(object item)
        {
            if (item == null)
            {
                return false;
            }

            if (funcs.Count == 0)
            {
                return true;
            }

            foreach (var func in FindFilters(item.GetType()))
            {
                if (!await func(item))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<Func<object, Task<bool>>> FindFilters(Type type)
        {
            foreach (var pair in funcs.Where(x => x.Key.IsAssignableFrom(type)))
            {
                yield return item => pair.Value(item);
            }
        }
    }
}