﻿using CMS.Model.Enum;
using System;
using System.Collections.Generic;

namespace CMS.Model.Model
{
    public class UserModel
    {
        public int Id { get; set; }
        public int UserType { get; set; }
        public string UserTypeName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public DateTime? TokenExpireDate { get; set; }
    }

    public class UserCreateOrUpdateModel { }


    public class UserProfileModel
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }

        public string Phone { get; set; }
    }

    public class RegisterModel
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string RePassword { get; set; }

        public string Phone { get; set; }
    }

    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string ReNewPassword { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string EmailAddress { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Code { get; set; }

        public string NewPassword { get; set; }

        public string ReNewPassword { get; set; }
    }

    public class ResetPasswordInfoModel
    {
        public string Code { get; set; }
        public string FullName { get; set; }
    }

    public class TokenResponseModel
    {
        public string Token { get; set; }

        public string FullName { get; set; }

        public bool IsAccessAdminPanel { get; set; }

        public List<string> OperationAccessRights { get; set; }

        public List<string> MenuAccessRights { get; set; }
    }

    public class AppUserModel
    {
        public int UserId { get; set; }

        public UserType UserType { get; set; }
    }
}
