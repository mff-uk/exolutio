using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST {
    public interface IExpressionSource {
        bool IsFromCode {
            get;
        }

        int Line {
            get;
        }

        int Column {
            get;
        }
    }

    public class InvalidExpressionSource : IExpressionSource {

        #region IExpressionSource Members

        public bool IsFromCode {
            get { return false; }
        }

        public int Line {
            get { return 0; }
        }

        public int Column {
            get { return 0; }
        }

        #endregion
    }
}
