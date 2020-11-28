using Lab5_6.Models;

namespace Lab5_6.Business
{
    public interface IUserManager
    {
        bool CreateUser(UserViewModel registerUserViewModel);
        bool LoginUser(UserViewModel registerUserViewModel);
        bool StoreSensitiveData(SensitiveDataViewModel sensitiveDataViewModel);
        SensitiveDataViewModel GetSensitiveData(string userEmail);
    }
}
