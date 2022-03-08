using System;
using System.Threading.Tasks;
using RecetasApi.Web.Data;
using RecetasApi.Web.Data.Entities;
using RecetasApi.Web.Models;

namespace RecetasApi.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;

        public ConverterHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<User> ToUserAsync(UserViewModel model, string imageId, bool isNew)
        {
            return new User
            {
                Document = model.Document,
                Email = model.Email,
                FirstName = model.FirstName,
                Id = isNew ? Guid.NewGuid().ToString() : model.Id,
                ImageId = imageId,
                LastName = model.LastName,
                UserName = model.Email,
                UserType = model.UserType,
            };
        }

        public UserViewModel ToUserViewModel(User user)
        {
            return new UserViewModel
            {
                Document = user.Document,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                ImageId = user.ImageId,
                LastName = user.LastName,
                UserType = user.UserType,
            };
        }
    }
}
