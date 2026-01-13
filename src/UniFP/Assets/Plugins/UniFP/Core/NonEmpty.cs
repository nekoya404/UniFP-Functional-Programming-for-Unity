using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UniFP
{
    /// <summary>
    /// Non-empty collection: Guarantees at least one element
    /// </summary>
    public readonly struct NonEmpty<T> : IEnumerable<T>
    {
        private readonly T _head;
        private readonly List<T> _tail;


        /// <summary>First element (always exists)</summary>
        public T Head => _head;

        /// <summary>Remaining elements</summary>
        public IReadOnlyList<T> Tail => _tail;

        /// <summary>Total count (always >= 1)</summary>
        public int Count => 1 + _tail.Count;

        /// <summary>Last element</summary>
        public T Last => _tail.Count > 0 ? _tail[_tail.Count - 1] : _head;



        public NonEmpty(T head, params T[] tail)
        {
            _head = head;
            _tail = tail?.ToList() ?? new List<T>();
        }

        public NonEmpty(T head, IEnumerable<T> tail)
        {
            _head = head;
            _tail = tail?.ToList() ?? new List<T>();
        }

        public static NonEmpty<T> Single(T value) => new NonEmpty<T>(value);

        public static Option<NonEmpty<T>> FromEnumerable(IEnumerable<T> source)
        {
            var list = source?.ToList();
            return list != null && list.Count > 0
                ? Option<NonEmpty<T>>.Some(new NonEmpty<T>(list[0], list.Skip(1)))
                : Option<NonEmpty<T>>.None();
        }



        /// <summary>Transform all elements</summary>
        public NonEmpty<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return new NonEmpty<TResult>(
                mapper(_head),
                _tail.Select(mapper).ToArray()
            );
        }

        /// <summary>Add element at the end</summary>
        public NonEmpty<T> Append(T item)
        {
            var newTail = new List<T>(_tail) { item };
            return new NonEmpty<T>(_head, newTail);
        }

        /// <summary>Add element at the start</summary>
        public NonEmpty<T> Prepend(T item)
        {
            var newTail = new List<T> { _head };
            newTail.AddRange(_tail);
            return new NonEmpty<T>(item, newTail);
        }

        /// <summary>Concatenate with another non-empty</summary>
        public NonEmpty<T> Concat(NonEmpty<T> other)
        {
            var newTail = new List<T>(_tail) { other._head };
            newTail.AddRange(other._tail);
            return new NonEmpty<T>(_head, newTail);
        }

        /// <summary>Filter (may become empty, returns Option)</summary>
        public Option<NonEmpty<T>> Filter(Func<T, bool> predicate)
        {
            var filtered = this.Where(predicate).ToList();
            return filtered.Count > 0
                ? Option<NonEmpty<T>>.Some(new NonEmpty<T>(filtered[0], filtered.Skip(1)))
                : Option<NonEmpty<T>>.None();
        }

        /// <summary>Fold from left</summary>
        public TResult Fold<TResult>(TResult seed, Func<TResult, T, TResult> folder)
        {
            var result = folder(seed, _head);
            return _tail.Aggregate(result, folder);
        }

        /// <summary>Reduce (no seed needed)</summary>
        public T Reduce(Func<T, T, T> reducer)
        {
            return _tail.Aggregate(_head, reducer);
        }

        /// <summary>Reverse order</summary>
        public NonEmpty<T> Reverse()
        {
            var all = this.Reverse().ToList();
            return new NonEmpty<T>(all[0], all.Skip(1));
        }

        /// <summary>Get element at index</summary>
        public T this[int index]
        {
            get
            {
                if (index == 0) return _head;
                if (index > 0 && index <= _tail.Count) return _tail[index - 1];
                throw new IndexOutOfRangeException($"Index {index} out of range [0, {Count})");
            }
        }



        public IEnumerator<T> GetEnumerator()
        {
            yield return _head;
            foreach (var item in _tail)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public override string ToString()
        {
            var items = string.Join(", ", this.Take(3));
            var suffix = Count > 3 ? $", ... ({Count} total)" : "";
            return $"NonEmpty[{items}{suffix}]";
        }
    }

    /// <summary>
    /// NonEmpty factory methods
    /// </summary>
    public static class NonEmpty
    {
        public static NonEmpty<T> Create<T>(T head, params T[] tail) => new NonEmpty<T>(head, tail);
        public static NonEmpty<T> Single<T>(T value) => NonEmpty<T>.Single(value);
    }
}
