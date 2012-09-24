using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida urcena k vytvoreni konecneho automatu. 
     **/
    class FiniteAutomatUtils
    {

        private static int stateCount = 0;
        private static AutomatEdgeComparer automatEdgeComparer = new AutomatEdgeComparer();
        private static AutomatStateComparer automatStateComparer = new AutomatStateComparer();
        private static StateNameComparer stateNameComparer = new StateNameComparer();
        private const String stateDesignation = "s";

        private AutomatState currentStartState;

        private Dictionary<String, HashSet<AutomatState>> wordToState;
        private HashSet<StateWordPair> uncompleteTransitionsBuffer;
        private List<AutomatState> states;
        private String leftSideOfRule;
        private CreationPhase mode;
        private List<AutomatState> leftSideOfRecursionStates = null;
        private HashSet<AutomatState> firstForRecursion = null;
        private bool firstForRecursionFound = false;
        private bool compulsoryAssociationOnLeftSideOfRecursion = true;
        private bool compulsoryAssociationOnRightSideOfRecursion = true;
        private List<String> allLeftSideNames = null;
        private Dictionary<StateWordPair, HashSet<AutomatEdge>> generalTransitFunction;
        private Dictionary<string, AttributeType> leftSideNameAtributeType;

        public FiniteAutomatUtils()
        {
            wordToState = new Dictionary<string, HashSet<AutomatState>>();
            stateCount = 0;
        }

        /**
         * Pro danou asociaci topAssociation vytvori konecny automat.
         * 
         * atribut leftSideNames urcuje nazvy levych stran prepisovacich pravidel, pomoci kterych se bude prechazet ve vracenem konecnem automatu
         * atribut leftSideOfRule urcuje nazev leve strany pravidla pro asociaci topAssociation
         * atribut automatforestStatesTransitions je vytvoreny konecny automat, ktery budeme vracet
         * atribut automatWordToState pro nazev asociace vraci stavy, ktere jsou pocatecnimi stavy automatu, ktery takovy element mohou prijmout
         **/
        public void createFiniteAutomatForAssociation(PSMAssociation topAssociation, List<String> leftSideNames, String leftSideOfRule, ref Dictionary<StateWordPair, HashSet<AutomatEdge>> automatforestStatesTransitions, ref Dictionary<String, HashSet<AutomatState>> automatWordToState)
        {
            this.leftSideOfRule = leftSideOfRule;
            uncompleteTransitionsBuffer = new HashSet<StateWordPair>();
            states = new List<AutomatState>();
            mode = CreationPhase.ATTRIBUTE;
            leftSideOfRecursionStates = null;

            leftSideNameAtributeType = new Dictionary<string, AttributeType>();

            Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction = new Dictionary<StateWordPair, HashSet<AutomatEdge>>();
            generalTransitFunction = transitFunction;
            AutomatState startState = new AutomatState(stateDesignation + stateCount, true, null);
            currentStartState = startState;
            HashSet<AutomatState> actualStates = new HashSet<AutomatState>() { startState };
            states.Add(startState);

            if (!wordToState.ContainsKey(topAssociation.Name))
            {
                wordToState.Add(topAssociation.Name, new HashSet<AutomatState>());
            }
            wordToState[topAssociation.Name].Add(startState);

            stateCount++;

            firstForRecursion = null;
            firstForRecursionFound = false;
            compulsoryAssociationOnLeftSideOfRecursion = true;
            compulsoryAssociationOnRightSideOfRecursion = true;
            allLeftSideNames = leftSideNames;

            int index = 0;
            // topAssociation je pojmenovana asociace do content modelu
            if (topAssociation.Child is PSMContentModel)
            {
                mode = CreationPhase.CONTENT_MODEL;
                uint lower = topAssociation.Lower;
                UnlimitedInt upper = topAssociation.Upper;
                topAssociation.Upper = 1;
                topAssociation.Lower = 1;
                actualStates.UnionWith(createTransitionsForContentModel(topAssociation, startState, leftSideNames, transitFunction, uncompleteTransitionsBuffer));
                topAssociation.Lower = lower;
                topAssociation.Upper = upper;
                if (actualStates.Count > 0)
                    index++;
            }
            else
            {
                // urcime, zda topAssociation ovlivnuje nejaka rekurze a pokud ano, tak jestli je jeji prava i leva strana povinna
                bool leftSide = true;
                bool foundCompulsaryAssociation = false;
                foreach (PSMAssociation association in AssociationsUtils.namedAssociations(topAssociation, true, false))
                {
                    if (association != null)
                        if (association.IsNonTreeAssociation)
                        {
                            leftSide = false;
                            compulsoryAssociationOnLeftSideOfRecursion = foundCompulsaryAssociation;
                            foundCompulsaryAssociation = false;
                        }
                        else
                        {
                            if (association.Lower > 0)
                                foundCompulsaryAssociation = true;
                        }

                }
                if (!leftSide)
                    compulsoryAssociationOnRightSideOfRecursion = foundCompulsaryAssociation;
                foreach (PSMAssociation association in AssociationsUtils.getAllAssociations(topAssociation))
                {
                    if (association == null)
                    {
                        if (mode == CreationPhase.ATTRIBUTE)
                            mode = CreationPhase.ELEMENT_ATTRIBUTE;
                        else
                            mode = CreationPhase.ELEMENT;
                    }
                    else
                    {
                        if (mode == CreationPhase.ELEMENT_ATTRIBUTE) {
                            AutomatState attState = new AutomatState(stateDesignation + stateCount, true, null);
                            attState.exitState = true;
                            attState.LeftSide = leftSideNames[index];
                            attState.startState = startState;
                            stateCount++;
                            states.Add(attState);
                            if (!wordToState.ContainsKey(association.Name))
                            {
                                wordToState.Add(association.Name, new HashSet<AutomatState>());
                            }
                            wordToState[association.Name].Add(attState);

                            leftSideNameAtributeType.Add(leftSideNames[index], ((PSMClass)association.Child).PSMAttributes[0].AttributeType);
                        }
                        HashSet<AutomatState> previousStates = actualStates;
                        actualStates = new HashSet<AutomatState>();
                        if (association.Child is PSMContentModel && !association.IsNamed)
                        {
                            // asociace vede do contentModelu
                            int indexIteration = 0;
                            indexIteration = AssociationsUtils.namedAssociations(association, mode != CreationPhase.ATTRIBUTE && mode != CreationPhase.ELEMENT_ATTRIBUTE).Count;
                            foreach (AutomatState actualState in previousStates)
                            {
                                actualStates.UnionWith(createTransitionsForContentModel(association, actualState, ListUtils.removeFromStartOfList(leftSideNames, index), transitFunction, uncompleteTransitionsBuffer));
                            }
                            index += indexIteration;
                        }
                        else
                        {
                            if (!association.IsNamed && association.IsNonTreeAssociation)
                            {
                                // asociace je nepojmenovanou rekurzi
                                createBackwardAddingTransitions(previousStates, leftSideNames, transitFunction);
                                actualStates = previousStates;
                            }
                            else
                            {
                                // asociace je pojmenovana asociace
                                HashSet<StateWordPair> originalUncompletedBuffer = ListUtils.copy(uncompleteTransitionsBuffer);
                                HashSet<StateWordPair> newUncompletedBuffer = new HashSet<StateWordPair>();
                                foreach (AutomatState actualState in previousStates)
                                {
                                    uncompleteTransitionsBuffer = originalUncompletedBuffer;
                                    actualStates.Add(createTransitionsForSimpleAssociation(association, actualState, ListUtils.removeFromStartOfList(leftSideNames, index), transitFunction, uncompleteTransitionsBuffer, uncompleteTransitionsBuffer));
                                    newUncompletedBuffer.UnionWith(uncompleteTransitionsBuffer);
                                }
                                uncompleteTransitionsBuffer = newUncompletedBuffer;
                                proceedeFirstForRecursion(previousStates);
                                index++;
                            }
                        }
                    }

                }
            }

            // pokud jsme nasli rekurzi, tak vytvorime zpetne prechody, ktere zmensuji hloubku stavu
            if (leftSideOfRecursionStates != null)
                foreach (AutomatState state in actualStates)
                {
                    foreach (AutomatState firstForRec in firstForRecursion)
                    {
                        foreach (String leftSideName in leftSideNames)
                        {
                            if (transitFunction.ContainsKey(new StateWordPair(firstForRec, leftSideName)))
                            {
                                HashSet<AutomatEdge> edges = ListUtils.copy(transitFunction[new StateWordPair(firstForRec, leftSideName)]);
                                foreach (AutomatEdge edge in edges)
                                    if (!leftSideOfRecursionStates.Contains(edge.EndState))
                                        if (!transitFunction.ContainsKey(new StateWordPair(state, leftSideName)))
                                            transitFunction.Add(new StateWordPair(state, leftSideName), new HashSet<AutomatEdge>() { new AutomatEdge(edge.EndState, EdgeMode.REMOVING) });
                            }
                        }
                    }
                }

            if (index == 0)
            {
                // asociace nema zadne dalsi asociace, ktere by ji ovlivnili
                startState.exitState = true;
                startState.LeftSide = leftSideOfRule;
                startState.startState = startState;
            }
            HashSet<AutomatState> exitStates = new HashSet<AutomatState>();
            exitStates.UnionWith(actualStates);
            // odebirame z uncompleteTransitionsBufferu prechody, ktere se nepodarilo dokoncit
            foreach (StateWordPair uncompletedTransition in uncompleteTransitionsBuffer)
            {
                AutomatState state = states.Find(a => a.name == uncompletedTransition.automatState.name);
                state.exitState = true;
                state.LeftSide = leftSideOfRule;
                exitStates.Add(state);
                // pokud jsme nasli rekurzi, tak vytvorime zpetne prechody, ktere zmensuji hloubku stavu
                if (leftSideOfRecursionStates != null)
                {
                    foreach (AutomatState pari in firstForRecursion)
                    {
                        // pridavame vzdy zpetny prechod z actualState do prvniho stavu prave strany rekurze
                        foreach (String leftSideName in leftSideNames)
                        {
                            if (transitFunction.ContainsKey(new StateWordPair(pari, leftSideName)) && uncompletedTransition.automatState.SideCompulsarity != SideCompusarity.VOLATILE_RIGHT_SIDE)
                            {
                                HashSet<AutomatEdge> edges = ListUtils.copy(transitFunction[new StateWordPair(pari, leftSideName)]);
                                foreach (AutomatEdge edge in edges)
                                    if (!leftSideOfRecursionStates.Contains(edge.EndState))
                                        if (!transitFunction.ContainsKey(new StateWordPair(uncompletedTransition.automatState, leftSideName)))
                                            transitFunction.Add(new StateWordPair(uncompletedTransition.automatState, leftSideName), new HashSet<AutomatEdge>() { new AutomatEdge(edge.EndState, EdgeMode.REMOVING) });
                            }
                        }
                    }
                }
            }

            // nastavime SideCompulsarity na prijmacich stavech
            foreach (AutomatState state in exitStates)
                if (!compulsoryAssociationOnLeftSideOfRecursion)
                    if (!compulsoryAssociationOnRightSideOfRecursion)
                        state.SideCompulsarity = SideCompusarity.BOTH_SIDES_VOLATILE;
                    else
                        state.SideCompulsarity = SideCompusarity.VOLATILE_LEFT_SIDE;
                else
                    if (!compulsoryAssociationOnRightSideOfRecursion)
                        state.SideCompulsarity = SideCompusarity.VOLATILE_RIGHT_SIDE;

            foreach (KeyValuePair<StateWordPair, HashSet<AutomatEdge>> pair in transitFunction)
            {
                automatforestStatesTransitions.Add(pair.Key, pair.Value);
            }

            automatWordToState = wordToState;
        }

        /**
         * Metoda vytvori zpetne prechody, ktere zvetsuji hloubku stavu.
         **/
        private void createBackwardAddingTransitions(HashSet<AutomatState> actualStates, List<String> leftSideNames, Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction)
        {
            leftSideOfRecursionStates = ListUtils.copy(states);
            firstForRecursionFound = false;
            // pridame zpetne prechody na leve strane                                
            foreach (AutomatState state in actualStates)
            {
                foreach (AutomatState firstForRec in firstForRecursion)
                {
                    foreach (String leftSideName in leftSideNames)
                    {
                        if (generalTransitFunction.ContainsKey(new StateWordPair(firstForRec, leftSideName)))
                        {
                            HashSet<AutomatEdge> edges = ListUtils.copy(generalTransitFunction[new StateWordPair(firstForRec, leftSideName)]);
                            foreach (AutomatEdge edge in edges)
                                if (!transitFunction.ContainsKey(new StateWordPair(state, leftSideName)) && !generalTransitFunction.ContainsKey(new StateWordPair(state, leftSideName)))
                                    transitFunction.Add(new StateWordPair(state, leftSideName), new HashSet<AutomatEdge>() { new AutomatEdge(edge.EndState, EdgeMode.ADDING) });
                        }
                    }
                }
            }
            foreach (StateWordPair uncompleteTransition in uncompleteTransitionsBuffer)
            {
                foreach (AutomatState state in actualStates)
                {
                    foreach (AutomatState firstForRec in firstForRecursion)
                    {
                        foreach (String leftSideName in leftSideNames)
                        {
                            if (generalTransitFunction.ContainsKey(new StateWordPair(firstForRec, leftSideName)))
                            {
                                HashSet<AutomatEdge> edges = ListUtils.copy(generalTransitFunction[new StateWordPair(firstForRec, leftSideName)]);
                                foreach (AutomatEdge edge in edges)
                                    if (!transitFunction.ContainsKey(new StateWordPair(uncompleteTransition.automatState, leftSideName)) && !generalTransitFunction.ContainsKey(new StateWordPair(uncompleteTransition.automatState, leftSideName)))
                                        transitFunction.Add(new StateWordPair(uncompleteTransition.automatState, leftSideName), new HashSet<AutomatEdge>() { new AutomatEdge(edge.EndState, EdgeMode.ADDING) });
                            }
                        }
                    }
                }
            }
        }

        /**
         * Metdoa prida do firstForRecursion stavy, ktere jsou prvni stavy na leve ci prave strane rekurze. 
         **/
        private void proceedeFirstForRecursion(HashSet<AutomatState> currentState)
        {
            // cashujeme si prechod do prvniho stavu ktery je pro struktural representativ aby jsme mohli 
            // udelat zpetne smycky
            // stejne tak hashujem prvni prechod do prave strany rekurze, aby jsme mohli udelat zpetne 
            // prechody na prave strane
            if (!firstForRecursionFound)
            {
                firstForRecursion = new HashSet<AutomatState>();
                firstForRecursion.UnionWith(currentState);
                foreach (StateWordPair uncompletedTransition in uncompleteTransitionsBuffer)
                    firstForRecursion.Add(uncompletedTransition.automatState);
                firstForRecursionFound = true;
            }
        }

        /**
         * Zakoduje mnozinu stavu do unikatniho stringu.
         **/
        private string codeAutomatSet(HashSet<AutomatState> automatStates)
        {
            AutomatState[] array = new AutomatState[automatStates.Count];
            automatStates.CopyTo(array, 0, automatStates.Count);
            Array.Sort(array, stateNameComparer);
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                code.Append(array[i].name);
            }
            return code.ToString();
        }

        private List<PSMAssociation> getAssociations(PSMAssociation association)
        {
            return AssociationsUtils.getAssociations(association, mode);
        }

        /**
         * Metoda vytvori konecny automat pro asociaci, ktera vede do content modelu. 
         **/
        private Dictionary<StateWordPair, HashSet<AutomatEdge>> createTransitionsForChildOfContentModel(PSMAssociation associationToContentModel, List<String> leftSideNames, List<AutomatState> states, ref HashSet<AutomatState> finalStates, HashSet<StateWordPair> unfinishedTransitionsBuffer, HashSet<StateWordPair> outcomingUnfinishedTransitionsBuffer)
        {
            Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction = new Dictionary<StateWordPair, HashSet<AutomatEdge>>();
            if (((PSMContentModel)associationToContentModel.Child).Type == PSMContentModelType.Choice)
            {
                AutomatState startState = new AutomatState(stateDesignation + stateCount, true, currentStartState);
                stateCount++;
                proceedeFirstForRecursion(new HashSet<AutomatState>() { startState });
                List<PSMAssociation> associations = getAssociations(associationToContentModel);
                // jako left side names predavat podle toho, jestli je neco za choice nebo uz ne...
                bool lastInAutomat = AssociationsUtils.getAllContributingAssociations(associationToContentModel, new HashSet<PSMAssociation>(), mode).Count == leftSideNames.Count;
                HashSet<StateWordPair> uncompletedBuffer = new HashSet<StateWordPair>();
                foreach (PSMAssociation association in associations)
                {
                    if (association.IsNonTreeAssociation && !association.IsNamed)
                    {
                        createBackwardAddingTransitions(new HashSet<AutomatState>() { startState }, allLeftSideNames, transitFunction);
                        uncompletedBuffer.UnionWith(uncompleteTransitionsBuffer);
                        outcomingUnfinishedTransitionsBuffer.Add(new StateWordPair(startState));
                    }
                    else
                    {
                        if (association.Child is PSMContentModel)
                        {
                            int numberOfChildClasses = AssociationsUtils.namedAssociations(association, mode != CreationPhase.ATTRIBUTE && mode != CreationPhase.ELEMENT_ATTRIBUTE).Count;
                            finalStates.UnionWith(createTransitionsForContentModel(association, startState, ListUtils.getFirstFromList(leftSideNames, numberOfChildClasses), transitFunction, outcomingUnfinishedTransitionsBuffer));
                            leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, numberOfChildClasses);
                        }
                        else
                        {
                            if (mode == CreationPhase.ATTRIBUTE)
                            {
                                foreach (PSMAttribute attribute in ((PSMClass)association.Child).GetActualPSMAttributesIncludingInherited())
                                {
                                    if (!attribute.Element)
                                    {
                                        AutomatState currentState = createTransitionsForAttribute(attribute, startState, leftSideNames, transitFunction, unfinishedTransitionsBuffer, outcomingUnfinishedTransitionsBuffer);
                                        finalStates.Add(currentState);
                                        leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, 1);
                                    }
                                }
                            }
                            else
                            {
                                finalStates.Add(createTransitionsForSimpleAssociation(association, startState, lastInAutomat ? leftSideNames.GetRange(0, 1) : leftSideNames, transitFunction, unfinishedTransitionsBuffer, outcomingUnfinishedTransitionsBuffer));
                                leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, 1);                                
                                uncompletedBuffer.UnionWith(uncompleteTransitionsBuffer);
                            }
                        }
                    }
                }
                uncompleteTransitionsBuffer.UnionWith(uncompletedBuffer);
            }
            if (((PSMContentModel)associationToContentModel.Child).Type == PSMContentModelType.Sequence)
            {
                AutomatState startState = new AutomatState(stateDesignation + stateCount, true, currentStartState);
                states.Add(startState);
                HashSet<StateWordPair> uncompletedBuffer = ListUtils.copy(unfinishedTransitionsBuffer);
                stateCount++;
                HashSet<AutomatState> currentState = new HashSet<AutomatState>() { startState };
                foreach (PSMAssociation association in getAssociations(associationToContentModel))
                {
                    if (association.IsNonTreeAssociation && !association.IsNamed)
                    {
                        createBackwardAddingTransitions(currentState, allLeftSideNames, transitFunction);
                    }
                    else
                    {
                        HashSet<AutomatState> newCurrentState = new HashSet<AutomatState>();
                        foreach (AutomatState state in currentState)
                        {
                            HashSet<StateWordPair> newOutComingBuffer = new HashSet<StateWordPair>();
                            if (association.Child is PSMContentModel)
                            {
                                int numberOfChildClasses = getAssociations(association).Count;
                                HashSet<AutomatState> endStates = createTransitionsForContentModel(association, state, leftSideNames, transitFunction, uncompletedBuffer);
                                newCurrentState.UnionWith(endStates);
                                leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, numberOfChildClasses);
                            }
                            else
                            {
                                AutomatState newState = createTransitionsForSimpleAssociation(association, state, leftSideNames, transitFunction, uncompletedBuffer, newOutComingBuffer);
                                newCurrentState.Add(newState);
                                leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, 1);
                                proceedeFirstForRecursion(currentState);

                            }
                            uncompletedBuffer.UnionWith(newOutComingBuffer);
                            outcomingUnfinishedTransitionsBuffer.UnionWith(newOutComingBuffer);
                        }
                        currentState = newCurrentState;
                    }
                }
                unfinishedTransitionsBuffer.RemoveWhere(a => !uncompletedBuffer.Contains(a));
                outcomingUnfinishedTransitionsBuffer.RemoveWhere(a => !uncompletedBuffer.Contains(a));
                finalStates = currentState;
            }
            if (((PSMContentModel)associationToContentModel.Child).Type == PSMContentModelType.Set)
            {
                AutomatState startState = new AutomatState(stateDesignation + stateCount, true, currentStartState);
                stateCount++;
                int numberOfChildClasses = AssociationsUtils.namedAssociations(associationToContentModel, mode != CreationPhase.ATTRIBUTE && mode != CreationPhase.ELEMENT_ATTRIBUTE).Count;
                finalStates = createTransitionsForSet(associationToContentModel.Child.ChildPSMAssociations, new HashSet<AutomatState>() { startState }, leftSideNames, states, transitFunction, unfinishedTransitionsBuffer, outcomingUnfinishedTransitionsBuffer);
                leftSideNames = ListUtils.removeFromStartOfList(leftSideNames, numberOfChildClasses);
            }
            return transitFunction;
        }

        /**
         * Metoda vytvori prechody pro asociace vedouci z content modelu typu set.
         **/
        private HashSet<AutomatState> createTransitionsForSet(ICollection<PSMAssociation> associations, HashSet<AutomatState> startState, List<String> leftSideNames, List<AutomatState> states, Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, HashSet<StateWordPair> unfinishedTransitionsBuffer, HashSet<StateWordPair> outcomingUnfinishedBuffer)
        {
            HashSet<AutomatState> result = new HashSet<AutomatState>();
            int index = 0;
            int counter = 0;
            foreach (PSMAssociation association in associations)
            {
                int indexIncreese = 0;
                HashSet<StateWordPair> tempUnfinishedBuffer = ListUtils.copy(unfinishedTransitionsBuffer);
                foreach (AutomatState state in startState)
                {
                    HashSet<AutomatState> newCurrentState = new HashSet<AutomatState>();
                    List<String> newLeftSideNames = null;
                    HashSet<StateWordPair> buffer = new HashSet<StateWordPair>();
                    if (association.Child is PSMContentModel)
                    {
                        int numberOfChildClasses = AssociationsUtils.namedAssociations(association, mode != CreationPhase.ATTRIBUTE && mode != CreationPhase.ELEMENT_ATTRIBUTE).Count;
                        newLeftSideNames = ListUtils.setOnStartPosition(leftSideNames, index, numberOfChildClasses);
                        HashSet<AutomatState> endStates = createTransitionsForContentModel(association, state, newLeftSideNames, transitFunction, outcomingUnfinishedBuffer);
                        newCurrentState.UnionWith(endStates);
                        newLeftSideNames.RemoveRange(0, numberOfChildClasses);
                        indexIncreese = numberOfChildClasses;
                    }
                    else
                    {
                        newLeftSideNames = ListUtils.setOnStartPosition(leftSideNames, index, 1);
                        AutomatState newState = null;
                        if (mode == CreationPhase.ATTRIBUTE || mode == CreationPhase.ELEMENT_ATTRIBUTE)
                        {
                            foreach (PSMAttribute attribute in ((PSMClass)association.Child).PSMAttributes)
                            {
                                newState = createTransitionsForAttribute(attribute, state, newLeftSideNames, transitFunction, tempUnfinishedBuffer, buffer);
                                newCurrentState.Add(newState);
                                newLeftSideNames.RemoveAt(0);
                                indexIncreese++;
                            }
                        }
                        else
                        {
                            newState = createTransitionsForSimpleAssociation(association, state, newLeftSideNames, transitFunction, tempUnfinishedBuffer, buffer);
                            proceedeFirstForRecursion(startState);
                            newCurrentState.Add(newState);
                            newLeftSideNames.RemoveAt(0);
                            indexIncreese = 1;
                        }
                    }
                    PSMAssociation[] newAssociations = new PSMAssociation[associations.Count];
                    associations.CopyTo(newAssociations, 0);
                    List<PSMAssociation> associationList = newAssociations.ToList();
                    associationList.Remove(association);
                    if (associationList.Count > 0)
                    {
                        HashSet<StateWordPair> newOutcomming = new HashSet<StateWordPair>();
                        buffer.UnionWith(tempUnfinishedBuffer);
                        result.UnionWith(createTransitionsForSet(associationList, newCurrentState, newLeftSideNames, states, transitFunction, buffer, newOutcomming));
                        tempUnfinishedBuffer.RemoveWhere(a => !newOutcomming.Contains(a));
                        outcomingUnfinishedBuffer.UnionWith(newOutcomming);
                        outcomingUnfinishedBuffer.RemoveWhere(a => !newOutcomming.Contains(a));
                    }
                    else
                    {
                        outcomingUnfinishedBuffer.UnionWith(buffer);
                        outcomingUnfinishedBuffer.RemoveWhere(a => !buffer.Contains(a));
                        result.UnionWith(newCurrentState);
                    }
                }
                index += indexIncreese;
                counter++;
                if (counter == associations.Count)
                    unfinishedTransitionsBuffer.RemoveWhere(a => !tempUnfinishedBuffer.Contains(a));
            }
            return result;
        }

        /**
         * Metoda vytvori prechody pro asociaci vedouci do content modelu. 
         **/
        private HashSet<AutomatState> createTransitionsForContentModel(PSMAssociation association, AutomatState currentState, List<String> leftSideNames, Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, HashSet<StateWordPair> beforeContentModelBuffer)
        {
            Dictionary<StateWordPair, HashSet<AutomatEdge>> childTransitFunction = new Dictionary<StateWordPair, HashSet<AutomatEdge>>();
            HashSet<StateWordPair> behindContentModelBuffer = new HashSet<StateWordPair>();
            HashSet<StateWordPair> inContentModelBuffer = new HashSet<StateWordPair>();

            // seznam prechodu ze startovaciho stavu
            List<StateWordPair> startTransitions = new List<StateWordPair>();

            List<StateWordPair> startPairs = new List<StateWordPair>();
            HashSet<AutomatState> oldExitStates = new HashSet<AutomatState>();
            oldExitStates.Add(currentState);
            HashSet<AutomatState> newExitStates = new HashSet<AutomatState>();

            HashSet<AutomatState> finalStates = new HashSet<AutomatState>();
            HashSet<AutomatState> originalFinalStates = new HashSet<AutomatState>();
            HashSet<StateWordPair> childUncompletedTransitions = new HashSet<StateWordPair>();
            List<AutomatState> beforeChildContentModelStates = ListUtils.copy(states);
            int basicStateCount = states.Count;
            if (association.Lower == 0)
            {
                // preskoceni celyho kontent modelu
                // z prvku pred kontent modelem pridat prechod do prvku za kontent modelem
                // pridavame do BehinContent buferu, aby se z toho vybiralo az pri tvorbe prechodu pro dalsi asociaci
                behindContentModelBuffer.Add(new StateWordPair(currentState));
                behindContentModelBuffer.UnionWith(beforeContentModelBuffer);
            }
            // vytvorime konecny automat rozpoznavajci strom urceny content modelem do ktereho vede association
            // potrebujem aby se pri vytvareni simpleAssociation bralo z uncompleted bufferu a dokoncovali se tak nedokonceny asociace pred content modelem
            childTransitFunction = createTransitionsForChildOfContentModel(association, leftSideNames, states, ref finalStates, beforeContentModelBuffer, childUncompletedTransitions);
            if (beforeContentModelBuffer.Count > 0 && ((PSMContentModel)association.Child).Type == PSMContentModelType.Choice)
            {
                behindContentModelBuffer.Add(new StateWordPair(currentState));
                behindContentModelBuffer.UnionWith(beforeContentModelBuffer);
            }
            basicStateCount = states.Count - basicStateCount;
            originalFinalStates = finalStates;
            basicStateCount++;
            for (uint i = 0; i < association.Lower; i++)
            {
                // pridame minimalni opakovani automatu
                HashSet<AutomatState> oldFinalStates = finalStates;
                HashSet<StateWordPair> oldInContentBuffer = inContentModelBuffer;
                inContentModelBuffer = new HashSet<StateWordPair>();
                if (i == 0)
                    oldFinalStates = oldExitStates;
                // pri kopirovani automatu, potrebujem upravit i prechody ktery se vytvorily pri dokonceni nekompletnich asociaci...
                // chce to jeste nakej seznam dokoncenych asociacia v kopirovani budeme pouze menit jejich koncovy stav... ale to se bude
                // dit pouze pro prvni kopii
                // neni jednodusi ten automat znova vytvorit, nez ho kopirovat?! - znova vytvaret je neefektivni kvuli set...
                addAllTransitions(createCopyOfAutomatFromTransitions(childTransitFunction, states, startPairs, newExitStates, states.Count + basicStateCount, ref finalStates, originalFinalStates, childUncompletedTransitions, inContentModelBuffer, beforeChildContentModelStates, i == 0), transitFunction);
                // odebereme z uncompletedTransitionBufferu
                // take potrebujem aby se zde taky napojovali nedokonceny asociace zacinajci pred kontent modelem - to muze nastat, pokud mame
                // jako content model choice a jedna z moznosti ma dolni kardinalitu 0... tj. potrebujem vybirat z uncompletedTransitionsBufferu
                // pro sequence toto nenastava
                if (((PSMContentModel)association.Child).Type == PSMContentModelType.Choice)
                {
                    foreach (StateWordPair uncompletedTransition in uncompleteTransitionsBuffer)
                    {
                        foreach (StateWordPair startPair in startPairs)
                        {
                            StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, startPair.word);
                            // nemusime odebirat z uncompletdTransitionsBuffer protoze, pokud by dana asociace mela nenulovou kardinalitu,
                            // tak jsme jiz odebrali pri vytvareni automatu pro content model                        
                            if (!transitFunction.ContainsKey(newUncompletedTransition))
                            {
                                transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                            }
                            if (!transitFunction[newUncompletedTransition].Contains(new AutomatEdge(startPair.automatState), automatEdgeComparer))
                                transitFunction[newUncompletedTransition].Add(new AutomatEdge(startPair.automatState));

                        }
                    }
                }
                else
                {
                    HashSet<StateWordPair> toRemove = new HashSet<StateWordPair>();
                    foreach (StateWordPair uncompletedTransition in oldInContentBuffer)
                    {
                        foreach (StateWordPair startPair in startPairs)
                        {
                            bool lowerZero = transitFunction.Keys.Count(a => a.automatState == startPair.automatState) > 1;
                            if (!lowerZero)
                                toRemove.Add(uncompletedTransition);
                            StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, startPair.word);
                            if (!transitFunction.ContainsKey(newUncompletedTransition))
                            {
                                transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                            }
                            transitFunction[newUncompletedTransition].UnionWith(transitFunction[startPair]);
                        }
                    }
                    foreach (StateWordPair completedTransition in toRemove)
                    {
                        oldInContentBuffer.Remove(completedTransition);
                    }
                }
                // propojime opakujici se automaty
                conectAutomats(transitFunction, ref startPairs, oldFinalStates, inContentModelBuffer);
                startTransitions = startPairs;
                oldExitStates = newExitStates;
                startPairs = new List<StateWordPair>();
                newExitStates = new HashSet<AutomatState>();
                inContentModelBuffer.UnionWith(oldInContentBuffer);
            }
            if (association.Upper == UnlimitedInt.Infinity)
            {
                if (association.Lower == 0)
                {
                    HashSet<AutomatState> oldFinalStates = oldExitStates;
                    addAllTransitions(createCopyOfAutomatFromTransitions(childTransitFunction, states, startPairs, newExitStates, states.Count + basicStateCount, ref finalStates, originalFinalStates, childUncompletedTransitions, inContentModelBuffer, beforeChildContentModelStates, true), transitFunction);
                    if (((PSMContentModel)association.Child).Type == PSMContentModelType.Choice)
                    {
                        foreach (StateWordPair uncompletedTransition in uncompleteTransitionsBuffer)
                        {
                            foreach (StateWordPair startPair in startPairs)
                            {
                                StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, startPair.word);
                                if (!transitFunction.ContainsKey(newUncompletedTransition))
                                {
                                    transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                                }
                                if (!transitFunction[newUncompletedTransition].Contains(new AutomatEdge(startPair.automatState), automatEdgeComparer))
                                    transitFunction[newUncompletedTransition].Add(new AutomatEdge(startPair.automatState));

                            }
                        }
                    }
                    // propojime opakujici se automaty
                    conectAutomats(transitFunction, ref startPairs, oldFinalStates, inContentModelBuffer);
                    startTransitions = startPairs;
                    oldExitStates = newExitStates;
                    startPairs = new List<StateWordPair>();
                    newExitStates = new HashSet<AutomatState>();

                }

                // z posledniho prvku content modelu pridat prechod do posledniho prvku pred vstupem do content modelu                
                createInfiniteLoop(transitFunction, startTransitions, finalStates);

            }
            else
            {
                if (association.Lower != association.Upper.Value)
                {
                    createTransitionFromStartStateBehindContentModel(finalStates, behindContentModelBuffer);
                }
                for (uint i = association.Lower; i < association.Upper.Value; i++)
                {
                    HashSet<StateWordPair> oldInContentBuffer = inContentModelBuffer;
                    inContentModelBuffer = new HashSet<StateWordPair>();
                    HashSet<AutomatState> oldFinalStates = finalStates;
                    if (i == 0)
                        oldFinalStates = oldExitStates;
                    addAllTransitions(createCopyOfAutomatFromTransitions(childTransitFunction, states, startPairs, newExitStates, states.Count + basicStateCount, ref finalStates, originalFinalStates, childUncompletedTransitions, inContentModelBuffer, beforeChildContentModelStates, i == 0), transitFunction);
                    if (((PSMContentModel)association.Child).Type == PSMContentModelType.Choice)
                    {
                        foreach (StateWordPair uncompletedTransition in uncompleteTransitionsBuffer)
                        {
                            foreach (StateWordPair startPair in startPairs)
                            {
                                StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, startPair.word);
                                // nemusime odebirat z uncompletdTransitionsBuffer protoze, pokud by dana asociace mela nenulovou kardinalitu,
                                // tak jsme jiz odebrali pri vytvareni automatu pro content model                        
                                if (!transitFunction.ContainsKey(newUncompletedTransition))
                                {
                                    transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                                }
                                if (!transitFunction[newUncompletedTransition].Contains(new AutomatEdge(startPair.automatState), automatEdgeComparer))
                                    transitFunction[newUncompletedTransition].Add(new AutomatEdge(startPair.automatState));

                            }
                        }
                    }
                    else
                    {
                        HashSet<StateWordPair> toRemove = new HashSet<StateWordPair>();
                        foreach (StateWordPair uncompletedTransition in oldInContentBuffer)
                        {
                            foreach (StateWordPair startPair in startPairs)
                            {
                                StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, startPair.word);
                                if (!transitFunction.ContainsKey(newUncompletedTransition))
                                {
                                    transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                                }
                                transitFunction[newUncompletedTransition].UnionWith(transitFunction[startPair]);
                            }
                        }
                    }

                    // propojime opakujici se automaty
                    conectAutomats(transitFunction, ref startPairs, oldFinalStates, behindContentModelBuffer);
                    oldExitStates = newExitStates;
                    startPairs = new List<StateWordPair>();
                    newExitStates = new HashSet<AutomatState>();
                    inContentModelBuffer.UnionWith(oldInContentBuffer);
                    // z kazdeho start state pro dany content model pridame prechod do prvku za content model
                    if (i < association.Upper.Value - 1)
                        createTransitionFromStartStateBehindContentModel(finalStates, behindContentModelBuffer);
                }
            }

            beforeContentModelBuffer.UnionWith(inContentModelBuffer);
            beforeContentModelBuffer.UnionWith(behindContentModelBuffer);

            return finalStates;
        }

        /**
         * Vytvori asociace z pocateniho stavu content modelu za content model. 
         **/
        private void createTransitionFromStartStateBehindContentModel(HashSet<AutomatState> newExitStates, HashSet<StateWordPair> uncompletedTransitionsBuffer)
        {
            foreach (AutomatState exitState in newExitStates)
            {
                uncompletedTransitionsBuffer.Add(new StateWordPair(exitState));
            }
        }

        /**
         * Vytvori z konecnych stavu pro content model prechod do prvniho stavu pro conten model.
         **/
        private void createInfiniteLoop(Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, List<StateWordPair> startTransitions, HashSet<AutomatState> exitStates)
        {
            foreach (StateWordPair startTransition in startTransitions)
            {
                foreach (AutomatState exitState in exitStates)
                {
                    if (!transitFunction.ContainsKey(new StateWordPair(exitState, startTransition.word)))
                        transitFunction.Add(new StateWordPair(exitState, startTransition.word), transitFunction[startTransition]);
                    else
                        transitFunction[new StateWordPair(exitState, startTransition.word)] = transitFunction[startTransition];
                }
            }
        }

        /**
         * Propoji dva automaty pro content model.
         **/
        private void conectAutomats(Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, ref List<StateWordPair> startPairs, HashSet<AutomatState> exitStates, HashSet<StateWordPair> behindContentBuffer)
        {
            List<StateWordPair> newStartPairs = new List<StateWordPair>();
            if (exitStates.Count > 0)
                foreach (StateWordPair startPair in startPairs)
                {
                    foreach (AutomatState exitState in exitStates)
                    {
                        exitState.exitState = false;
                        if (!transitFunction.ContainsKey(new StateWordPair(exitState, startPair.word)))
                            transitFunction.Add(new StateWordPair(exitState, startPair.word), transitFunction[startPair]);
                        newStartPairs.Add(new StateWordPair(exitState, startPair.word));
                        if (behindContentBuffer.Contains(new StateWordPair(startPair.automatState)))
                        {
                            behindContentBuffer.Remove(new StateWordPair(startPair.automatState));
                            behindContentBuffer.Add(new StateWordPair(exitState));
                        }
                    }
                    transitFunction.Remove(startPair);
                }
            startPairs = newStartPairs;
        }

        /**
         * Vytvori kopii automatu pro content model, nazvy stavu jsou jiny v kopii nez v originalu. 
         **/
        private Dictionary<StateWordPair, HashSet<AutomatEdge>> createCopyOfAutomatFromTransitions(Dictionary<StateWordPair, HashSet<AutomatEdge>> originalTransitFunction, List<AutomatState> states, List<StateWordPair> startPairs, HashSet<AutomatState> exitStates, int basicStateCount, ref HashSet<AutomatState> finalStates, HashSet<AutomatState> originalFinalStates, HashSet<StateWordPair> originalAutomatUncompletedTransitions, HashSet<StateWordPair> behindContentModelBuffer, List<AutomatState> beforeChildContentModelStates, bool firstCopy)
        {
            basicStateCount++;
            Dictionary<StateWordPair, HashSet<AutomatEdge>> result = new Dictionary<StateWordPair, HashSet<AutomatEdge>>();
            HashSet<AutomatState> newFinalStates = new HashSet<AutomatState>();
            foreach (KeyValuePair<StateWordPair, HashSet<AutomatEdge>> dictionaryRecord in originalTransitFunction)
            {
                if (beforeChildContentModelStates.Contains(dictionaryRecord.Key.automatState) && !firstCopy)
                    continue;
                StateWordPair newStateWordPair = new StateWordPair();
                newStateWordPair.word = dictionaryRecord.Key.word;
                if (beforeChildContentModelStates.Contains(dictionaryRecord.Key.automatState))
                {
                    newStateWordPair.automatState = dictionaryRecord.Key.automatState;
                }
                else
                {
                    AutomatState fromState = new AutomatState(stateDesignation + (dictionaryRecord.Key.automatState.getIndexOfState() + basicStateCount), dictionaryRecord.Key.automatState, null);
                    stateCount++;
                    if (firstForRecursion != null && firstForRecursion.Contains(dictionaryRecord.Key.automatState))
                    {
                        firstForRecursion.Remove(dictionaryRecord.Key.automatState);
                        firstForRecursion.Add(fromState);
                    }
                    if (originalFinalStates.Contains(dictionaryRecord.Key.automatState, automatStateComparer) && !newFinalStates.Contains(fromState, automatStateComparer))
                    {
                        newFinalStates.Add(fromState);
                    }
                    if (!states.Contains(fromState))
                    {
                        states.Add(fromState);
                        if (stateCount <= dictionaryRecord.Key.automatState.getIndexOfState() + basicStateCount)
                        {
                            stateCount = dictionaryRecord.Key.automatState.getIndexOfState() + basicStateCount + 1;
                        }
                    }
                    if (states.Contains(fromState, automatStateComparer))
                    {
                        fromState = states.Find(a => a.name.Equals(fromState.name));
                    }
                    newStateWordPair.automatState = fromState;
                    if (newStateWordPair.automatState.enterState)
                        startPairs.Add(newStateWordPair);
                    if (originalAutomatUncompletedTransitions.Contains(new StateWordPair(dictionaryRecord.Key.automatState)))
                    {
                        behindContentModelBuffer.Add(new StateWordPair(fromState));
                    }
                }
                HashSet<AutomatEdge> toStates = new HashSet<AutomatEdge>();
                foreach (AutomatEdge edge in dictionaryRecord.Value)
                {
                    AutomatState toState = null;
                    if (beforeChildContentModelStates.Contains(edge.EndState))
                    {
                        toState = edge.EndState;
                    }
                    else
                    {
                        toState = new AutomatState(stateDesignation + (edge.EndState.getIndexOfState() + basicStateCount), edge.EndState, null);
                        stateCount++;
                        if (firstForRecursion != null && firstForRecursion.Contains(edge.EndState))
                        {
                            firstForRecursion.Remove(edge.EndState);
                            firstForRecursion.Add(toState);
                        }
                        if (originalFinalStates.Contains(edge.EndState, automatStateComparer) && !newFinalStates.Contains(toState, automatStateComparer))
                        {
                            newFinalStates.Add(toState);
                        }
                        if (!states.Contains(toState))
                        {
                            states.Add(toState);
                            if (stateCount <= edge.EndState.getIndexOfState() + basicStateCount)
                            {
                                stateCount = edge.EndState.getIndexOfState() + basicStateCount + 1;
                            }
                        }
                        if (toState.exitState && !exitStates.Contains(toState))
                            exitStates.Add(toState);
                        if (states.Contains(toState, automatStateComparer))
                        {
                            toState = states.Find(a => a.name.Equals(toState.name));
                        }
                    }
                    toStates.Add(new AutomatEdge(toState, edge.EdgeMode,edge.AttributeType));
                    if (originalAutomatUncompletedTransitions.Contains(new StateWordPair(edge.EndState)))
                    {
                        behindContentModelBuffer.Add(new StateWordPair(toState));
                    }
                }
                result.Add(newStateWordPair, toStates);
            }
            finalStates = newFinalStates;
            return result;
        }

        private void addAllTransitions(Dictionary<StateWordPair, HashSet<AutomatEdge>> from, Dictionary<StateWordPair, HashSet<AutomatEdge>> to)
        {
            foreach (KeyValuePair<StateWordPair, HashSet<AutomatEdge>> pair in from)
            {
                if (to.ContainsKey(pair.Key))
                    to[pair.Key].UnionWith(pair.Value);
                else
                    to.Add(pair.Key, pair.Value);
            }
        }

        /**
         * Vytvori prechod v konecnem automatu pro dany atribut. 
         **/
        private AutomatState createTransitionsForAttribute(PSMAttribute attribute, AutomatState actualState, List<String> leftSideNames, Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, HashSet<StateWordPair> unfinishedTransitionsBuffer, HashSet<StateWordPair> outcomingUnfinishedBuffer)
        {
            List<StateWordPair> newUncompleteTransitionsBuffer = new List<StateWordPair>();
            int index = 0;
            AutomatState firstStateForClass = actualState;
            if (attribute.Lower == 0)
            {
                // pridat prechod z predchoziho stavu do stavu nasledujiciho
                newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
            }
            for (uint i = 0; i < attribute.Lower; i++)
            {
                // splneni minimalniho poctu elementu 
                StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                if ((attribute.Upper == attribute.Lower && index == leftSideNames.Count - 1 && i == attribute.Lower - 1) ||
                    (attribute.Upper == UnlimitedInt.Infinity && index == leftSideNames.Count - 1 && i == attribute.Lower - 1))
                {
                    newState.exitState = true;
                    newState.LeftSide = leftSideOfRule;
                }
                states.Add(newState);
                transitFunction[transit].Add(new AutomatEdge(newState,attribute.AttributeType));
                stateCount++;
                actualState = newState;
                if (i == 0)
                {
                    firstStateForClass = newState;
                }
            }
            if (attribute.Upper == UnlimitedInt.Infinity)
            {
                // pridame cyklus na novem stavu
                StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                if (attribute.Lower == 0)
                {
                    AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                    if (index == leftSideNames.Count - 1)
                    {
                        newState.exitState = true;
                        newState.LeftSide = leftSideOfRule;
                    }
                    states.Add(newState);
                    transitFunction[transit].Add(new AutomatEdge(newState,attribute.AttributeType));
                    actualState = newState;
                    stateCount++;
                }

                transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                transitFunction[transit].Add(new AutomatEdge(actualState,attribute.AttributeType));

            }
            else
            {
                // splneni omezeni horniho poctu elementu
                if (attribute.Lower != attribute.Upper)
                    if (index == leftSideNames.Count - 1)
                    {
                        actualState.exitState = true;
                        actualState.LeftSide = leftSideOfRule;
                    }
                    else
                    {
                        newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
                    }
                uint upper = attribute.Upper.Value > 1 && mode == CreationPhase.ATTRIBUTE ? 1 : attribute.Upper.Value;
                for (uint i = attribute.Lower; i < upper; i++)
                {
                    StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                    if (!transitFunction.ContainsKey(transit))
                    {
                        transitFunction.Add(transit, new HashSet<AutomatEdge>());
                    }
                    AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                    if (index == leftSideNames.Count - 1)
                    {
                        newState.exitState = true;
                        newState.LeftSide = leftSideOfRule;
                    }
                    else
                    {
                        newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
                    }
                    states.Add(newState);
                    transitFunction[transit].Add(new AutomatEdge(newState,attribute.AttributeType));
                    actualState = newState;
                    stateCount++;
                    if (i == 0)
                    {
                        firstStateForClass = newState;
                    }
                }
            }


            // odebirani s uncompletedTransitionsBufferu
            List<StateWordPair> transitionsToRemove = new List<StateWordPair>();
            foreach (StateWordPair uncompletedTransition in unfinishedTransitionsBuffer)
            {
                StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, leftSideNames[index]);
                if (attribute.Lower != 0)
                {
                    transitionsToRemove.Add(uncompletedTransition);
                }
                if (!newUncompletedTransition.automatState.Equals(firstStateForClass))
                {
                    if (!transitFunction.ContainsKey(newUncompletedTransition))
                    {
                        transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                    }
                    if (!transitFunction[newUncompletedTransition].Contains(new AutomatEdge(firstStateForClass, attribute.AttributeType), automatEdgeComparer))
                        transitFunction[newUncompletedTransition].Add(new AutomatEdge(firstStateForClass, attribute.AttributeType));
                }
            }
            foreach (StateWordPair uncompletedTransition in transitionsToRemove)
            {
                unfinishedTransitionsBuffer.Remove(uncompletedTransition);
            }
            foreach (StateWordPair uncompletedTransition in newUncompleteTransitionsBuffer)
            {
                outcomingUnfinishedBuffer.Add(uncompletedTransition);
            }

            return actualState;
        }

        /**
         * Vytvori prechod v konecnem automatu pro danou asociaci. 
         **/
        private AutomatState createTransitionsForSimpleAssociation(PSMAssociation association, AutomatState actualState, List<String> leftSideNames, Dictionary<StateWordPair, HashSet<AutomatEdge>> transitFunction, HashSet<StateWordPair> unfinishedTransitionsBuffer, HashSet<StateWordPair> outcomingUnfinishedBuffer)
        {
            AttributeType attType = leftSideNameAtributeType.ContainsKey(leftSideNames[0]) ? leftSideNameAtributeType[leftSideNames[0]] : null; 
            List<StateWordPair> newUncompleteTransitionsBuffer = new List<StateWordPair>();
            int index = 0;
            AutomatState firstStateForClass = actualState;
            if (association.Lower == 0)
            {
                // pridat prechod z predchoziho stavu do stavu nasledujiciho
                newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
            }
            for (uint i = 0; i < association.Lower; i++)
            {
                // splneni minimalniho poctu elementu 
                StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                if ((association.Upper == association.Lower && index == leftSideNames.Count - 1 && i == association.Lower - 1) ||
                    (association.Upper == UnlimitedInt.Infinity && index == leftSideNames.Count - 1 && i == association.Lower - 1))
                {
                    newState.exitState = true;
                    newState.LeftSide = leftSideOfRule;
                }
                states.Add(newState);
                transitFunction[transit].Add(new AutomatEdge(newState,attType));
                stateCount++;
                actualState = newState;
                if (i == 0)
                {
                    firstStateForClass = newState;
                }
            }
            if (association.Upper == UnlimitedInt.Infinity)
            {
                // pridame cyklus na novem stavu
                StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                if (association.Lower == 0)
                {
                    AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                    if (index == leftSideNames.Count - 1)
                    {
                        newState.exitState = true;
                        newState.LeftSide = leftSideOfRule;
                    }
                    states.Add(newState);
                    transitFunction[transit].Add(new AutomatEdge(newState,attType));
                    actualState = newState;
                    stateCount++;
                }

                transit = new StateWordPair(actualState, leftSideNames[index]);
                if (!transitFunction.ContainsKey(transit))
                {
                    transitFunction.Add(transit, new HashSet<AutomatEdge>());
                }
                transitFunction[transit].Add(new AutomatEdge(actualState,attType));

            }
            else
            {
                // splneni omezeni horniho poctu elementu
                if (association.Lower != association.Upper)
                    if (index == leftSideNames.Count - 1)
                    {
                        actualState.exitState = true;
                        actualState.LeftSide = leftSideOfRule;
                    }
                    else
                    {
                        newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
                    }
                for (uint i = association.Lower; i < association.Upper.Value; i++)
                {
                    StateWordPair transit = new StateWordPair(actualState, leftSideNames[index]);
                    if (!transitFunction.ContainsKey(transit))
                    {
                        transitFunction.Add(transit, new HashSet<AutomatEdge>());
                    }
                    AutomatState newState = new AutomatState(stateDesignation + stateCount, currentStartState);
                    if (index == leftSideNames.Count - 1)
                    {
                        newState.exitState = true;
                        newState.LeftSide = leftSideOfRule;
                    }
                    else
                    {
                        newUncompleteTransitionsBuffer.Add(new StateWordPair(actualState));
                    }
                    states.Add(newState);
                    transitFunction[transit].Add(new AutomatEdge(newState,attType));
                    actualState = newState;
                    stateCount++;
                    if (i == 0)
                    {
                        firstStateForClass = newState;
                    }
                }
            }


            // odebirani s uncompletedTransitionsBufferu
            List<StateWordPair> transitionsToRemove = new List<StateWordPair>();
            foreach (StateWordPair uncompletedTransition in unfinishedTransitionsBuffer)
            {
                StateWordPair newUncompletedTransition = new StateWordPair(uncompletedTransition.automatState, leftSideNames[index]);
                if (association.Lower != 0)
                {
                    transitionsToRemove.Add(uncompletedTransition);
                }
                if (!newUncompletedTransition.automatState.Equals(firstStateForClass))
                {
                    if (!transitFunction.ContainsKey(newUncompletedTransition))
                    {
                        transitFunction.Add(newUncompletedTransition, new HashSet<AutomatEdge>());
                    }
                    if (!transitFunction[newUncompletedTransition].Contains(new AutomatEdge(firstStateForClass), automatEdgeComparer))
                        transitFunction[newUncompletedTransition].Add(new AutomatEdge(firstStateForClass));
                }
            }
            foreach (StateWordPair uncompletedTransition in transitionsToRemove)
            {
                unfinishedTransitionsBuffer.Remove(uncompletedTransition);
            }
            foreach (StateWordPair uncompletedTransition in newUncompleteTransitionsBuffer)
            {
                outcomingUnfinishedBuffer.Add(uncompletedTransition);
            }

            return actualState;
        }
    }
}