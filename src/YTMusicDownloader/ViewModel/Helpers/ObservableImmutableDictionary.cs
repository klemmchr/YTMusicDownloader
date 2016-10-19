using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YTMusicDownloader.ViewModel.Helpers
{
    public class ObservableImmutableDictionary<T, V> : ObservableCollectionObject, IImmutableDictionary<T, V>,
        IReadOnlyDictionary<T, V>, IReadOnlyCollection<KeyValuePair<T, V>>, IDictionary<T, V>,
        ICollection<KeyValuePair<T, V>>, IEnumerable<KeyValuePair<T, V>>, IDictionary, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        #region Private

        private ImmutableDictionary<T, V> _items;

        #endregion Private

        #region Constructors

        public ObservableImmutableDictionary() : this(new KeyValuePair<T, V>[0], LockTypeEnum.SpinWait)
        {
        }

        public ObservableImmutableDictionary(IEnumerable<KeyValuePair<T, V>> items) : this(items, LockTypeEnum.SpinWait)
        {
        }

        public ObservableImmutableDictionary(LockTypeEnum lockType) : this(new KeyValuePair<T, V>[0], lockType)
        {
        }

        public ObservableImmutableDictionary(IEnumerable<KeyValuePair<T, V>> items, LockTypeEnum lockType)
            : base(lockType)
        {
            SyncRoot = new object();
            _items = ImmutableDictionary<T, V>.Empty.AddRange(items);
        }

        #endregion Constructors

        #region Thread-Safe Methods

        #region General

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryOperation(Func<ImmutableDictionary<T, V>, ImmutableDictionary<T, V>> operation)
        {
            return TryOperation(operation, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DoOperation(Func<ImmutableDictionary<T, V>, ImmutableDictionary<T, V>> operation)
        {
            return DoOperation(operation, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryOperation(Func<ImmutableDictionary<T, V>, ImmutableDictionary<T, V>> operation,
            NotifyCollectionChangedEventArgs args)
        {
            try
            {
                if (TryLock())
                {
                    var oldList = _items;
                    var newItems = operation(oldList);

                    if (newItems == null)
                        return false;

                    _items = newItems;

                    if (args != null)
                        RaiseNotifyCollectionChanged(args);
                    return true;
                }
            }
            finally
            {
                Unlock();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryOperation(
            Func<ImmutableDictionary<T, V>, KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>>
                operation)
        {
            try
            {
                if (TryLock())
                {
                    var oldList = _items;
                    var kvp = operation(oldList);
                    var newItems = kvp.Key;
                    var args = kvp.Value;

                    if (newItems == null)
                        return false;

                    _items = newItems;

                    if (args != null)
                        RaiseNotifyCollectionChanged(args);
                    return true;
                }
            }
            finally
            {
                Unlock();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoOperation(Func<ImmutableDictionary<T, V>, ImmutableDictionary<T, V>> operation,
            NotifyCollectionChangedEventArgs args)
        {
            bool result;

            try
            {
                Lock();
                var oldItems = _items;
                var newItems = operation(_items);

                if (newItems == null)
                    return false;

                result = (_items = newItems) != oldItems;

                if (args != null)
                    RaiseNotifyCollectionChanged(args);
            }
            finally
            {
                Unlock();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoOperation(
            Func<ImmutableDictionary<T, V>, KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>>
                operation)
        {
            bool result;

            try
            {
                Lock();
                var oldItems = _items;
                var kvp = operation(_items);
                var newItems = kvp.Key;
                var args = kvp.Value;

                if (newItems == null)
                    return false;

                result = (_items = newItems) != oldItems;

                if (args != null)
                    RaiseNotifyCollectionChanged(args);
            }
            finally
            {
                Unlock();
            }

            return result;
        }

        #endregion Helpers

        #endregion General

        #region Specific

        public bool DoAdd(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                {
                    var kvp = valueProvider(currentItems);
                    var newItems = _items.Add(kvp.Key, kvp.Value);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kvp));
                }
            );
        }

        public bool DoAddRange(Func<ImmutableDictionary<T, V>, IEnumerable<KeyValuePair<T, V>>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                        currentItems.AddRange(valueProvider(currentItems))
            );
        }

        public bool DoClear()
        {
            return DoOperation
            (
                currentItems =>
                        currentItems.Clear()
            );
        }

        public bool DoRemove(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                {
                    var newKVP = valueProvider(currentItems);
                    var oldKVP = new KeyValuePair<T, V>(newKVP.Key, currentItems[newKVP.Key]);
                    var newItems = currentItems.Remove(newKVP.Key);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldKVP));
                }
            );
        }

        public bool DoRemoveRange(Func<ImmutableDictionary<T, V>, IEnumerable<T>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                        currentItems.RemoveRange(valueProvider(currentItems))
            );
        }

        public bool DoSetItem(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                {
                    var newKVP = valueProvider(currentItems);
                    var oldKVP = new KeyValuePair<T, V>(newKVP.Key, currentItems[newKVP.Key]);
                    var newItems = currentItems.SetItem(newKVP.Key, newKVP.Value);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldKVP, newKVP));
                }
            );
        }

        public bool DoSetItems(Func<ImmutableDictionary<T, V>, IEnumerable<KeyValuePair<T, V>>> valueProvider)
        {
            return DoOperation
            (
                currentItems =>
                        currentItems.SetItems(valueProvider(currentItems))
            );
        }

        public bool TryAdd(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                {
                    var kvp = valueProvider(currentItems);
                    var newItems = _items.Add(kvp.Key, kvp.Value);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kvp));
                }
            );
        }

        public bool TryAddRange(Func<ImmutableDictionary<T, V>, IEnumerable<KeyValuePair<T, V>>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                        currentItems.AddRange(valueProvider(currentItems))
            );
        }

        public bool TryClear()
        {
            return TryOperation
            (
                currentItems =>
                        currentItems.Clear()
            );
        }

        public bool TryRemove(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                {
                    var newKVP = valueProvider(currentItems);
                    var oldKVP = new KeyValuePair<T, V>(newKVP.Key, currentItems[newKVP.Key]);
                    var newItems = currentItems.Remove(newKVP.Key);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldKVP));
                }
            );
        }

        public bool TryRemoveRange(Func<ImmutableDictionary<T, V>, IEnumerable<T>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                        currentItems.RemoveRange(valueProvider(currentItems))
            );
        }

        public bool TrySetItem(Func<ImmutableDictionary<T, V>, KeyValuePair<T, V>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                {
                    var newKVP = valueProvider(currentItems);
                    var oldKVP = new KeyValuePair<T, V>(newKVP.Key, currentItems[newKVP.Key]);
                    var newItems = currentItems.SetItem(newKVP.Key, newKVP.Value);
                    return new KeyValuePair<ImmutableDictionary<T, V>, NotifyCollectionChangedEventArgs>(newItems,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldKVP, newKVP));
                }
            );
        }

        public bool TrySetItems(Func<ImmutableDictionary<T, V>, IEnumerable<KeyValuePair<T, V>>> valueProvider)
        {
            return TryOperation
            (
                currentItems =>
                        currentItems.SetItems(valueProvider(currentItems))
            );
        }

        #endregion Specific

        public ImmutableDictionary<T, V> ToImmutableDictionary()
        {
            return _items;
        }

        #region IEnumerable<KeyValuePair<T, V>>

        public IEnumerator<KeyValuePair<T, V>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<T>

        #endregion Thread-Safe Methods

        #region Non Thead-Safe Methods

        #region IImmutableDictionary<T, V>

        public IImmutableDictionary<T, V> Add(T key, V value)
        {
            _items = _items.Add(key, value);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public IImmutableDictionary<T, V> AddRange(IEnumerable<KeyValuePair<T, V>> pairs)
        {
            _items = _items.AddRange(pairs);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public IImmutableDictionary<T, V> Clear()
        {
            _items = _items.Clear();
            RaiseNotifyCollectionChanged();
            return this;
        }

        public bool Contains(KeyValuePair<T, V> pair)
        {
            return _items.Contains(pair);
        }

        public IImmutableDictionary<T, V> Remove(T key)
        {
            _items = _items.Remove(key);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public IImmutableDictionary<T, V> RemoveRange(IEnumerable<T> keys)
        {
            _items = _items.RemoveRange(keys);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public IImmutableDictionary<T, V> SetItem(T key, V value)
        {
            _items = _items.SetItem(key, value);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public IImmutableDictionary<T, V> SetItems(IEnumerable<KeyValuePair<T, V>> items)
        {
            _items = _items.SetItems(items);
            RaiseNotifyCollectionChanged();
            return this;
        }

        public bool TryGetKey(T equalKey, out T actualKey)
        {
            return _items.TryGetKey(equalKey, out actualKey);
        }

        public bool ContainsKey(T key)
        {
            return _items.ContainsKey(key);
        }

        public IEnumerable<T> Keys
        {
            get { return _items.Keys; }
        }

        public bool TryGetValue(T key, out V value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerable<V> Values
        {
            get { return _items.Values; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        #endregion IImmutableDictionary<T, V>

        #region IDictionary<T, V>

        void IDictionary<T, V>.Add(T key, V value)
        {
            Add(key, value);
        }

        ICollection<T> IDictionary<T, V>.Keys
        {
            get { return (_items as IDictionary<T, V>).Keys; }
        }

        bool IDictionary<T, V>.Remove(T key)
        {
            var oldItems = _items;
            var newItems = _items = oldItems.Remove(key);

            if (oldItems == newItems)
                return false;

            RaiseNotifyCollectionChanged();
            return true;
        }

        ICollection<V> IDictionary<T, V>.Values
        {
            get { return (_items as IDictionary<T, V>).Values; }
        }

        public V this[T key]
        {
            get { return _items[key]; }
            set
            {
                _items.SetItem(key, value);
                RaiseNotifyCollectionChanged();
            }
        }

        public void Add(KeyValuePair<T, V> item)
        {
            (_items as IDictionary<T, V>).Add(item);
            RaiseNotifyCollectionChanged();
        }

        void ICollection<KeyValuePair<T, V>>.Clear()
        {
            Clear();
        }

        public void CopyTo(KeyValuePair<T, V>[] array, int arrayIndex)
        {
            (_items as IDictionary<T, V>).CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<T, V> item)
        {
            var result = (_items as IDictionary<T, V>).Remove(item);
            RaiseNotifyCollectionChanged();
            return result;
        }

        #endregion IDictionary<T, V>

        #region IDictionary

        public void Add(object key, object value)
        {
            Add((T) key, (V) value);
        }

        void IDictionary.Clear()
        {
            Clear();
        }

        public bool Contains(object key)
        {
            return (_items as IDictionary).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (_items as IDictionary).GetEnumerator();
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get { return (_items as IDictionary).Keys; }
        }

        public void Remove(object key)
        {
            (_items as IDictionary).Remove(key);
            RaiseNotifyCollectionChanged();
        }

        ICollection IDictionary.Values
        {
            get { return (_items as IDictionary).Values; }
        }

        public object this[object key]
        {
            get { return this[(T) key]; }
            set { this[(T) key] = (V) value; }
        }

        public void CopyTo(Array array, int index)
        {
            (_items as IDictionary).CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot { get; }

        #endregion IDictionary

        #endregion Non Thead-Safe Methods
    }
}