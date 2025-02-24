using System;
using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class CollapseTests
    {
        private readonly Random _random;
        private readonly RandomCharFilterGenerator _generator;

        public CollapseTests()
        {
            _random = new Random();
            _generator = new RandomCharFilterGenerator(_random);
        }

        [Fact]
        public void Collapse_FilterIsEquivalent()
        {
            var interestingCount = 0;
            while (interestingCount < 10)
            {
                var numSets = _random.Next(1000);

                var strings = Enumerable.Range(1, numSets).Select(_ => _generator.GetRandomString(_random.Next(RandomCharFilterGenerator.Chars.Length)))
                    .ToHashSet();

                const int nodeCount = 10;

                var filter = _generator.GetRandomFilter(nodeCount);
                var collapsedFilter = filter.Collapse();
                
                if (filter.Equals(collapsedFilter))
                {
                    // Trivial, ignore.
                    continue;
                }

                var expectedResult = strings.Where(filter.IsMatch);
                var actualResult = strings.Where(collapsedFilter.IsMatch);

                Assert.Equal(expectedResult, actualResult);
                interestingCount++;
            }
        }
    }
}
