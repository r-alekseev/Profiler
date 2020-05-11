using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;


namespace Profiler.PerformanceTests
{
    public class TestChainEqualityComparerDecorator : IEqualityComparer<string[]>
    {
        private readonly IEqualityComparer<string[]> _comparer;

        private long _getHashCodeCounter;
        private long _equalsCounter;

        private Stopwatch _getHashCodeStopwatch = new Stopwatch();
        private Stopwatch _equalsStopwatch = new Stopwatch();

        public TestChainEqualityComparerDecorator(IEqualityComparer<string[]> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public long GetHashCodeCounter => _getHashCodeCounter;
        public long EqualsCounter => _equalsCounter;

        public long GetHashCodeMs => _getHashCodeStopwatch.ElapsedMilliseconds;
        public long EqualsMs => _equalsStopwatch.ElapsedMilliseconds;

        public int GetHashCode(string[] chain)
        {
            Interlocked.Increment(ref _getHashCodeCounter);
            
            _getHashCodeStopwatch.Start();
            int result = _comparer.GetHashCode(chain);
            _getHashCodeStopwatch.Stop();

            return result;
        }

        public bool Equals(string[] chainX, string[] chainY)
        {
            Interlocked.Increment(ref _equalsCounter);

            _equalsStopwatch.Start();
            bool result = _comparer.Equals(chainX, chainY);
            _equalsStopwatch.Stop();

            return result;
        }
    }

    public class ChainEqualityComparerTests
    {
        private readonly ITestOutputHelper _output;

        public ChainEqualityComparerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void ChainEqualityComparer_WithDecorator_Test(TestChainEqualityComparerDecorator comparer, List<string> log, int count, int length, Func<int, string> getNext)
        {
            var hashSet = new HashSet<string[]>(comparer);

            for(int i = 0; i < count; i ++)
            {
                string[] chain = new string[length];

                for(int j = 0; j < length; j++)
                {
                    string next = getNext(i);

                    chain[j] = next;
                }

                hashSet.Add(chain);
            }

            log.Add(comparer.GetHashCodeCounter.ToString());
            log.Add(comparer.EqualsCounter.ToString());
            log.Add(comparer.GetHashCodeMs.ToString());
            log.Add(comparer.EqualsMs.ToString());
        }

        private void ChainEqualityComparer_WithDecorator_Tests(TestChainEqualityComparerDecorator comparer, int count, int lengthFrom, long lengthTo)
        {
            for(int length = lengthFrom; length <= lengthTo; length++)
            {

                var log = new List<string>();
                log.Add(count.ToString());
                log.Add(length.ToString());

                log.Add("\tequal:");
                // equal chains
                ChainEqualityComparer_WithDecorator_Test(comparer, log, count, length, i => "an item of the chain");

                log.Add("\trandom:");
                // random chains
                var random = new Random();
                ChainEqualityComparer_WithDecorator_Test(comparer, log, count, length, i => random.Next(0, 1_000_000).ToString());

                log.Add("\tpermut:");
                // permutation chains
                var keys = GenerateKeys(count, length, random);
                ChainEqualityComparer_WithDecorator_Test(comparer, log, count, length, i => keys[i][random.Next(0, length - 1)]);

                string line = string.Join('|', log);

                _output.WriteLine(line);
                Console.WriteLine(line);
            }
        }

        [Theory]
        [InlineData(100_000, 1, 15)]
        [InlineData(1_000_000, 1, 15)]
        public void ChainEqualityComparer_Tests(int count, int lengthFrom, long lengthTo)
        {
            var comparer = new TestChainEqualityComparerDecorator(new ChainEqualityComparer());

            ChainEqualityComparer_WithDecorator_Tests(comparer,  count, lengthFrom, lengthTo);
        }

        private List<string>[] GenerateKeys(int count, int length, Random random)
        {
            var space = new List<string>[count];
            for(int i = 0; i < count; i++)
            {
                space[i] = new List<string>();
                for(int j = 0; j < length; j++)
                {
                    space[i].Add(j.ToString());
                }
            }

            var keys = new List<string>[count];
            for(int i = 0; i < count; i++)
            {
                keys[i] = new List<string>();
                for(int j = 0; j < length; j++)
                {
                    int localLength = space[i].Count;
                    int randomIndex = random.Next(0, localLength);
                    keys[i].Add(space[i][randomIndex]);
                    space[i][randomIndex] = space[i][localLength - 1];
                    space[i].RemoveAt(localLength - 1);
                }
            }

            return keys;
        }
    }
}
