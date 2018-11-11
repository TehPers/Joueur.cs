using System;
using System.Collections.Generic;
using System.Text;

namespace Joueur.cs.Conflux.Convenience {
    public static class EnumerableExtensions {

        public static IEnumerable<T> Yield<T>(this T source) {
            yield return source;
        }
    }
}
