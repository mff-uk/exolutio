using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Xml;


namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     *  Trida reprezentujci PushdownAutomat 
     **/
    public class PushDownAutomat
    {
        private static Dictionary<PSMSchema, InitConfig> initializationCache = new Dictionary<PSMSchema, InitConfig>();

        private const string leftSideName = "T";
        private const String attributeLeftSideDesignation = "A";
        private static AutomatStateComparer automatStateComparer = new AutomatStateComparer();

        private XmlDocument XMLDocument;

        // prechody automatu
        private Dictionary<StateWordPair, HashSet<AutomatEdge>> forestStatesTransitions;        

        // pr: pro book -> BOOK
        // tj zaznam prava strana pravidla->leva strana
        private Dictionary<String, HashSet<String>> rightSideToLeftSide;

        // uchovava informaci nazev uzlu->pocatecni stav automatu, ktery rozeznava jeho pravou stranu prepisu
        // pr: BOOK -> book< necoo > pro toto pravidlo uchova dvojici book->pocatecni stav automatu rozeznavajici book
        // BLACKBOOK -> book< neco > .... book->pocatecni stav automatu rozeznavajici book
        private Dictionary<String, HashSet<AutomatState>> wordToState;
        private HashSet<AutomatState> startState;
        private HashSet<AutomatState> finalState;
        private int treeStateCount = 0;

        private FiniteAutomatUtils automatUtils;

        private Stack<String> readedNodesStackTrace;
        
        private Dictionary<PSMAssociation, String> associationLeftSideName;

        private HashSet<PSMAssociation> inicializedAssociations;    
        
        private HashSet<AutomatState> downFunction(HashSet<AutomatState> currentNodes, String childNodeNAme)
        {            
            HashSet<AutomatState> downFunction = new HashSet<AutomatState>();
            if (!wordToState.ContainsKey(childNodeNAme))
            {
                return downFunction;
            }
            foreach (AutomatState currentNode in currentNodes)
            {
                HashSet<AutomatState> automatsRecognizingChildNode = wordToState[childNodeNAme];
                foreach (AutomatState state in automatsRecognizingChildNode)
                {
                    foreach (String word in rightSideToLeftSide[childNodeNAme])
                        if (forestStatesTransitions.ContainsKey(new StateWordPair(currentNode.AutomatStateWithoutDepth, word)) )
                        {
                            downFunction.Add(state);
                        }
                }
            }
            return downFunction;
        }

       
        private HashSet<String> upFunction(HashSet<AutomatState> currentNodes, String currentWord)
        {
            HashSet<String> upFunction = new HashSet<String>();
            foreach (AutomatState currentNode in currentNodes)
            {
                if (currentNode.exitState && (currentNode.Depth == 0 || (currentNode.SideCompulsarity == SideCompusarity.VOLATILE_RIGHT_SIDE && currentNode.Depth > 0) || (currentNode.SideCompulsarity == SideCompusarity.VOLATILE_LEFT_SIDE && currentNode.Depth < 0) || (currentNode.SideCompulsarity == SideCompusarity.BOTH_SIDES_VOLATILE)))
                {                    
                    upFunction.Add(currentNode.LeftSide);                    
                }
            }
            return upFunction;
        }
        
        private HashSet<AutomatState> sideFunction(HashSet<AutomatState> currentNodes, HashSet<String> readedLeftSides)
        {
            return sideFunction(currentNodes,readedLeftSides,null);
        }

        private HashSet<AutomatState> sideFunction(HashSet<AutomatState> currentNodes, HashSet<String> readedLeftSides, String attributeValue)
        {
            HashSet<AutomatState> sideFunction = new HashSet<AutomatState>();
            foreach (AutomatState node in currentNodes)
            {
                foreach (String leftSide in readedLeftSides)
                {
                    StateWordPair stateWordPair = new StateWordPair(node.AutomatStateWithoutDepth, leftSide);
                    if (forestStatesTransitions.ContainsKey(stateWordPair))
                    {
                        foreach (AutomatEdge edge in forestStatesTransitions[stateWordPair])
                            if (!sideFunction.Contains(new AutomatState(node, edge), automatStateComparer) && matchType(edge.AttributeType,attributeValue))
                                sideFunction.Add(new AutomatState(node, edge));
                    }
                }
            }
            return sideFunction;
        }

        private bool matchType(AttributeType type, String value) {            
            if (type == null || value == null)
                return true;
            switch (type.Name) { 
                case "boolean":
                    bool boolResult;
                    return Boolean.TryParse(value, out boolResult);
                case "float":
                    float floatResult;
                    return float.TryParse(value, out floatResult);
                case "double":
                    double doubleResult;
                    return double.TryParse(value, out doubleResult);
                case "decimal":
                    decimal decimalResult;
                    return decimal.TryParse(value, out decimalResult);
                case "int":
                case "integer":
                    int intResult;
                    return int.TryParse(value, out intResult);
                case "nonPositiveIntegeer":
                    if (int.TryParse(value, out intResult))
                        return intResult < 1;
                    return false;
                case "negativeInteger":
                    if (int.TryParse(value, out intResult))
                        return intResult < 0;
                    return false;
                case "long":
                    long longResult;
                    return long.TryParse(value, out longResult);
                case "short":
                    short shortResult;
                    return short.TryParse(value, out shortResult);
                case "unsignedByte":
                case "byte":
                    byte byteResult;
                    return byte.TryParse(value, out byteResult);
                case "nonNegativeInteger":
                    if (int.TryParse(value, out intResult))
                        return intResult > -1;
                    return false;
                case "unsignedLong":
                    ulong ulongResult;
                    return ulong.TryParse(value, out ulongResult);
                case "positiveInteger":
                    if (int.TryParse(value, out intResult))
                        return intResult > 0;
                    return false;
                case "unsignedInt":
                    uint uintResult;
                    return uint.TryParse(value, out uintResult);
                case "unsignedShort":
                    ushort ushortResult;
                    return ushort.TryParse(value, out ushortResult);                  
            }
            return true;
        }

        /**
         * Provede validaci souboru vuci modelu. 
         **/
        public ValidationResult processTree()
        {
            ValidationResult validationResult = null;
            HashSet<AutomatState> q = startState;
            HashSet<String> p = processNode(q, XMLDocument.DocumentElement);
            q = sideFunction(q, p);
            HashSet<AutomatState> q2 = new HashSet<AutomatState>();
            foreach(AutomatState state in q){
                q2.Add(state.AutomatStateWithoutDepth);
            }
            q2.IntersectWith(finalState);
            if (q2.Count > 0)
            {
                validationResult = new ValidationResult(true, readedNodesStackTrace);
            }
            else
            {
                validationResult = new ValidationResult(false, readedNodesStackTrace);
            }
            return validationResult;
        }

        private HashSet<String> processNode(HashSet<AutomatState> currentState, XmlElement xmlElement)
        {
            currentState = downFunction(currentState, xmlElement.Name);
            if (currentState.Count > 0)
                readedNodesStackTrace.Push(xmlElement.Name);
            foreach (XmlAttribute att in xmlElement.Attributes) {
                currentState = sideFunction(currentState,  rightSideToLeftSide[att.Name], att.Value );
            }
            foreach (XmlNode childNode in xmlElement.ChildNodes)
            {        
                if(childNode is XmlElement)
                    currentState = sideFunction(currentState, processNode(currentState,(XmlElement) childNode));                
            }
            HashSet<String> result = new HashSet<string>();
            result = upFunction(currentState, xmlElement.Name);
            if (result.Count > 0)
                readedNodesStackTrace.Pop();
            return result;
        }

        private void removeFromCache(Object o,System.ComponentModel.PropertyChangedEventArgs args){
            initializationCache.Clear();
        }

        /**
         *  Inicializuje PushDownAutomat a jeho pomocne struktury. 
         **/
        public void inicialize(PSMSchema schema, String XMLFilePath)
        {

            XMLDocument = new XmlDocument();
            XMLDocument.Load(XMLFilePath);

            readedNodesStackTrace = new Stack<String>();

            schema.Project.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(removeFromCache);


            if (initializationCache.ContainsKey(schema))
            {
                forestStatesTransitions = initializationCache[schema].ForestStatesTransitions;
                rightSideToLeftSide = initializationCache[schema].RightSideToLeftSide;
                startState = initializationCache[schema].StartState;
                finalState = initializationCache[schema].FinalState;
                wordToState = initializationCache[schema].WordToState;
            }
            else
            {
                treeStateCount = 0;

                forestStatesTransitions = new Dictionary<StateWordPair, HashSet<AutomatEdge>>();
                rightSideToLeftSide = new Dictionary<string, HashSet<string>>();
                startState = new HashSet<AutomatState>();
                finalState = new HashSet<AutomatState>();
                wordToState = new Dictionary<string, HashSet<AutomatState>>();

                associationLeftSideName = new Dictionary<PSMAssociation, string>();

                automatUtils = new FiniteAutomatUtils();

                inicializedAssociations = new HashSet<PSMAssociation>();

                AutomatState forestStartState = new AutomatState("ss1", true, null);
                AutomatState forestFinalState = new AutomatState("ss2", forestStartState);
                forestFinalState.exitState = true;
                startState.Add(forestStartState);
                finalState.Add(forestFinalState);
                forestStatesTransitions.Add(new StateWordPair(forestStartState, "T0"), new HashSet<AutomatEdge> { new AutomatEdge(forestFinalState) });

                if (schema.PSMSchemaClass.ChildPSMAssociations.Count > 1)
                    throw new Exception("XML must have root element");
                treeStateCount++;
                associationLeftSideName.Add(schema.PSMSchemaClass.ChildPSMAssociations[0], leftSideName + (treeStateCount - 1));
                inicialize(schema.PSMSchemaClass.ChildPSMAssociations[0], leftSideName + (treeStateCount - 1));

                if (!initializationCache.ContainsKey(schema))
                { 
                    initializationCache.Add(schema,new InitConfig(forestStatesTransitions,rightSideToLeftSide,startState,finalState,wordToState));    
                }
            }
        }

        private void inicialize(PSMAssociation topAssociation, String leftSideOfRules)
        {
            if (!rightSideToLeftSide.ContainsKey(topAssociation.Name))
            {
                rightSideToLeftSide.Add(topAssociation.Name, new HashSet<string>());
            }
            rightSideToLeftSide[topAssociation.Name].Add(leftSideOfRules);

            List<String> leftSideNames = new List<string>();
            if (topAssociation.Child is PSMClass)
            {
                foreach (PSMAttribute attribute in ((PSMClass)topAssociation.Child).GetActualPSMAttributesIncludingInherited())
                {
                        String attLeftSideName = leftSideName + treeStateCount;
                        treeStateCount++;
                        leftSideNames.Add(attLeftSideName);
                        if (!rightSideToLeftSide.ContainsKey(attribute.Name))
                        {
                            rightSideToLeftSide.Add(attribute.Name, new HashSet<string>());
                        }
                        rightSideToLeftSide[attribute.Name].Add(attLeftSideName);                    
                }
            }
            foreach (PSMAssociation association in AssociationsUtils.namedAssociations(topAssociation, false, false))
            {
                if (!association.IsNamed && ((PSMClass)association.Child).PSMAttributes.Count > 0)
                {
                    foreach (PSMAttribute attribute in ((PSMClass)association.Child).PSMAttributes)
                    {
                        String attLeftSideName = leftSideName + treeStateCount;
                        leftSideNames.Add(attLeftSideName);
                        treeStateCount++;
                        if (!rightSideToLeftSide.ContainsKey(attribute.Name))
                        {
                            rightSideToLeftSide.Add(attribute.Name, new HashSet<string>());
                        }
                        rightSideToLeftSide[attribute.Name].Add(attLeftSideName);
                    }
                }                
            }

            foreach (PSMAssociation association in AssociationsUtils.namedAssociations(topAssociation, false, false))
            {
                if (association.IsNamed)
                {
                    if (associationLeftSideName.ContainsKey(association))
                    {
                        leftSideNames.Add(associationLeftSideName[association]);
                    }
                    else
                    {
                        leftSideNames.Add(leftSideName + treeStateCount);
                        associationLeftSideName.Add(association, leftSideName + treeStateCount);
                        treeStateCount++;
                    }
                }
            }

            automatUtils.createFiniteAutomatForAssociation(topAssociation, leftSideNames, leftSideOfRules, ref forestStatesTransitions, ref wordToState);
            int index = topAssociation.Child is PSMClass ? ((PSMClass)topAssociation.Child).PSMAttributes.Count : 0;
            foreach (PSMAssociation association in AssociationsUtils.namedAssociations(topAssociation, false, false))
            {
                if (!association.IsNamed && !association.IsNonTreeAssociation)
                    index++;
            }
            foreach (PSMAssociation association in AssociationsUtils.namedAssociations(topAssociation, false, false))
            {
                if (association.IsNamed && !inicializedAssociations.Contains(association))
                {
                    inicializedAssociations.Add(association);
                    inicialize(association, leftSideNames[index]);                                   
                }
                if (association.IsNamed) {
                    index++;
                }
            }
        }        

        /**
         *  Struktura slouzici k inicializaci PushDownAutomatu z cache a ukladani konfigurace do cache.
         **/
        private struct InitConfig {

            private Dictionary<StateWordPair, HashSet<AutomatEdge>> forestStatesTransitions;
            private Dictionary<string, HashSet<string>> rightSideToLeftSide;
            private HashSet<AutomatState> startState;
            private HashSet<AutomatState> finalState;
            private Dictionary<string, HashSet<AutomatState>> wordToState;

            public InitConfig(Dictionary<StateWordPair, HashSet<AutomatEdge>> forestStatesTransitions, Dictionary<string, HashSet<string>> rightSideToLeftSide, HashSet<AutomatState> startState, HashSet<AutomatState> finalState, Dictionary<string, HashSet<AutomatState>> wordToState)
            {
                this.forestStatesTransitions = forestStatesTransitions;
                this.rightSideToLeftSide = rightSideToLeftSide;
                this.startState = startState;
                this.finalState = finalState;
                this.wordToState = wordToState;
            }

            public Dictionary<StateWordPair, HashSet<AutomatEdge>> ForestStatesTransitions {
                get {
                    return forestStatesTransitions;
                }
            }

            public Dictionary<string, HashSet<string>> RightSideToLeftSide {
                get {
                    return rightSideToLeftSide;
                }
            }

            public HashSet<AutomatState> StartState {
                get {
                    return startState;
                }
            }

            public HashSet<AutomatState> FinalState {
                get {
                    return finalState;
                }
            }

            public Dictionary<string, HashSet<AutomatState>> WordToState {
                get {
                    return wordToState;
                }
            }
        }
    }
}
