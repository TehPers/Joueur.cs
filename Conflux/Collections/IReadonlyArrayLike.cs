﻿using System.Collections.Generic;

namespace Joueur.cs.Conflux.Collections {
    public interface IReadonlyArrayLike<T> : IEnumerable<T> {
        /// <summary>The number of elements.</summary>
        int Length { get; }

        /// <summary>Selects an element.</summary>
        /// <param name="index">The index of the element to select.</param>
        /// <returns>The selected element.</returns>
        T this[Index index] { get; }
    }
}