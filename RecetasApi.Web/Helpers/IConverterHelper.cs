using System;
using System.Threading.Tasks;
using RecetasApi.Web.Data.Entities;
using RecetasApi.Web.Models;

namespace RecetasApi.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<User> ToUserAsync(UserViewModel model, string imageId, bool isNew);

        UserViewModel ToUserViewModel(User user);
    }
}
