﻿using System.Collections.Generic;

namespace Joueur.cs.Conflux.Matching {
    public static class MatchExtensions {

        /// <summary>Maps this value to another value through matching operators.</summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source object being matched.</param>
        /// <returns>An operator to perform matches on the source to generate a final result.</returns>
        public static MatchOperator<TSource, TResult> Match<TSource, TResult>(this TSource source) {
            return new MatchOperator<TSource, TResult>(source);
        }

        /// <summary>Maps each value in this <see cref="IEnumerable{T}"/> to another value through matching operators.</summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source objects being matched.</param>
        /// <returns>An operator to perform matches on the sources to generate final results.</returns>
        public static MatchAllOperator<TSource, TResult> MatchAll<TSource, TResult>(this IEnumerable<TSource> source) {
            return new MatchAllOperator<TSource, TResult>(source);
        }
    }
}
