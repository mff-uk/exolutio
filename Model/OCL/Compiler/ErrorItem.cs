using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

namespace Exolutio.Model.OCL.Compiler {
    public class ErrorItem {
        public string Text {
            protected set;
            get;
        }

        public ErrorItem(string text) {
            Text = text;
        }

        public override string ToString() {
            return Text;
        }
    }

    public class CodeErrorItem:ErrorItem {
        public IToken StartToken {
            protected set;
            get;
        }
        public IToken EndToken {
            protected set;
            get;
        }

        public CodeErrorItem(string text, IToken startToken, IToken endToken):base(text){
            StartToken = startToken;
            EndToken = endToken;
        }

        public override string ToString() {
            return string.Format("{0} (Line:{1}, Col:{2})",Text,StartToken != null?StartToken.Line:0,StartToken != null?StartToken.CharPositionInLine:0);
        }
    }
}
