using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST {
    public interface IAstVisitor<T> {
        T Visit(BooleanLiteralExp node);
    
        T Visit(CollectionLiteralExp node);
   
        T Visit(EnumLiteralExp node);
   
        T Visit(ErrorExp node);
   
        T Visit(IfExp node);
    
        T Visit(IntegerLiteralExp node);
  
        T Visit(InvalidLiteralExp node);
 
        T Visit(IterateExp node);

        T Visit(IteratorExp node);

        T Visit(LetExp node);

        T Visit(NullLiteralExp node);
  
        T Visit(OperationCallExp node);
   
        T Visit(PropertyCallExp node);
 
        T Visit(RealLiteralExp node);

        T Visit(StringLiteralExp node);

        T Visit(TupleLiteralExp node);

        T Visit(TypeExp node);

        T Visit(UnlimitedNaturalLiteralExp node);

        T Visit(VariableExp node);
    }

    public interface IAstVisitor {
        void Visit(BooleanLiteralExp node);

        void Visit(CollectionLiteralExp node);

        void Visit(EnumLiteralExp node);

        void Visit(ErrorExp node);

        void Visit(IfExp node);

        void Visit(IntegerLiteralExp node);

        void Visit(InvalidLiteralExp node);

        void Visit(IterateExp node);

        void Visit(IteratorExp node);

        void Visit(LetExp node);

        void Visit(NullLiteralExp node);

        void Visit(OperationCallExp node);

        void Visit(PropertyCallExp node);

        void Visit(RealLiteralExp node);

        void Visit(StringLiteralExp node);

        void Visit(TupleLiteralExp node);

        void Visit(TypeExp node);

        void Visit(UnlimitedNaturalLiteralExp node);

        void Visit(VariableExp node);
    }
}
