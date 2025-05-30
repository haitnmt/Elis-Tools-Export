using Haihv.Elis.Tools.Export.App.Shared.Services;

namespace Haihv.Elis.Tools.Export.App.Web.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
