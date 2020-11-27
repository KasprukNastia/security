﻿using Lab5_6.Models;
using System.Collections.Generic;

namespace Lab5_6.DAL
{
    public interface IUserRepository
    {
        bool StoreUser(User user);

        List<User> GetAllUsers();

        User GetUserByCreds(string email, string passwordHash);
    }
}