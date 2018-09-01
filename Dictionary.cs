using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ElectionBot
{
    [System.Serializable]
    public class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
    {
        public Dictionary() : base() { }

        public Dictionary(int capacity) : base(capacity) { }

        public Dictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        protected Dictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public void Put(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }
}
