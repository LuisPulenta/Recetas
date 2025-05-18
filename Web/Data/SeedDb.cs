using System.Threading.Tasks;
using Web.Data.Entities;
using Web.Helpers;
using Common.Enums;

namespace Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckRolesAsycn();
            await CheckUserAsync("1010", "Luis", "Núñez", "luis@yopmail.com", UserType.Admin);
            await CheckUserAsync("2020", "Pablo", "Lacuadri", "pablo@yopmail.com", UserType.Admin);
            await CheckUserAsync("3030", "Lionel", "Messi", "messi@yopmail.com", UserType.User);
            await CheckUserAsync("4040", "Diego", "Maradona", "maradona@yopmail.com",UserType.User);
        }

        private async Task CheckRolesAsycn()
        {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckUserAsync(string document, string firstName, string lastName, string email,UserType userType)
        {
            User user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Document = document,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = email,
                    UserType = userType
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

            }
        }
    }
}
