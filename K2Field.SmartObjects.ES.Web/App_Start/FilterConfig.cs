using System.Web;
using System.Web.Mvc;

namespace K2Field.SmartObjects.ES.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
