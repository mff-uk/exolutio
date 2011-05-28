using System;
using System.IO;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.Serialization;
#if SILVERLIGHT
#else 
using NUnit.Framework;
using Tests.ModelIntegrity;
using Exolutio.Controller;
#endif

using Version = Exolutio.Model.Versioning.Version;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

using System.Collections.Generic;
namespace Tests
{
	public static class TestUtils
	{
        public static Project CreateSimpleSampleProject()
        {
            Project p = new Project();
            p.InitNewEmptyProject();
            PSMSchema Spsm1 = new PSMSchema(p);
            p.SingleVersion.PSMSchemas.Add(Spsm1);

            PIMSchema sPIM = new PIMSchema(p);
            p.SingleVersion.PIMSchema = sPIM;

            PIMClass pimcProduct = new PIMClass(p, sPIM) { Name = "Product" };
            PIMClass pimcItem = new PIMClass(p, sPIM) { Name = "Item" };

            PIMAssociation a = new PIMAssociation(p, sPIM, pimcItem, pimcProduct);
            a.PIMAssociationEnds[0].Upper = 2;
            a.PIMAssociationEnds[1].Upper = UnlimitedInt.Infinity;

            AttributeType stringType = new AttributeType(p) { Name = "string", IsSealed = true, XSDDefinition = "string" };
            p.SingleVersion.AttributeTypes.Add(stringType);
            #if SILVERLIGHT
            #else
            ModelConsistency.CheckProject(p);
            #endif
            return p;
        }

        public static Project CreateSampleProject()
	    {
	        //return CreateSimpleSampleProject();

            Project p = new Project();
            p.InitNewEmptyProject();
            PIMSchema sPIM = new PIMSchema(p);

	        AttributeType stringType = new AttributeType(p) {Name = "string", IsSealed = true, XSDDefinition = "string"};
            p.SingleVersion.AttributeTypes.Add(stringType);

            p.SingleVersion.PIMSchema = sPIM;
            PIMClass pimcProduct = new PIMClass(p, sPIM) { Name = "Product" };
            PIMClass pimcCustomer = new PIMClass(p, sPIM) { Name = "Customer" };
            PIMClass pimcPurchase = new PIMClass(p, sPIM) { Name = "Purchase" };
            PIMClass pimcItem = new PIMClass(p, sPIM) { Name = "Item" };
            PIMClass pimcAddress = new PIMClass(p, sPIM) { Name = "Address" };

	        PIMAttribute pimattProduct_title = new PIMAttribute(p, pimcProduct, sPIM) { Name = "title", AttributeType = stringType };
            PIMAttribute pimattProduct_price = new PIMAttribute(p, pimcProduct, sPIM) { Name = "price" };
            PIMAttribute pimattCustomer_name = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "name"};
            PIMAttribute pimattCustomer_email = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "email", Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAttribute pimattCustomer_phone = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "phone", Lower = 0, Upper = UnlimitedInt.Infinity };
            PIMAttribute pimattItem_tester = new PIMAttribute(p, pimcItem, sPIM) { Name = "tester" };
            PIMAttribute pimattItem_itemprice = new PIMAttribute(p, pimcItem, sPIM) { Name = "itemprice" };
            PIMAttribute pimattItem_amount = new PIMAttribute(p, pimcItem, sPIM) { Name = "amount" };
            PIMAttribute pimattPurchase_code = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "code" };
            PIMAttribute pimattPurchase_create_date = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "create-date" };
            PIMAttribute pimattPurchase_status = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "status" };
            PIMAttribute pimattAddress_street = new PIMAttribute(p, pimcAddress, sPIM) { Name = "street" };
            PIMAttribute pimattAddress_city = new PIMAttribute(p, pimcAddress, sPIM) { Name = "city" };
            PIMAttribute pimattAddress_country = new PIMAttribute(p, pimcAddress, sPIM) { Name = "country" };
            PIMAttribute pimattAddress_gps = new PIMAttribute(p, pimcAddress, sPIM) { Name = "gps" };

            PIMAssociationEnd pimaeProduct1 = new PIMAssociationEnd(p, pimcProduct, sPIM);
            PIMAssociationEnd pimaeItem1 = new PIMAssociationEnd(p, pimcItem, sPIM) { Lower = 0, Upper = UnlimitedInt.Infinity };
            PIMAssociation pimaProduct_Item = new PIMAssociation(p, sPIM, pimaeProduct1, pimaeItem1);

            PIMAssociationEnd pimaeItem2 = new PIMAssociationEnd(p, pimcItem, sPIM);
            PIMAssociationEnd pimaePurchase1 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAssociation pimaItem_Purchase = new PIMAssociation(p, sPIM, pimaeItem2, pimaePurchase1);

            PIMAssociationEnd pimaePurchase2 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAssociationEnd pimaeCustomer1 = new PIMAssociationEnd(p, pimcCustomer, sPIM);
            PIMAssociation pimaMakes = new PIMAssociation(p, sPIM, pimaePurchase2, pimaeCustomer1) { Name = "makes" };

            PIMAssociationEnd pimaePurchase3 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociationEnd pimaeAddress1 = new PIMAssociationEnd(p, pimcAddress, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociation pimaShipTo = new PIMAssociation(p, sPIM, pimaePurchase3, pimaeAddress1) { Name = "ship-to" };

            PIMAssociationEnd pimaePurchase4 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociationEnd pimaeAddress2 = new PIMAssociationEnd(p, pimcAddress, sPIM);
            PIMAssociation pimaBillTo = new PIMAssociation(p, sPIM, pimaePurchase4, pimaeAddress2) { Name = "bill-to" };

            PSMSchema sPSM1 = new PSMSchema(p);
            p.SingleVersion.PSMSchemas.Add(sPSM1);

            PSMSchemaClass sPSM1C = new PSMSchemaClass(p, sPSM1) { Name = "PSMSchemaClass1" };

            PSMClass psmcAddress = new PSMClass(p, sPSM1) { Name = "Address", Interpretation = pimcAddress };
            //sPSM1.Roots.Add(psmcAddress);

            PSMAttribute psmattStreet = new PSMAttribute(p, psmcAddress, sPSM1) { Name = "street", Element = true, Interpretation = pimattAddress_street };
            PSMAttribute psmattCity = new PSMAttribute(p, psmcAddress, sPSM1) { Name = "city", Element = true, Interpretation = pimattAddress_city };
            
            PSMClass psmcPurchase = new PSMClass(p, sPSM1) { Name = "Purchase", Interpretation = pimcPurchase };

            PSMAssociation psmaPurchase = new PSMAssociation(p, sPSM1C, psmcPurchase, sPSM1) { Name = "purchase" };

            PSMAttribute psmattPurchase_code = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "code", Element = false, Interpretation = pimattPurchase_code };
            PSMAttribute psmattPurchase_create_date = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "create-date", Element = true, Interpretation = pimattPurchase_create_date };
            PSMAttribute psmattPurchase_version = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "version", Element = false };

            PSMClass psmcBillAddress = new PSMClass(p, sPSM1) { Name = "BillAddress", Interpretation = pimcAddress, RepresentedClass = psmcAddress };
            PSMClass psmcShipAddress = new PSMClass(p, sPSM1) { Name = "ShipAddress", Interpretation = pimcAddress, RepresentedClass = psmcAddress };
            PSMClass psmcCustomer = new PSMClass(p, sPSM1) { Name = "Customer", Interpretation = pimcCustomer };
            PSMClass psmcItems = new PSMClass(p, sPSM1) { Name = "Items"};

            PSMAttribute psmattCustomer_name = new PSMAttribute(p, psmcCustomer, sPSM1) { Name = "name", Element = true, Interpretation = pimattCustomer_name };
            
            PSMAssociation psmaBillTo = new PSMAssociation(p, psmcPurchase, psmcBillAddress, sPSM1) { Name = "bill-to", Interpretation = pimaBillTo };
            PSMAssociation psmaShipTo = new PSMAssociation(p, psmcPurchase, psmcShipAddress, sPSM1) { Name = "ship-to", Interpretation = pimaShipTo, Lower = 0 };
            PSMAssociation psmacust = new PSMAssociation(p, psmcPurchase, psmcCustomer, sPSM1) { Name = "cust", Interpretation = pimaMakes };
            PSMAssociation psmaItems = new PSMAssociation(p, psmcPurchase, psmcItems, sPSM1) { Name = "items" };

            PSMClass psmcContact = new PSMClass(p, sPSM1) { Name = "Contact" };

            PSMAttribute psmattContactEmail = new PSMAttribute(p, psmcContact, sPSM1) { Name = "email", Lower = 1, Upper = UnlimitedInt.Infinity, Interpretation = pimattCustomer_email, Element = true };
            PSMAttribute psmattContactPhone = new PSMAttribute(p, psmcContact, sPSM1) { Name = "phone", Lower = 0, Upper = UnlimitedInt.Infinity, Interpretation = pimattCustomer_phone, Element = true };

            PSMAssociation psmaCustomerContact = new PSMAssociation(p, psmcCustomer, psmcContact, sPSM1);

            PSMClass psmcItem = new PSMClass(p, sPSM1) { Name = "Item", Interpretation = pimcItem };
            PSMAssociation psmaItemsItem = new PSMAssociation(p, psmcItems, psmcItem, sPSM1) { Name = "item", Lower = 1, Upper = UnlimitedInt.Infinity, Interpretation = pimaItem_Purchase };

            PSMContentModel psmcm1 = new PSMContentModel(p, sPSM1) { Type = PSMContentModelType.Choice };
            PSMAssociation psmaPSMC1 = new PSMAssociation(p, psmcItem, psmcm1, sPSM1);

            PSMClass psmcProduct = new PSMClass(p, sPSM1) { Name = "Product", Interpretation = pimcProduct };
            PSMAttribute psmattProduct_code = new PSMAttribute(p, psmcProduct, sPSM1) { Name = "code", Element = true, Interpretation = pimattProduct_title };
            PSMAssociation psmaPSMC1Product = new PSMAssociation(p, psmcm1, psmcProduct, sPSM1) { Interpretation = pimaProduct_Item };

            PSMClass psmcItemTester = new PSMClass(p, sPSM1) { Name = "ItemTester" };
            PSMAttribute psmattItemTester_tester = new PSMAttribute(p, psmcItemTester, sPSM1) { Name = "tester", Element = false, Interpretation = pimattItem_tester };
            PSMAssociation psmaPSMC1ItemTester = new PSMAssociation(p, psmcm1, psmcItemTester, sPSM1);

            PSMClass psmcItemPricing = new PSMClass(p, sPSM1) { Name = "ItemPricing" };
            PSMAttribute psmattItemPricing_price = new PSMAttribute(p, psmcItemPricing, sPSM1) { Name = "price", Element = true, Interpretation = pimattItem_itemprice };
            PSMAttribute psmattItemPricing_amount = new PSMAttribute(p, psmcItemPricing, sPSM1) { Name = "amount", Element = true, Interpretation = pimattItem_amount };
            PSMAssociation psmaPSMC1ItemPricing = new PSMAssociation(p, psmcm1, psmcItemPricing, sPSM1);

            p.HasUnsavedChanges = true;

            // classes were added to roots... 
            foreach (PSMAssociationMember member in sPSM1.Roots.ToArray())
            {
                if (member.ParentAssociation != null)
                {
                    sPSM1.Roots.Remove(member);
                }
            }

            #if SILVERLIGHT
            #else
            ModelConsistency.CheckProject(p);
            #endif

            return p;
        }

        #if SILVERLIGHT
        #else
        public static void CommandTest()
        {
            Project p = new Project();
            p.InitNewEmptyProject();
            PIMSchema sPIM = new PIMSchema(p);

            AttributeType stringType = new AttributeType(p) { Name = "string", IsSealed = true, XSDDefinition = "string" };
            p.SingleVersion.AttributeTypes.Add(stringType);

            p.SingleVersion.PIMSchema = sPIM;
            PIMClass pimcProduct = new PIMClass(p, sPIM) { Name = "Product" };
            PIMClass pimcCustomer = new PIMClass(p, sPIM) { Name = "Customer" };
            PIMClass pimcPurchase = new PIMClass(p, sPIM) { Name = "Purchase" };
            PIMClass pimcItem = new PIMClass(p, sPIM) { Name = "Item" };
            PIMClass pimcAddress = new PIMClass(p, sPIM) { Name = "Address" };

            PIMAttribute pimattProduct_title = new PIMAttribute(p, pimcProduct, sPIM) { Name = "title", AttributeType = stringType };
            PIMAttribute pimattProduct_price = new PIMAttribute(p, pimcProduct, sPIM) { Name = "price" };
            PIMAttribute pimattCustomer_name = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "name" };
            PIMAttribute pimattCustomer_email = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "email", Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAttribute pimattCustomer_phone = new PIMAttribute(p, pimcCustomer, sPIM) { Name = "phone", Lower = 0, Upper = UnlimitedInt.Infinity };
            PIMAttribute pimattItem_tester = new PIMAttribute(p, pimcItem, sPIM) { Name = "tester" };
            PIMAttribute pimattItem_itemprice = new PIMAttribute(p, pimcItem, sPIM) { Name = "itemprice" };
            PIMAttribute pimattItem_amount = new PIMAttribute(p, pimcItem, sPIM) { Name = "amount" };
            PIMAttribute pimattPurchase_code = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "code" };
            PIMAttribute pimattPurchase_create_date = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "create-date" };
            PIMAttribute pimattPurchase_status = new PIMAttribute(p, pimcPurchase, sPIM) { Name = "status" };
            PIMAttribute pimattAddress_street = new PIMAttribute(p, pimcAddress, sPIM) { Name = "street" };
            PIMAttribute pimattAddress_city = new PIMAttribute(p, pimcAddress, sPIM) { Name = "city" };
            PIMAttribute pimattAddress_country = new PIMAttribute(p, pimcAddress, sPIM) { Name = "country" };
            PIMAttribute pimattAddress_gps = new PIMAttribute(p, pimcAddress, sPIM) { Name = "gps" };

            PIMAssociationEnd pimaeProduct1 = new PIMAssociationEnd(p, pimcProduct, sPIM);
            PIMAssociationEnd pimaeItem1 = new PIMAssociationEnd(p, pimcItem, sPIM) { Lower = 0, Upper = UnlimitedInt.Infinity };
            PIMAssociation pimaProduct_Item = new PIMAssociation(p, sPIM, pimaeProduct1, pimaeItem1);

            PIMAssociationEnd pimaeItem2 = new PIMAssociationEnd(p, pimcItem, sPIM);
            PIMAssociationEnd pimaePurchase1 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAssociation pimaItem_Purchase = new PIMAssociation(p, sPIM, pimaeItem2, pimaePurchase1);

            PIMAssociationEnd pimaePurchase2 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 1, Upper = UnlimitedInt.Infinity };
            PIMAssociationEnd pimaeCustomer1 = new PIMAssociationEnd(p, pimcCustomer, sPIM);
            PIMAssociation pimaMakes = new PIMAssociation(p, sPIM, pimaePurchase2, pimaeCustomer1) { Name = "makes" };

            PIMAssociationEnd pimaePurchase3 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociationEnd pimaeAddress1 = new PIMAssociationEnd(p, pimcAddress, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociation pimaShipTo = new PIMAssociation(p, sPIM, pimaePurchase3, pimaeAddress1) { Name = "ship-to" };

            PIMAssociationEnd pimaePurchase4 = new PIMAssociationEnd(p, pimcPurchase, sPIM) { Lower = 0, Upper = 1 };
            PIMAssociationEnd pimaeAddress2 = new PIMAssociationEnd(p, pimcAddress, sPIM);
            PIMAssociation pimaBillTo = new PIMAssociation(p, sPIM, pimaePurchase4, pimaeAddress2) { Name = "bill-to" };

            PSMSchema sPSM1 = new PSMSchema(p);
            p.SingleVersion.PSMSchemas.Add(sPSM1);

            PSMSchemaClass sPSM1C = new PSMSchemaClass(p, sPSM1) { Name = "PSMSchemaClass1" };

            PSMClass psmcAddress = new PSMClass(p, sPSM1) { Name = "Address", Interpretation = pimcAddress };
            sPSM1.Roots.Add(psmcAddress);

            PSMAttribute psmattStreet = new PSMAttribute(p, psmcAddress, sPSM1) { Name = "street", Element = true, Interpretation = pimattAddress_street };
            PSMAttribute psmattCity = new PSMAttribute(p, psmcAddress, sPSM1) { Name = "city", Element = true, Interpretation = pimattAddress_city };

            PSMClass psmcPurchase = new PSMClass(p, sPSM1) { Name = "Purchase", Interpretation = pimcPurchase };

            PSMAssociation psmaPurchase = new PSMAssociation(p, sPSM1C, psmcPurchase, sPSM1) { Name = "purchase" };

            PSMAttribute psmattPurchase_code = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "code", Element = false, Interpretation = pimattPurchase_code };
            PSMAttribute psmattPurchase_create_date = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "create-date", Element = true, Interpretation = pimattPurchase_create_date };
            PSMAttribute psmattPurchase_version = new PSMAttribute(p, psmcPurchase, sPSM1) { Name = "version", Element = false };

            PSMClass psmcBillAddress = new PSMClass(p, sPSM1) { Name = "BillAddress", Interpretation = pimcAddress, RepresentedClass = psmcAddress };
            PSMClass psmcShipAddress = new PSMClass(p, sPSM1) { Name = "ShipAddress", Interpretation = pimcAddress, RepresentedClass = psmcAddress };
            PSMClass psmcCustomer = new PSMClass(p, sPSM1) { Name = "Customer", Interpretation = pimcCustomer };
            PSMClass psmcItems = new PSMClass(p, sPSM1) { Name = "Items" };

            PSMAttribute psmattCustomer_name = new PSMAttribute(p, psmcCustomer, sPSM1) { Name = "name", Element = true, Interpretation = pimattCustomer_name };

            PSMAssociation psmaBillTo = new PSMAssociation(p, psmcPurchase, psmcBillAddress, sPSM1) { Name = "bill-to", Interpretation = pimaBillTo };
            PSMAssociation psmaShipTo = new PSMAssociation(p, psmcPurchase, psmcShipAddress, sPSM1) { Name = "ship-to", Interpretation = pimaShipTo, Lower = 0 };
            PSMAssociation psmacust = new PSMAssociation(p, psmcPurchase, psmcCustomer, sPSM1) { Name = "cust", Interpretation = pimaMakes };
            PSMAssociation psmaItems = new PSMAssociation(p, psmcPurchase, psmcItems, sPSM1) { Name = "items" };

            PSMClass psmcContact = new PSMClass(p, sPSM1) { Name = "Contact" };

            PSMAttribute psmattContactEmail = new PSMAttribute(p, psmcContact, sPSM1) { Name = "email", Lower = 1, Upper = UnlimitedInt.Infinity, Interpretation = pimattCustomer_email, Element = true };
            PSMAttribute psmattContactPhone = new PSMAttribute(p, psmcContact, sPSM1) { Name = "phone", Lower = 0, Upper = UnlimitedInt.Infinity, Interpretation = pimattCustomer_phone, Element = true };

            PSMAssociation psmaCustomerContact = new PSMAssociation(p, psmcCustomer, psmcContact, sPSM1);

            PSMClass psmcItem = new PSMClass(p, sPSM1) { Name = "Item", Interpretation = pimcItem };
            PSMAssociation psmaItemsItem = new PSMAssociation(p, psmcItems, psmcItem, sPSM1) { Name = "item", Lower = 1, Upper = UnlimitedInt.Infinity, Interpretation = pimaItem_Purchase };

            PSMContentModel psmcm1 = new PSMContentModel(p, sPSM1) { Type = PSMContentModelType.Choice };
            PSMAssociation psmaPSMC1 = new PSMAssociation(p, psmcItem, psmcm1, sPSM1);

            PSMClass psmcProduct = new PSMClass(p, sPSM1) { Name = "Product", Interpretation = pimcProduct };
            PSMAttribute psmattProduct_code = new PSMAttribute(p, psmcProduct, sPSM1) { Name = "code", Element = true, Interpretation = pimattProduct_title };
            PSMAssociation psmaPSMC1Product = new PSMAssociation(p, psmcm1, psmcProduct, sPSM1) { Interpretation = pimaProduct_Item };

            PSMClass psmcItemTester = new PSMClass(p, sPSM1) { Name = "ItemTester" };
            PSMAttribute psmattItemTester_tester = new PSMAttribute(p, psmcItemTester, sPSM1) { Name = "tester", Element = false, Interpretation = pimattItem_tester };
            PSMAssociation psmaPSMC1ItemTester = new PSMAssociation(p, psmcm1, psmcItemTester, sPSM1);

            PSMClass psmcItemPricing = new PSMClass(p, sPSM1) { Name = "ItemPricing" };
            PSMAttribute psmattItemPricing_price = new PSMAttribute(p, psmcItemPricing, sPSM1) { Name = "price", Element = true, Interpretation = pimattItem_itemprice };
            PSMAttribute psmattItemPricing_amount = new PSMAttribute(p, psmcItemPricing, sPSM1) { Name = "amount", Element = true, Interpretation = pimattItem_amount };
            PSMAssociation psmaPSMC1ItemPricing = new PSMAssociation(p, psmcm1, psmcItemPricing, sPSM1);

            PSMSchema sPSM2 = new PSMSchema(p);
            p.SingleVersion.PSMSchemas.Add(sPSM2);
            p.HasUnsavedChanges = true;

            Controller c = new Controller(p);

            Guid sPSM1Guid = sPSM1;

            ModelConsistency.CheckProject(p);

            Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers.cmdDeletePIMClass command = new Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers.cmdDeletePIMClass(c);
            command.Set(pimcItem);
            command.Execute();

            Exolutio.Controller.Commands.UndoCommand undo = new Exolutio.Controller.Commands.UndoCommand(c);
            undo.Execute();

            Exolutio.Controller.Commands.RedoCommand redo = new Exolutio.Controller.Commands.RedoCommand(c);
            redo.Execute();

            /*cmdDeletePSMSchema command = new cmdDeletePSMSchema(c);

            command.Set(sPSM1);

            command.Execute();
            ModelConsistency.CheckProject(p);

            try
            {
                p.TranslateComponent<PSMSchema>(sPSM1Guid);
            }
            catch {  }

            command.UnExecute();

            sPSM1 = p.TranslateComponent<PSMSchema>(sPSM1Guid);
            ModelConsistency.CheckProject(p);

            command.ExecuteAsRedo();*/
        }
        
        public static Project CreateSampleProject3Versions()
        {
            Project sampleProject = CreateSampleProject();
            
            ModelConsistency.CheckProject(sampleProject);
            sampleProject.StartVersioning();
            Version secondVersion = new Version(sampleProject) { Label = "v2", Number = 2 };
            sampleProject.VersionManager.BranchProject(sampleProject.GetProjectVersion(sampleProject.VersionManager.Versions[0]), secondVersion);
            Version thirdVersion = new Version(sampleProject) { Label = "v3", Number = 3 };
            sampleProject.VersionManager.BranchProject(sampleProject.GetProjectVersion(sampleProject.VersionManager.Versions[1]), thirdVersion);

            ModelConsistency.CheckProject(sampleProject);

            return sampleProject;
        }

        public static Project CreateSimpleSampleProject3Versions()
        {
            Project sampleProject = CreateSimpleSampleProject();

            sampleProject.StartVersioning();
            Version secondVersion = new Version(sampleProject) { Label = "v2", Number = 2 };
            sampleProject.VersionManager.BranchProject(sampleProject.GetProjectVersion(sampleProject.VersionManager.Versions[0]), secondVersion);
            Version thirdVersion = new Version(sampleProject) { Label = "v3", Number = 3 };
            sampleProject.VersionManager.BranchProject(sampleProject.GetProjectVersion(sampleProject.VersionManager.Versions[1]), thirdVersion);

            ModelConsistency.CheckProject(sampleProject);

            return sampleProject;
        }

		public static void AssertException<CustomException> (Action action)
			where CustomException : System.Exception
		{
			try
			{
				action();
			}
			catch (CustomException)
			{
				return;
			}
			catch (Exception e)
			{
				Assert.Fail(string.Format("Expected exception of type: {0}. Exception of type {1} occured instead.", 
					typeof(CustomException).Name, e.GetType().Name));
				return; 
			}
			Assert.Fail(string.Format("Expected exception of type: {0}.",
					typeof(CustomException).Name));
		}

        public static void LoadSaveAndCompare(string fileName)
        {
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project loadedProject = m.LoadProject(fileName);
            ModelIntegrity.ModelConsistency.CheckProject(loadedProject);

            CollectionAssert.IsEmpty(m.Log, "Log contains errors or warnings");

            string withoutSuffix = Path.GetFileNameWithoutExtension(fileName);
            string suffix = Path.GetExtension(fileName);
            string copyName = string.Format("{0}-copy.{1}", withoutSuffix, suffix);
            m.SaveProject(loadedProject, copyName);
            CollectionAssert.IsEmpty(m.Log, "Log contains errors or warnings");

            FileAssert.AreEqual(fileName, copyName);
        }
        #endif
    }
}
