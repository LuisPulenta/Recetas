using System;
using System.Threading.Tasks;
using Web.Data;
using Web.Data.Entities;
using Web.Models;

namespace Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;

        public ConverterHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<User> ToUserAsync(UserViewModel model, string Photo, bool isNew)
        {
            return new User
            {
                Document = model.Document,
                Email = model.Email,
                FirstName = model.FirstName,
                Id = isNew ? Guid.NewGuid().ToString() : model.Id,
                Photo = Photo,
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
                Photo = user.Photo,
                LastName = user.LastName,
                UserType = user.UserType,
            };
        }
    }
}
