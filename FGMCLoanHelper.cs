
namespace FGMCDocumentCopy.Helpers
{
    public class FgmcLoanHelper
    {
        public EllieMae.Encompass.BusinessObjects.DataObject GetCustomDataObject(string dataObjectName, EllieMae.Encompass.Client.Session session)
        {
            EllieMae.Encompass.BusinessObjects.DataObject dataObject = null;
            if (session == null) { return dataObject; }
            if (dataObjectName == null || dataObjectName.Length <= 0) { return dataObject; }
            dataObject = session.DataExchange.GetCustomDataObject(dataObjectName);
            return dataObject;
        }
    }
}
