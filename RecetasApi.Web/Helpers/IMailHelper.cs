using RecetasApi.Common.Models;

namespace RecetasApi.Web.Helpers
{
    public interface IMailHelper
    {
        Response SendMail(string to, string subject, string body);
    }
}
