using System.Linq;
using Xunit;

namespace ExtremeAndy.CombinatoryFilters.Tests
{
    public class GetPredicateTests
    {
        [Fact]
        public void LeafItemFilter_GetPredicate_FiltersToExpectedResults()
        {
            var filter = new NumericRangeFilter(5, 10);
            var values = new[] {1, 3, 5, 9, 11};
            var expectedFilteredValues = new[] {5, 9};

            var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
            var filteredValues = values.Where(filterPredicate);

            Assert.Equal(expectedFilteredValues, filteredValues);
        }

        [Fact]
        public void CombinationFilter_AndOperator_GetPredicate_FiltersToExpectedResults()
        {
            var filter1 = new NumericRangeFilter(5, 10);
            var filter2 = new NumericRangeFilter(8, 15);
            var filter = new CombinationFilter<NumericRangeFilter>(new [] { filter1, filter2 });
            var values = new[] { 1, 3, 5, 9, 11 };
            var expectedFilteredValues = new[] { 9 };

            var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
            var filteredValues = values.Where(filterPredicate);

            Assert.Equal(expectedFilteredValues, filteredValues);
        }

        [Fact]
        public void CombinationFilter_OrOperator_GetPredicate_FiltersToExpectedResults()
        {
            var filter1 = new NumericRangeFilter(5, 10);
            var filter2 = new NumericRangeFilter(8, 15);
            var filter = new CombinationFilter<NumericRangeFilter>(new[] { filter1, filter2 }, CombinationOperator.Any);
            var values = new[] { 1, 3, 5, 9, 11 };
            var expectedFilteredValues = new[] { 5, 9, 11 };

            var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
            var filteredValues = values.Where(filterPredicate);

            Assert.Equal(expectedFilteredValues, filteredValues);
        }

        [Fact]
        public void ComplexFilter_GetPredicate_FiltersToExpectedResults()
        {
            var filter5To10 = new NumericRangeFilter(5, 10);
            var filter8To15 = new NumericRangeFilter(8, 15);
            var filter5To10And8To15 = new CombinationFilter<NumericRangeFilter>(new[] { filter5To10, filter8To15 }, CombinationOperator.Any);
            var filter9To12 = new NumericRangeFilter(9, 12);
            var filter = new CombinationFilter<NumericRangeFilter>(new IFilterNode<NumericRangeFilter>[] { filter5To10And8To15, filter9To12 }, CombinationOperator.All);
            var values = new[] { 1, 3, 5, 9, 11 };
            var expectedFilteredValues = new[] { 9, 11 };

            var filterPredicate = filter.GetPredicate<NumericRangeFilter, int>();
            var filteredValues = values.Where(filterPredicate);

            Assert.Equal(expectedFilteredValues, filteredValues);
        }
    }
}
