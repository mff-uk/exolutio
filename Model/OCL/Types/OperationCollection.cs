using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.SupportingClasses;

namespace Exolutio.Model.OCL.Types
{
    public class OperationCollection:ActionDictionary<string,OperationList>
    {

        protected Classifier owner;

        internal OperationCollection(Classifier owner)
        {
            this.owner = owner;
        }
        
        protected override void OnPreAdd(string key, OperationList value) {
            throw new InvalidOperationException();
        }

        protected override void OnPreDelete(string key) {
            throw new InvalidOperationException();
        }

        protected override void OnPreSet(string key, OperationList value) {
            throw new InvalidOperationException();
        }

        public void Add(Operation operation)
        {
            if (Data.ContainsKey(operation.Name) == false) {
                Data.Add(operation.Name, new OperationList(owner,operation.Name));
            }
            OperationList opsWithSameName = Data[operation.Name];
            opsWithSameName.Add(operation);
        }

        
    }

    public class OperationList:ActionList<Operation>
    {
        protected Classifier owner;
        protected string NameOfOperations;

        internal OperationList(Classifier owner, string operationName)
        {
            this.owner = owner;
            NameOfOperations = operationName;
        }

        protected override void OnPreDelete(Operation item)
        {
            throw new NotSupportedException();
        }

        protected override void OnPreSet(Operation item)
        {
            throw new NotSupportedException();
        }

        protected override void OnPreAdd(Operation item)
        {
            item.Owner = owner;

            if (item.Name != NameOfOperations) {
                System.Diagnostics.Debug.Fail("Try add operation with differnetn name to OperationList (it containts operations with same name but different signature.).");
            }

            if (Data.Any(it => it.Parametrs == item.Parametrs)) {
                throw new Exception("Tries add opretion with duplicite name and signature");
            }
        }

        protected override void OnAdded(Operation item)
        {
           
        }

        public Operation LookupOperation(IEnumerable<Classifier> paremetrs) {
            //Obsahuje chybu danou specifikaci
            foreach (Operation ops in Data) {
                if (ops.Parametrs.HasMatchingSignature(paremetrs)) {
                    return ops;
                }
            }
            return null;
        }
    }
}
