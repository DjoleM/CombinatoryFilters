﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeAndy.CombinatoryFilters
{
    public static class FilterNodeExtensions
    {
        public static InvertedFilter<TLeafNode> Invert<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode
        {
            return new InvertedFilter<TLeafNode>(filter);
        }

        // TODO: Maybe this can be made private/internal?
        public static Func<TItemToTest, bool> GetPredicate<TLeafNode, TItemToTest>(
            this IFilterNode<TLeafNode> filter,
            Func<TLeafNode, Func<TItemToTest, bool>> itemPredicate)
            where TLeafNode : class, ILeafFilterNode
        {
            return filter.Aggregate(
                Combine,
                Invert,
                itemPredicate);
        }

        /// <summary>
        /// Removes redundancies in a filter to give simplest expression
        /// </summary>
        /// <typeparam name="TLeafNode"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IFilterNode<TLeafNode> Collapse<TLeafNode>(this IFilterNode<TLeafNode> filter)
            where TLeafNode : class, ILeafFilterNode
        {
            return filter.Match(
                combinationFilter =>
                {
                    return combinationFilter.Operator.Match(
                        () =>
                        {
                            if (combinationFilter.Filters.Any(f => f.Equals(FilterNode<TLeafNode>.False)))
                            {
                                return FilterNode<TLeafNode>.False;
                            }

                            var nonTrivialFilters = combinationFilter.Filters.Where(f => !f.Equals(FilterNode<TLeafNode>.True));
                            var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, combinationFilter.Operator);
                            return collapsedCombinationFilter.Filters.Count == 1
                                ? collapsedCombinationFilter.Filters.Single()
                                : collapsedCombinationFilter;
                        },
                        () =>
                        {
                            if (combinationFilter.Filters.Any(f => f.Equals(FilterNode<TLeafNode>.True)))
                            {
                                return FilterNode<TLeafNode>.True;
                            }

                            var nonTrivialFilters = combinationFilter.Filters.Where(f => !f.Equals(FilterNode<TLeafNode>.False));
                            var collapsedCombinationFilter = new CombinationFilter<TLeafNode>(nonTrivialFilters, combinationFilter.Operator);
                            return collapsedCombinationFilter.Filters.Count == 1
                                ? collapsedCombinationFilter.Filters.Single()
                                : collapsedCombinationFilter;
                        });
                },
                invertedFilter =>
                {
                    if (invertedFilter.FilterToInvert is ICombinationFilterNode<TLeafNode> combinationInner && combinationInner.Filters.Count == 0)
                    {
                        var invertedCombinationOperation = combinationInner.Operator.Match(
                            () => CombinationOperator.Any, 
                            () => CombinationOperator.All);
                        return new CombinationFilter<TLeafNode>(new IFilterNode<TLeafNode>[0], invertedCombinationOperation);
                    }

                    return invertedFilter.FilterToInvert is IInvertedFilter<TLeafNode> invertedInner
                        ? invertedInner.FilterToInvert
                        : invertedFilter;
                },
                leafFilter => (IFilterNode<TLeafNode>)leafFilter); // TODO: Figure out a way to remove this evil cast?
        }

        internal static Func<TItemToTest, bool> Combine<TItemToTest>(
            IEnumerable<Func<TItemToTest, bool>> innerResults,
            CombinationOperator @operator)
        {
            Func<TItemToTest, bool> AllReducer()
                => relatedItemCollection => innerResults.All(comparator => comparator(relatedItemCollection));

            Func<TItemToTest, bool> AnyReducer()
                => relatedItemCollection => innerResults.Any(comparator => comparator(relatedItemCollection));

            return @operator.Match(AllReducer, AnyReducer);
        }

        internal static Func<TItemToTest, bool> Invert<TItemToTest>(Func<TItemToTest, bool> innerResult)
            => relatedItemCollection => !innerResult(relatedItemCollection);
    }
}