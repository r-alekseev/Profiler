using System;

namespace Profiler
{
    internal class CollectionKey
    {
        private readonly string[] _chain;

        public CollectionKey(string[] chain)
        {
            _chain = chain;
        }

        // the hash code of collection 
        //  is the unchecked sum of collection item hash codes 
        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                for (int i = 0; i < _chain.Length; i++)
                {
                    hashCode += _chain[i].GetHashCode();
                }
            }
            return hashCode;
        }

        // collections are equals 
        //  if all items are equals
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var collectionKey = obj as CollectionKey;

            if (collectionKey == null)
                return false;

            return collectionKey.ItemsEquals(_chain);
        }

        internal bool ItemsEquals(string[] items)
        {
            if (_chain.Length != items.Length)
                return false;

            for (int i = items.Length - 1; i > -1; i--)
            {
                if (_chain[i].Length != items[i].Length)
                    return false;
            }

            for (int i = items.Length - 1; i > -1; i--)
            {
                if (!string.Equals(_chain[i], items[i]))
                    return false;
            }

            return true;
        }
    }
}
