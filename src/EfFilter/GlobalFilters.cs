using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfFilter
{
    #region GlobalFiltersSignature

    public class GlobalFilters
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

        internal async Task<IEnumerable<T>> ApplyFilter<T>(IEnumerable<T> result)
        {
            if (funcs.Count == 0)
            {
                return result;
            }

            var filters = FindFilters<T>().ToList();
            if (filters.Count == 0)
            {
                return result;
            }

            var list = new List<T>();
            foreach (var item in result)
            {
                if (await ShouldInclude(item, filters))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        static async Task<bool> ShouldInclude<T>(T item, List<Func<T, Task<bool>>> filters)
        {
            if (item == null)
            {
                return false;
            }

            foreach (var func in filters)
            {
                if (!await func(item))
                {
                    return false;
                }
            }

            return true;
        }

        internal async Task<bool> ShouldInclude<T>(T item)
        {
            if (item == null)
            {
                return false;
            }

            if (funcs.Count == 0)
            {
                return true;
            }

            foreach (var func in FindFilters<T>())
            {
                if (!await func(item))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<Func<T, Task<bool>>> FindFilters<T>()
        {
            var type = typeof(T);
            foreach (var pair in funcs.Where(x => x.Key.IsAssignableFrom(type)))
            {
                yield return item => pair.Value(item);
            }
        }
    }
}