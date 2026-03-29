// Stub implementations for C5 Generic Collection Library.
// Used by UnitySteer components for spatial data structures.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace C5
{
    // C5.IList<T> is a superset of System.Collections.Generic.IList<T>
    public interface IList<T> : System.Collections.Generic.IList<T>
    {
        T First { get; }
        T Last { get; }
        void AddFirst(T item);
        void AddLast(T item);
        bool FindOrAdd(ref T item);
        IList<T> Slide(int offset, int count);
        IList<T> View(int index, int count);
    }

    public interface ICollection<T> : System.Collections.Generic.ICollection<T>
    {
        new bool Add(T item);
        bool FindOrAdd(ref T item);
        bool Remove(T item);
    }

    public class HashSet<T> : IEnumerable<T>
    {
        public int Count => 0;
        public void Add(T item) { }
        public void Clear() { }
        public bool Contains(T item) => false;
        public bool Remove(T item) => false;
        public IEnumerator<T> GetEnumerator() => System.Linq.Enumerable.Empty<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // ICollection<T> in C5 has a different Add signature than System.Collections.Generic
    // We expose a simpler stub to avoid conflict
    public class TreeSet<T> : IEnumerable<T>
    {
        public int Count => 0;
        public void Add(T item) { }
        public bool Contains(T item) => false;
        public bool Remove(T item) => false;
        public void Clear() { }
        public IEnumerator<T> GetEnumerator() => System.Linq.Enumerable.Empty<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class ArrayList<T> : IList<T>
    {
        private readonly List<T> _inner = new List<T>();
        public T this[int index] { get => _inner[index]; set => _inner[index] = value; }
        public int Count => _inner.Count;
        public bool IsReadOnly => false;
        public T First => Count > 0 ? _inner[0] : default;
        public T Last => Count > 0 ? _inner[Count - 1] : default;
        public void Add(T item) => _inner.Add(item);
        public void AddFirst(T item) => _inner.Insert(0, item);
        public void AddLast(T item) => _inner.Add(item);
        public void AddAll(IEnumerable<T> items) { foreach (var item in items) _inner.Add(item); }
        // Generic overload: list.AddAll<T>(collection) as used in RadarPing.cs.
        public void AddAll<U>(IEnumerable<U> items) where U : T { foreach (var item in items) _inner.Add(item); }
        public void Clear() => _inner.Clear();
        public bool Contains(T item) => _inner.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public bool FindOrAdd(ref T item) { if (_inner.Contains(item)) return true; _inner.Add(item); return false; }
        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        public int IndexOf(T item) => _inner.IndexOf(item);
        public void Insert(int index, T item) => _inner.Insert(index, item);
        public bool Remove(T item) => _inner.Remove(item);
        public void RemoveAt(int index) => _inner.RemoveAt(index);
        public IList<T> Slide(int offset, int count) => new ArrayList<T>();
        public IList<T> View(int index, int count) => new ArrayList<T>();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // GuardedList<T>: read-only wrapper for C5 lists, used in Radar.cs
    public class GuardedList<T> : IList<T>
    {
        private readonly List<T> _inner = new List<T>();
        public GuardedList() { }
        public GuardedList(IList<T> inner) { if (inner != null) foreach (var item in inner) _inner.Add(item); }
        public T this[int index] { get => _inner[index]; set { } }
        public int Count => _inner.Count;
        public bool IsReadOnly => true;
        public T First => Count > 0 ? _inner[0] : default;
        public T Last => Count > 0 ? _inner[Count - 1] : default;
        public void Add(T item) { }
        public void AddFirst(T item) { }
        public void AddLast(T item) { }
        public void Clear() { }
        public bool Contains(T item) => _inner.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public bool FindOrAdd(ref T item) => false;
        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        public int IndexOf(T item) => _inner.IndexOf(item);
        public void Insert(int index, T item) { }
        public bool Remove(T item) => false;
        public void RemoveAt(int index) { }
        public IList<T> Slide(int offset, int count) => new GuardedList<T>();
        public IList<T> View(int index, int count) => new GuardedList<T>();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
