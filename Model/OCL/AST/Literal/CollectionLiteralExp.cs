using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A CollectionLiteralExp represents a reference to collection literal.
    /// </summary>
    public class CollectionLiteralExp : LiteralExp
    {

        public CollectionLiteralExp(Types.CollectionType type, List<CollectionLiteralPart> parts)
            : base(type) {
                this._type = type;
                this.Parts = parts;
        }

        /// <summary>
        /// The kind of collection literal that is specified by this CollectionLiteralExp.
        /// </summary>
        public CollectionKind Kind
        {
            get {
                return _type.CollectionKind;
            }
        }


        /// <summary>
        /// The parts of the collection literal expression.
        /// </summary>
        public List<CollectionLiteralPart> Parts
        {
            get;
            set;
        }

        Types.CollectionType _type;

        public override Types.Classifier Type {
            get {
                return _type;
            }   
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
}
