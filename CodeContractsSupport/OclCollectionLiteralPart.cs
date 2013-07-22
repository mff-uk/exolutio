using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    public abstract class OclCollectionLiteralPart : IEnumerable<OclAny>
    {
        public abstract IEnumerator<OclAny> GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator OclCollectionLiteralPart(OclAny o)
        {
            return new OclCollectionLiteralPartItem(o);
        }
    }

    public sealed class OclCollectionLiteralPartRange : OclCollectionLiteralPart
    {
        private readonly OclInteger from, to;
        public OclCollectionLiteralPartRange(OclInteger from, OclInteger to)
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
    public sealed class OclCollectionLiteralPartItem : OclCollectionLiteralPart
    {
        private readonly OclAny item;
        public OclCollectionLiteralPartItem(OclAny to)
        {
            this.item = to;
        }
        public override IEnumerator<OclAny> GetEnumerator()
        {
            yield return item;
        }
    }
}
