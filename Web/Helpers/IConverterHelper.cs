using System;
using System.Threading.Tasks;
using Web.Data.Entities;
using Web.Models;

namespace Web.Helpers
{
    public interface IConverterHelper
    {
        Task<User> ToUserAsync(UserViewModel model, string imageId, bool isNew);

        UserViewModel ToUserViewModel(User user);
    }
}
