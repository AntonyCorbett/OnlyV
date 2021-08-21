using System;
using System.Collections.Generic;

namespace OnlyV.VerseExtraction.Utils
{
    internal class MostRecentlyUsedList<TKey, TValue>
    {
        private readonly Dictionary<TKey, Node> _items = new Dictionary<TKey, Node>();
        private readonly int _capacity;
        private Node _head;
        private Node _tail;

        public MostRecentlyUsedList(int capacity = 16)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than zero");
            }

            _capacity = capacity;
            _head = null;
        }

        public void Add(TKey key, TValue value)
        {
            if (!_items.TryGetValue(key, out var entry))
            {
                entry = new Node { Key = key, Value = value };
                if (_items.Count == _capacity)
                {
                    _items.Remove(_tail.Key);
                    _tail = _tail.Previous;

                    if (_tail != null)
                    {
                        _tail.Next = null;
                    }
                }

                _items.Add(key, entry);
            }

            entry.Value = value;
            MoveToHead(entry);

            if (_tail == null)
            {
                _tail = _head;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            if (!_items.TryGetValue(key, out var entry))
            {
                return false;
            }

            MoveToHead(entry);
            value = entry.Value;

            return true;
        }

        private void MoveToHead(Node entry)
        {
            if (entry == _head || entry == null)
            {
                return;
            }

            var next = entry.Next;
            var previous = entry.Previous;

            if (next != null)
            {
                next.Previous = entry.Previous;
            }

            if (previous != null)
            {
                previous.Next = entry.Next;
            }

            entry.Previous = null;
            entry.Next = _head;

            if (_head != null)
            {
                _head.Previous = entry;
            }

            _head = entry;

            if (_tail == entry)
            {
                _tail = previous;
            }
        }

        private class Node
        {
            public Node Next { get; set; }

            public Node Previous { get; set; }

            public TKey Key { get; set; }

            public TValue Value { get; set; }
        }
    }
}
