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
}
