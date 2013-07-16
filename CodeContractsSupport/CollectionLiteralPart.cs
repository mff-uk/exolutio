using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    public abstract class CollectionLiteralPart : IEnumerable<OclAny>
    {
        public abstract IEnumerator<OclAny> GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator CollectionLiteralPart(OclAny o)
        {
            return new CollectionLiteralPartItem(o);
        }
    }

    public sealed class CollectionLiteralPartRange : CollectionLiteralPart
    {
        OclInteger from, to;
        public CollectionLiteralPartRange(OclInteger from, OclInteger to)
        {
            this.from = from;
            this.to = to;
        }

        public override IEnumerator<OclAny> GetEnumerator()
        {
            for (int i = (int)from; i <= (int)to; ++i)
            {
                yield return (OclInteger)i;
            }
        }
    }
    public sealed class CollectionLiteralPartItem : CollectionLiteralPart
    {
        OclAny item;
        public CollectionLiteralPartItem(OclAny to)
        {
            this.item = to;
        }
        public override IEnumerator<OclAny> GetEnumerator()
        {
            yield return item;
        }
    }
}
