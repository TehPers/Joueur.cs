using System.Collections.Generic;

namespace Joueur.cs.Conflux.Collections {
    public interface ISliceableList<T> : IList<T>, ISliceable<T> { }
}