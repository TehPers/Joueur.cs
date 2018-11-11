using System;

namespace Joueur.cs.Conflux.Responsive {
    internal class WhereResponsiveOperator<T> : ResponsiveValue<T> {
        public WhereResponsiveOperator(ResponsiveValue<T> source, Func<T, bool> predicate) : base(predicate(source.Value) ? source.Value : default) {
            source.ValueChanged += newValue => {
                if (predicate(newValue)) {
                    this.Value = newValue;
                }
            };
        }
    }
}