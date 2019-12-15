using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Structs
{
    public class MultiMap<K, V> : Dictionary<K, HashSet<V>>
    {

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the default initial capacity, and uses the default equality
        //     comparer for the key type.
        public MultiMap() { }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the default equality comparer for the key type.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public MultiMap(IDictionary<K, HashSet<V>> dictionary) : base(dictionary)
        {

        }

        //
        // Parameters:
        //   collection:
        public MultiMap(IEnumerable<KeyValuePair<K, HashSet<V>>> collection) : base(collection)
        {

        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the default initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        public MultiMap(IEqualityComparer<K> comparer) : base(comparer)
        {

        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the default equality
        //     comparer for the key type.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public MultiMap(int capacity) : base(capacity)
        {

        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public MultiMap(IDictionary<K, HashSet<V>> dictionary, IEqualityComparer<K> comparer) : base(dictionary, comparer)
        {

        }

        //
        // Parameters:
        //   collection:
        //
        //   comparer:
        public MultiMap(IEnumerable<KeyValuePair<K, HashSet<V>>> collection, IEqualityComparer<K> comparer) : base(collection, comparer)
        {

        }

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public MultiMap(int capacity, IEqualityComparer<K> comparer) : base(capacity, comparer)
        {

        }

        /// <summary>
        /// Add the specified value for the passed key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to Add</param>
        /// <returns>Added?</returns>
        public bool Add(K key, V value)
        {
            Preconditions.CheckArgument(key);
            HashSet<V> set = null;
            if (!TryGetValue(key, out set))
            {
                set = new HashSet<V>();
                base.Add(key, set);
            }
            return set.Add(value);
        }

        /// <summary>
        /// Remove the specified value mapped to the passed key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to remove</param>
        /// <returns>Removed?</returns>
        public bool Remove(K key, V value)
        {
            Preconditions.CheckArgument(key);

            HashSet<V> set = null;
            if (TryGetValue(key, out set))
            {
                return set.Remove(value);
            }
            return false;
        }

        /// <summary>
        /// Check if the Map contains the specified value for the passed key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to check</param>
        /// <returns>Contains?</returns>
        public bool ContainsValue(K key, V value)
        {
            Preconditions.CheckArgument(key);

            HashSet<V> set = null;
            if (TryGetValue(key, out set))
            {
                return set.Contains(value);
            }

            return false;
        }

        /// <summary>
        /// Add all the Key/Values from the passed dictionary.
        /// </summary>
        /// <param name="values">Dictionay of Key/Values</param>
        public void AddAll(Dictionary<K, V> values)
        {
            Preconditions.CheckArgument(values);

            if (values.Count > 0)
            {
                foreach (KeyValuePair<K, V> pair in values)
                {
                    Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Copy the passed MultiMap data to this instance.
        /// </summary>
        /// <param name="values">MultiMap data</param>
        public void Copy(MultiMap<K, V> values)
        {
            Preconditions.CheckArgument(values);

            if (values.Count > 0)
            {
                foreach (KeyValuePair<K, HashSet<V>> pair in values)
                {
                    foreach (V value in pair.Value)
                    {
                        Add(pair.Key, value);
                    }
                }
            }
        }

        /// <summary>
        /// Get the collection of values for the passed key.
        /// </summary>
        /// <param name="key">Key to fetch for</param>
        /// <returns>Collection of values</returns>
        public ICollection<V> GetValues(K key)
        {
            Preconditions.CheckArgument(key);

            HashSet<V> set = null;
            if (TryGetValue(key, out set))
            {
                return set;
            }
            return null;
        }
    }

    public class ConcurrentMultiMap<K, V> : ConcurrentDictionary<K, HashSet<V>>
    {
        private const int DEFAULT_READ_LOCK_TIMEOUT = 1 * 60 * 1000; // 1 minute
        private const int DEFAULT_WRITE_LOCK_TIMEOUT = 5 * 60 * 1000; // 5 minutes

        private ReaderWriterLock __lock = new ReaderWriterLock();

        /// <summary>
        /// Add the specified value for the passed key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to Add</param>
        /// <returns>Added?</returns>
        public bool Add(K key, V value)
        {
            Preconditions.CheckArgument(key);
            HashSet<V> set = null;
            if (!TryGetValue(key, out set))
            {
                set = new HashSet<V>();
                base.TryAdd(key, set);
            }
            try
            {
                __lock.AcquireWriterLock(DEFAULT_WRITE_LOCK_TIMEOUT);
                try
                {
                    return set.Add(value);
                }
                finally
                {
                    __lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException e)
            {
                LogUtils.Warn(e);
                return false;
            }
        }

        /// <summary>
        /// Remove the specified value mapped to the passed key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to remove</param>
        /// <returns>Removed?</returns>
        public bool Remove(K key, V value)
        {
            Preconditions.CheckArgument(key);

            HashSet<V> set = null;
            if (TryGetValue(key, out set))
            {
                try
                {
                    __lock.AcquireWriterLock(DEFAULT_WRITE_LOCK_TIMEOUT);
                    try
                    {
                        return set.Remove(value);
                    }
                    finally
                    {
                        __lock.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException e)
                {
                    LogUtils.Warn(e);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if the Map contains the specified value for the passed key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value to check</param>
        /// <returns>Contains?</returns>
        public bool ContainsValue(K key, V value)
        {
            Preconditions.CheckArgument(key);

            HashSet<V> set = null;
            if (TryGetValue(key, out set))
            {
                try
                {
                    __lock.AcquireReaderLock(DEFAULT_READ_LOCK_TIMEOUT);
                    try
                    {
                        return set.Contains(value);
                    }
                    finally
                    {
                        __lock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException e)
                {
                    LogUtils.Warn(e);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Add all the Key/Values from the passed dictionary.
        /// </summary>
        /// <param name="values">Dictionay of Key/Values</param>
        public void AddAll(Dictionary<K, V> values)
        {
            Preconditions.CheckArgument(values);

            if (values.Count > 0)
            {
                try
                {
                    __lock.AcquireWriterLock(DEFAULT_WRITE_LOCK_TIMEOUT);
                    try
                    {
                        foreach (KeyValuePair<K, V> pair in values)
                        {
                            Add(pair.Key, pair.Value);
                        }
                    }
                    finally
                    {
                        __lock.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException e)
                {
                    LogUtils.Warn(e);
               }
            }
        }

        /// <summary>
        /// Copy the passed MultiMap data to this instance.
        /// </summary>
        /// <param name="values">MultiMap data</param>
        public void Copy(MultiMap<K, V> values)
        {
            Preconditions.CheckArgument(values);

            if (values.Count > 0)
            {
                try
                {
                    __lock.AcquireWriterLock(DEFAULT_WRITE_LOCK_TIMEOUT);
                    try
                    {
                        foreach (KeyValuePair<K, HashSet<V>> pair in values)
                        {

                            foreach (V value in pair.Value)
                            {
                                Add(pair.Key, value);
                            }
                        }
                    }
                    finally
                    {
                        __lock.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException e)
                {
                    LogUtils.Warn(e);
                }
            }
        }

        /// <summary>
        /// Get the collection of values for the passed key.
        /// </summary>
        /// <param name="key">Key to fetch for</param>
        /// <returns>Collection of values</returns>
        public ICollection<V> GetValues(K key)
        {
            Preconditions.CheckArgument(key);

            try
            {
                __lock.AcquireReaderLock(DEFAULT_READ_LOCK_TIMEOUT);
                try
                {
                    HashSet<V> set = null;
                    if (TryGetValue(key, out set))
                    {
                        return set;
                    }
                }
                finally
                {
                    __lock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException e)
            {
                LogUtils.Warn(e);
            }
            
            return null;
        }
    }
}
