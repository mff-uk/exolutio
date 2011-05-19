using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;
using EvoX.Controller.Commands;
using EvoX.Web;
using EvoX.Web.ModelHelper;

namespace EvoXWeb.EvoX
{
    /// <summary>
    /// Summary description for EvoXWS
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class EvoXWS : System.Web.Services.WebService
    {

        private static List<string> allOperations = null;

        public static List<string> AllOperations
        {
            get
            {
                if (allOperations == null)
                {
                    allOperations = new List<string>();
                    
                    Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> availableOperations = ModelHelper.GetAvailableOperations();

                    foreach (KeyValuePair<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> kvp in availableOperations)
                    {
                        allOperations.Add(kvp.Value.Item1);
                    }
                }
                return allOperations;
            }
        }

        [WebMethod]
        public string[] GetOperationsList(string prefixText)
        {
            return AllOperations.Where(opname => opname.Contains(prefixText)).ToArray();
        }
    }
}
