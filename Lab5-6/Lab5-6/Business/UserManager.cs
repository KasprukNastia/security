using Lab5_6.DAL;
using Lab5_6.Models;
using System;

namespace Lab5_6.Business
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public bool CreateUser(UserViewModel registerUserViewModel)
        {
            User user = new User();
            
            return _userRepository.StoreUser(user);
        }

        public bool LoginUser(UserViewModel registerUserViewModel)
        {
            return true;
        }
    }
}
