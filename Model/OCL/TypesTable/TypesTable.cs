using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.TypesTable
{
    public class TypesTable
    {
        private Dictionary<Classifier, TypeRecord> table = new Dictionary<Classifier, TypeRecord>();
        private List<TypeRecord> matrix = new List<TypeRecord>();
        private BitArray markingTemp = null;
        private int[] distanceTemp = null;

        public bool RegisterType(Classifier type)
        {
            if (type == null)
                throw new NullReferenceException("Type is null.");

            TypeRecord newTypeRecord = CreateTypeRecord(type);
            type.TypeTable = this;

            if (type is ICompositeType)
            {
                RegisterCompositeType(type as ICompositeType);
            }

            if (newTypeRecord == null)
                return false;

            CreateEdgeForRecord(newTypeRecord);

            return true;
        }

        public void RegisterCompositeType(ICompositeType composit)
        {
            composit.RegistredComposite(this);
        }

        private TypeRecord CreateTypeRecord(Classifier type)
        {
            if (table.ContainsKey(type))
                return null;

            TypeRecord newTypeRecord = new TypeRecord();
            newTypeRecord.Type = type;
            newTypeRecord.MatrixIndex = matrix.Count;

            matrix.Add(newTypeRecord);
            table.Add(type, newTypeRecord);

            return newTypeRecord;
        }

        private void CreateEdgeForRecord(TypeRecord rec)
        {
            foreach (TypeRecord old in matrix)
            {
                if (rec.Type.ConformsToRegister(old.Type))
                    rec.EdgesIndex.Add(old.MatrixIndex);
                if (old.Type.ConformsToRegister(rec.Type))
                    old.EdgesIndex.Add(rec.MatrixIndex);
            }
        }



        public bool ConformsTo(Classifier left, Classifier right)
        {
            if (left is ICompositeType || right is IConformsToComposite)
                return ResolveComposite(left, right);

            //realokace makingTemp na velikost matrix.count
            PrepareMarkingTemp();

            int leftIndex = table[left].MatrixIndex;
            int rightIndex = table[right].MatrixIndex;

            Queue<int> toFind = new Queue<int>();
            toFind.Enqueue(leftIndex);

            while (toFind.Count > 0)
            {
                int actIndex = toFind.Dequeue();
                if (markingTemp[actIndex])
                    continue;
                markingTemp[actIndex] = true;

                if (actIndex == rightIndex)
                    return true;

                foreach (int newIndex in matrix[actIndex].EdgesIndex)
                {
                    if (markingTemp[newIndex] == false)
                        toFind.Enqueue(newIndex);
                }
            }

            return false;

        }

        public bool ResolveComposite(Classifier left, Classifier right)
        {
            //left or right must be composite
            if (left is ICompositeType)
            {
                ICompositeType leftComposite = left as ICompositeType;
                if (right is ICompositeType)
                    return leftComposite.ConformsToComposite(right);
                else
                    return leftComposite.ConformsToSimple(right);
            }
            else
            {
                // right must be composite
                if (left is IConformsToComposite)
                    return ((IConformsToComposite)left).ConformsToComposite(right);
                else
                    return false;
            }
        }

        private void PrepareMarkingTemp()
        {
            if (markingTemp == null)
            {
                markingTemp = new BitArray(matrix.Count);
                return;
            }

            if (markingTemp.Length != matrix.Count)
                markingTemp = new BitArray(matrix.Count);//default value is False
            else
                markingTemp.SetAll(false);
        }

        public Classifier CommonSuperType(Classifier typeA, Classifier typeB)
        {
            PrepareDistanceTemp();
            PrepareMarkingTemp();

            int typeAIndex = table[typeA].MatrixIndex;

            Queue<Tuple<int, int>> toFindFromA = new Queue<Tuple<int, int>>();
            toFindFromA.Enqueue(new Tuple<int, int>(typeAIndex, 0));
            //marking distance from TypeA
            while (toFindFromA.Count > 0)
            {
                Tuple<int, int> act = toFindFromA.Dequeue();
                int actIndex = act.Item1;
                int actDistanc = act.Item2;

                if (distanceTemp[actIndex] <= actDistanc)
                    continue;
                distanceTemp[actIndex] = actDistanc;
                foreach (int newIndex in matrix[actIndex].EdgesIndex)
                    if (distanceTemp[newIndex] > actDistanc + 1)
                        toFindFromA.Enqueue(new Tuple<int, int>(newIndex, actDistanc + 1));
            }

            int minDistance = int.MaxValue;
            Classifier commonSuperType = null;
            int typeBIndex = table[typeB].MatrixIndex;

            Queue<Tuple<int, int>> toFindFromB = new Queue<Tuple<int, int>>();
            toFindFromB.Enqueue(new Tuple<int, int>(typeBIndex, 0));
            //find the shortiest cast path from A,B to C
            while (toFindFromB.Count > 0)
            {
                Tuple<int, int> act = toFindFromB.Dequeue();
                int actIndex = act.Item1;
                int actDistanc = act.Item2;

                if (markingTemp[actIndex])
                    continue;
                else
                    markingTemp[actIndex] = true;

                if (minDistance > distanceTemp[actIndex] + actDistanc && distanceTemp[actIndex]< Int32.MaxValue)
                {
                    minDistance = distanceTemp[actIndex];
                    commonSuperType = matrix[actIndex].Type;
                }

                foreach (int newIndex in matrix[actIndex].EdgesIndex)
                    if (markingTemp[newIndex] == false)
                        toFindFromB.Enqueue(new Tuple<int, int>(newIndex, actDistanc + 1));
            }

            return commonSuperType;
        }

        private void PrepareDistanceTemp()
        {
            if (distanceTemp == null)
                distanceTemp = new int[matrix.Count];

            if (distanceTemp.Length != matrix.Count)
                distanceTemp = new int[matrix.Count];

            for (int i = 0; i < matrix.Count; i++)
                distanceTemp[i] = Int32.MaxValue;
        }
    }
}
