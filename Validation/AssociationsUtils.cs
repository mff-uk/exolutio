using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida, ktera slouzi k ziskavani asociaci, ktere ovlivnuji danou asociaci.
     **/
    class AssociationsUtils
    {
        /**
         * Vraci vsechny asociace, ktere nejak ovlivnuji konecny automat vytvareny pro asociaci topAssociation. 
         **/
        public static List<PSMAssociation> getAllAssociations(PSMAssociation topAssociation)
        {
            List<PSMAssociation> resultAssociations = new List<PSMAssociation>();
            // 1. vytovrime fake set s vlastnimi @atributy child tridy
            if (topAssociation.Child is PSMClass)
            {
                PSMClass firstClass = (PSMClass)topAssociation.Child;
                PSMContentModel fakeSet = new PSMContentModel(topAssociation.Project, Guid.NewGuid());
                fakeSet.Type = PSMContentModelType.Set;
                PSMAssociation associationToSet = new PSMAssociation(topAssociation.Project, Guid.NewGuid());
                associationToSet.Name = "";
                associationToSet.Lower = 1;
                associationToSet.Upper = 1;
                foreach (PSMAttribute attribute in firstClass.GetActualPSMAttributes())
                {
                    if (!attribute.Element)
                    {
                        PSMAssociation associationFromSet = new PSMAssociation(topAssociation.Project, Guid.NewGuid());
                        associationFromSet.Lower = 1;
                        associationFromSet.Upper = 1;
                        associationFromSet.Name = "";
                        PSMClass fakeAttributeClass = new PSMClass(topAssociation.Project, Guid.NewGuid());
                        fakeAttributeClass.PSMAttributes.Add(attribute);
                        fakeAttributeClass.ParentAssociation = associationFromSet;
                        associationFromSet.Child = fakeAttributeClass;
                        fakeSet.ChildPSMAssociations.Add(associationFromSet);
                    }
                }
                associationToSet.Child = fakeSet;
                if (fakeSet.ChildPSMAssociations.Count > 0)
                    resultAssociations.Add(associationToSet);
            }
            // 2. pridame asociace propagujci @atributy
            resultAssociations.AddRange(associationsPropagatingAttributes(topAssociation, false));
            resultAssociations.Add(null);
            // 3. pridame asociace na element atributy
            if (topAssociation.Child is PSMClass)
            {
                foreach (PSMAttribute attribute in ((PSMClass)topAssociation.Child).GetActualPSMAttributes())
                {
                    if (attribute.Element)
                    {
                        PSMAssociation association = new PSMAssociation(topAssociation.Project, Guid.NewGuid());
                        association.Lower = attribute.Lower;
                        association.Upper = attribute.Upper;
                        association.Name = attribute.Name;
                        PSMClass fakeAttributeClass = new PSMClass(topAssociation.Project, Guid.NewGuid());
                        fakeAttributeClass.PSMAttributes.Add(attribute);
                        fakeAttributeClass.ParentAssociation = association;
                        association.Child = fakeAttributeClass;
                        resultAssociations.Add(association);
                    }
                }
            }
            // 4. pridame asociace propagujici element atributy
            resultAssociations.AddRange(associationsPropagatingAttributes(topAssociation, true));
            resultAssociations.Add(null);
            // 5. ostatni asociace
            resultAssociations.AddRange(associationsPropagatingElements(topAssociation));

            return resultAssociations;
        }

        /**
         * Vraci asociace, ktere propaguji atributy do asociace topAssociation.
         * 
         * atribute elementAttribute urcije, zda vracime asociace propagujci element atributy, nebo pouze obycejne atributy
         **/
        private static List<PSMAssociation> associationsPropagatingAttributes(PSMAssociation topAssociation, bool elementAttributes)
        {
            List<PSMAssociation> resultAssociations = new List<PSMAssociation>();
            IEnumerable<PSMAssociation> associations = null;
            if (topAssociation.Child is PSMClass)
            {
                associations = ((PSMClass)topAssociation.Child).GetActualChildPSMAssociations();
            }
            else
                if (topAssociation.Child is PSMContentModel)
                {
                    associations = ((PSMContentModel)topAssociation.Child).ChildPSMAssociations;
                }
            foreach (PSMAssociation association in associations)
            {
                if (!association.IsNamed)
                {
                    if (association.Child is PSMClass && !((PSMClass)association.Child).Final && !association.IsNonTreeAssociation)
                    {
                        if (((PSMClass)association.Child).GetActualPSMAttributes().Count() > 0)
                        {
                            foreach (PSMAttribute att in ((PSMClass)association.Child).GetActualPSMAttributes())
                            {
                                if (att.Element && elementAttributes || !att.Element && !elementAttributes)
                                {
                                    resultAssociations.Add(association);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            resultAssociations.AddRange(associationsPropagatingAttributes(association, elementAttributes));
                        }
                    }
                    else
                    {
                        if (association.Child is PSMContentModel)
                        {
                            List<PSMAssociation> associationForContentModel = associationsPropagatingAttributes(association, elementAttributes);
                            if (associationForContentModel.Count > 0)
                                resultAssociations.Add(association);
                        }
                    }
                }
            }

            return resultAssociations;
        }

        /**
         * Vraci asociace, ktere propaguji elementy do asociace topAssociation.
         **/
        private static List<PSMAssociation> associationsPropagatingElements(PSMAssociation topAssociation)
        {
            List<PSMAssociation> resultAssociations = new List<PSMAssociation>();
            IEnumerable<PSMAssociation> associations = null;
            if (topAssociation.Child is PSMClass)
            {
                associations = ((PSMClass)topAssociation.Child).GetActualChildPSMAssociations();
            }
            else
                if (topAssociation.Child is PSMContentModel)
                {
                    associations = ((PSMContentModel)topAssociation.Child).ChildPSMAssociations;
                }
            foreach (PSMAssociation association in associations)
            {
                if (!association.IsNamed)
                {
                    if (association.IsNonTreeAssociation)
                        resultAssociations.Add(association);
                    else
                        if (association.Child is PSMClass && !((PSMClass)association.Child).Final)
                        {
                            resultAssociations.AddRange(associationsPropagatingElements(association));
                        }
                        else
                        {
                            if (association.Child is PSMContentModel)
                            {
                                List<PSMAssociation> associationForContentModel = associationsPropagatingElements(association);
                                if (associationForContentModel.Count > 0)
                                    resultAssociations.Add(association);
                            }
                        }
                }
                else
                {
                    resultAssociations.Add(association);
                }
            }

            return resultAssociations;
        }

        /**
         * Vraci asociace, ktere ovlivnuji asociaci association podle toho, v jake casti tvorby konecneho automatu se nachazime.
         * 
         * atribut mode urcuje, v jake casti tvorby konecneho automatu se nachazime
         **/
        public static List<PSMAssociation> getAssociations(PSMAssociation association, CreationPhase mode)
        {
            List<PSMAssociation> associations = null;
            switch (mode)
            {
                case CreationPhase.ATTRIBUTE:
                    associations = AssociationsUtils.associationsPropagatingAttributes(association, false);
                    break;
                case CreationPhase.ELEMENT_ATTRIBUTE:
                    associations = AssociationsUtils.associationsPropagatingAttributes(association, true);
                    break;
                case CreationPhase.ELEMENT:
                    associations = AssociationsUtils.associationsPropagatingElements(association);
                    break;
                case CreationPhase.CONTENT_MODEL:
                    associations = AssociationsUtils.associationsPropagatingAttributes(association, true);
                    associations.AddRange(AssociationsUtils.associationsPropagatingElements(association));
                    break;
            }
            return associations;
        }

        public static List<PSMAssociation> getAllContributingAssociations(PSMAssociation topAssociation, HashSet<PSMAssociation> visitedAssociations, CreationPhase mode)
        {
            List<PSMAssociation> result = new List<PSMAssociation>();
            IEnumerable<PSMAssociation> associations = null;
            if (topAssociation.Child is PSMClass)
            {
                associations = ((PSMClass)topAssociation.Child).GetActualChildPSMAssociations();
            }
            else
                if (topAssociation.Child is PSMContentModel)
                {
                    associations = ((PSMContentModel)topAssociation.Child).ChildPSMAssociations;
                }
            foreach (PSMAssociation association in associations)
            {
                if (!association.IsNamed)
                {
                    if (!visitedAssociations.Contains(association))
                    {
                        visitedAssociations.Add(association);
                        switch (mode)
                        {
                            case CreationPhase.ATTRIBUTE:
                                if (association.Child is PSMClass)
                                {
                                    foreach (PSMAttribute att in ((PSMClass)association.Child).PSMAttributes)
                                    {
                                        if (!att.Element)
                                        {
                                            result.Add(association);
                                            break;
                                        }
                                    }
                                }
                                break;
                            case CreationPhase.ELEMENT:
                                break;
                            case CreationPhase.CONTENT_MODEL:
                            case CreationPhase.ELEMENT_ATTRIBUTE:
                                if (association.Child is PSMClass)
                                {
                                    foreach (PSMAttribute att in ((PSMClass)association.Child).PSMAttributes)
                                    {
                                        if (att.Element)
                                        {
                                            result.Add(association);
                                            break;
                                        }
                                    }
                                }
                                break;

                        }
                        result.AddRange(getAllContributingAssociations(association, visitedAssociations, mode));
                    }
                }
                else
                {
                    switch (mode)
                    {
                        case CreationPhase.ELEMENT_ATTRIBUTE:
                        case CreationPhase.ATTRIBUTE:
                            break;
                        case CreationPhase.CONTENT_MODEL:
                        case CreationPhase.ELEMENT:
                            result.Add(association);
                            break;
                    }
                }
            }
            return result;
        }

        /**
         *  Metoda vraci seznam pojmenovanych asociaci a nepojmenovanych rekurzivnich asociaci, ktere ovlivnuji asociaci topAssociation.
         *  
         * atribut removeAttributes urcuje, zda chceme do vysledku zahrnout i nepojmenovane asociace propagujci atributy
         **/
        public static List<PSMAssociation> namedAssociations(PSMAssociation topAssociation, bool removeAttributes)
        {
            return namedAssociations(topAssociation, removeAttributes, true);
        }

        /**
         *  Metoda vraci seznam pojmenovanych asociaci a nepojmenovanych rekurzivnich asociaci, ktere ovlivnuji asociaci topAssociation.
         *  
         * atribut removeAttributes urcuje, zda chceme do vysledku zahrnout i nepojmenovane asociace propagujci atributy
         * atribut removeRecursion urcuje, zda chceme z vysledku odebrat nepojmenovanou rekurzivni asociace
         **/
        public static List<PSMAssociation> namedAssociations(PSMAssociation topAssociation, bool removeAttributes, bool removeRecursion)
        {
            return contributingAssociations(topAssociation, removeAttributes, removeRecursion);
        }

        private static List<PSMAssociation> contributingAssociations(PSMAssociation topAssociation, bool removeAttributes, bool removeRecursion)
        {
            List<PSMAssociation> resultAssociations = new List<PSMAssociation>();
            IEnumerable<PSMAssociation> associations = null;
            if (topAssociation.Child is PSMClass)
            {
                associations = ((PSMClass)topAssociation.Child).GetActualChildPSMAssociations();
            }
            else
                if (topAssociation.Child is PSMContentModel)
                {
                    associations = ((PSMContentModel)topAssociation.Child).ChildPSMAssociations;
                }
            foreach (PSMAssociation association in associations)
            {
                if (!association.IsNamed)
                {
                    if (association.IsNonTreeAssociation)
                    {
                        if (!removeRecursion)
                            resultAssociations.Add(association);
                    }
                    else
                    {
                        if (association.Child is PSMClass && ((PSMClass)association.Child).GetActualPSMAttributes().Count() > 0 && !removeAttributes)
                        {
                            resultAssociations.Add(association);
                        }
                        List<PSMAssociation> contribution = contributingAssociations(association, removeAttributes, removeRecursion);
                        resultAssociations.AddRange(contribution);
                    }
                }
                else
                {
                    resultAssociations.Add(association);
                }
            }

            return resultAssociations;
        }

    }
}
