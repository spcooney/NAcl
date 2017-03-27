using System;
using System.Collections.Generic;
using System.Text;

namespace NAcl
{
    public class ReverseComparer<TKey> : IComparer<TKey>
    {
        private IComparer<TKey> comparer;

        public ReverseComparer()
            : this(Comparer<TKey>.Default)
        {

        }

        public ReverseComparer(IComparer<TKey> comparer)
        {
            this.comparer = comparer;

        }

        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            return comparer.Compare(y, x);
        }

        #endregion
    }
}
