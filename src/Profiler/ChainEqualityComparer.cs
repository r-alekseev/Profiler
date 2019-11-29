using System.Collections.Generic;

namespace Profiler
{
    // the ability to store chains of collection of string as a key of a dictionary
    public class ChainEqualityComparer : IEqualityComparer<string[]>
    {
        // the hash code of a collection 
        //  is the unchecked sum of the collection item hash codes 
        public int GetHashCode(string[] chain)
        {
            int hashCode = 0;
            unchecked
            {
                for (int i = 0; i < chain.Length; i++)
                {
                    hashCode += chain[i].GetHashCode();
                }
            }
            return hashCode;
        }

        // collections are equals 
        //  if all items are equals
        public bool Equals(string[] chainX, string[] chainY)
        {
            if (chainX == null || chainY == null)
            {
                return false;
            }

            if (chainX.Length != chainY.Length)
            {
                return false;
            }

            for (int i = chainX.Length - 1; i > -1; i--)
            {
                if (chainX[i].Length != chainY[i].Length)
                {
                    return false;
                }
            }

            for (int i = chainX.Length - 1; i > -1; i--)
            {
                if (!string.Equals(chainX[i], chainY[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
