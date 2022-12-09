﻿using CMS.Data.Context;
using CMS.Data.Repository;
using CMS.Model.Consts;
using CMS.Model.Entity;
using CMS.Model.Enum;
using CMS.Model.Model;
using CMS.Service.Exceptions;
using CMS.Service.Helper;
using CMS.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CMS.Service
{
    public interface IUserService
    {
        IQueryable<UserModel> GetAll();
        Task<UserModel> GetById(int id);
        Task<LoginResponseModel> Authenticate(LoginModel model);
        Task<User> GetTokenInfo(int userId, string token);
        Task<ServiceResult> Post(UserModel model);
        Task<ServiceResult> Put(UserModel model);
        Task<ServiceResult> Delete(int id);
        Task<UserProfileModel> GetProfile();
        Task<ServiceResult> UpdateProfile(UserProfileModel model);
        Task<ServiceResult> Register(RegisterModel model);
        Task<ServiceResult> ForgotPassword(ForgotPasswordModel model);
        Task<ServiceResult> ChangePassword(ChangePasswordModel model);
        Task<ServiceResult> EmailVerified(string code);
        Task<ResetPasswordInfoModel> GetUserByCode(string code);
        Task<ServiceResult> ResetPassword(ResetPasswordModel model);
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWork<CMSContext> _unitOfWork;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IMemoryCache _memoryCache;

        public UserService(IUnitOfWork<CMSContext> unitOfWork,
            IHttpContextAccessor httpContext,
            IJwtHelper jwtHelper,
            IMailHelper mailHelper,
            IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContext;
            _jwtHelper = jwtHelper;
            _mailHelper = mailHelper;
            _memoryCache = memoryCache;
        }

        public IQueryable<UserModel> GetAll()
        {
            return _unitOfWork.Repository<User>()
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.Id)
                .Select(x => new UserModel
                {
                    UserType = (int)x.UserType,
                    UserTypeName = EnumHelper.GetDescription<UserType>(x.UserType),
                    EmailAddress = x.EmailAddress,
                    Id = x.Id,
                    Name = x.Name,
                    Surname = x.Surname,
                    IsActive = x.IsActive,
                    Phone = x.Phone,
                    Status = EnumHelper.GetDescription<UserStatus>(x.Status),
                });
        }

        public Task<UserModel> GetById(int id)
        {
            return _unitOfWork.Repository<User>()
                .Where(x => x.Id == id && !x.Deleted)
                .Select(x => new UserModel
                {
                    EmailAddress = x.EmailAddress,
                    Phone = x.Phone,
                    Id = x.Id,
                    IsActive = x.IsActive,
                    Name = x.Name,
                    Status = EnumHelper.GetDescription<UserStatus>(x.Status),
                    Surname = x.Surname,
                    UserType = (int)x.UserType,
                    Token = x.Token,
                    TokenExpireDate = x.TokenExpireDate,
                    UserTypeName = EnumHelper.GetDescription<UserType>(x.UserType)
                }).FirstOrDefaultAsync();
        }

        public async Task<LoginResponseModel> Authenticate(LoginModel model)
        {
            var hassPassword = Security.MD5Crypt(model.Password);

            var user = await _unitOfWork.Repository<User>()
                .Where(x => x.EmailAddress == model.EmailAddress && x.Password == hassPassword && !x.Deleted)
                .Include(x => x.UserAccessRights)
                .ThenInclude(x => x.AccessRight)
                .ThenInclude(x => x.AccessRightEndpoints).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException("Email adresi veya şifre hatalıdır.");
            }

            if (!user.IsActive)
            {
                throw new BadRequestException("Hesabınız aktif değildir.");
            }

            if (user.Status == UserStatus.EmailNotVerified)
            {
                throw new BadRequestException("Email adresiniz doğrulanmamış.");
            }

            if (user.PasswordExpireDate < DateTime.Now)
            {
                await SendMailResetPassword(user);
                throw new ForbiddenException("Şifre geçerlilik süresi dolmuş. Mail adresinize şifre belirleme linki gönderildi.");
            }

            var jwtResult = _jwtHelper.GenerateJwtToken(user);

            user.Token = jwtResult?.Token;
            user.TokenExpireDate = DateTime.Now.AddHours(2);
            await _unitOfWork.Save();

            var result = new LoginResponseModel
            {
                UserType = user.UserType,
                ExpireDate = jwtResult?.ExpireDate,
                FullName = user.FullName,
                Token = jwtResult?.Token
            };

            return result;
        }

        public Task<User> GetTokenInfo(int userId, string token)
        {
            var user = _unitOfWork.Repository<User>()
                .Where(x => !x.Deleted && x.IsActive && x.Id == userId)
                .Include(x => x.UserAccessRights)
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<ServiceResult> Post(UserModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var checkEmail = await _unitOfWork.Repository<User>()
                .Any(x => x.Id != model.Id && x.EmailAddress == model.EmailAddress && !x.Deleted);

            if (checkEmail)
            {
                throw new FoundException("Email adresi ile daha önce kullanıcı kaydedilmiş.");
            }

            var user = new User
            {
                Deleted = false,
                EmailAddress = model.EmailAddress,
                IsActive = model.IsActive,
                Name = model.Name,
                Surname = model.Surname,
                UserType = (UserType)model.UserType,
                InsertedDate = DateTime.Now,
                HashCode = Security.RandomBase64(),
                Status = UserStatus.NotSetPassword,
                Phone = model.Phone
            };

            await _unitOfWork.Repository<User>().Add(user);

            await _unitOfWork.Save();

            // kullanıcıya email gönderiliyor                
            result = await SendMailResetPassword(user);

            result.Message = AlertMessages.Post;

            return result;
        }

        public async Task<ServiceResult> Put(UserModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var loginUser = _httpContext.HttpContext.User.Parse();

            var isExistUser = await _unitOfWork.Repository<User>()
                .Any(x => x.EmailAddress == model.EmailAddress && !x.Deleted && x.Id != model.Id);

            if (isExistUser)
            {
                throw new FoundException("Email adresi ile daha önce kullanıcı kaydedilmiş.");
            }

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.Id == model.Id && !x.Deleted);

            if (user == null)
            {
                throw new NotFoundException(AlertMessages.NotFound);
            }

            user.EmailAddress = model.EmailAddress;
            user.IsActive = model.IsActive;
            user.Name = model.Name;
            user.Surname = model.Surname;
            user.UserType = (UserType)model.UserType;
            user.Phone = model.Phone;

            await _unitOfWork.Save();

            if (user.UserType != UserType.Member)
            {
                _memoryCache.Remove($"userMenu_{model.Id}");
            }

            result.Message = AlertMessages.Put;

            return result;
        }

        public async Task<ServiceResult> Delete(int id)
        {
            var result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.Id == id && !x.Deleted);

            if (user == null)
            {
                throw new NotFoundException(AlertMessages.NotFound);
            }

            user.Deleted = true;

            await _unitOfWork.Save();

            if (user.UserType != UserType.Member)
            {
                _memoryCache.Remove($"userMenu_{id}");
            }

            result.Message = AlertMessages.Delete;

            return result;
        }

        public async Task<UserProfileModel> GetProfile()
        {
            UserProfileModel model = null;

            var loginUser = _httpContext.HttpContext.User.Parse();

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.Id == loginUser.UserId && !x.Deleted);

            if (user == null)
            {
                throw new NotFoundException(AlertMessages.NotFound);
            }

            model = new UserProfileModel()
            {
                EmailAddress = user.EmailAddress,
                Name = user.Name,
                Surname = user.Surname,
                Phone = user.Phone
            };

            return model;
        }

        public async Task<ServiceResult> UpdateProfile(UserProfileModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var loginUser = _httpContext.HttpContext.User.Parse();

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.Id == loginUser.UserId && !x.Deleted);

            if (user == null)
            {
                throw new NotFoundException(AlertMessages.NotFound);
            }

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Phone = model.Phone;
            user.UpdatedDate = DateTime.Now;

            await _unitOfWork.Save();

            result.Message = AlertMessages.Put;

            return result;
        }

        public async Task<ServiceResult> Register(RegisterModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            if (model.Password != model.RePassword)
            {
                throw new BadRequestException("Şifre alanları uyuşmamaktadır.");
            }

            var isExistEmail = await _unitOfWork.Repository<User>()
                .Any(x => x.EmailAddress == model.EmailAddress && !x.Deleted);

            if (isExistEmail)
            {
                throw new FoundException($"{model.EmailAddress} mail adresiyle üye mevcuttur.");
            }

            var user = new User()
            {
                Deleted = false,
                EmailAddress = model.EmailAddress,
                HashCode = Security.RandomBase64(),
                Name = model.Name,
                Surname = model.Surname,
                InsertedDate = DateTime.Now,
                IsActive = true,
                Password = Security.MD5Crypt(model.Password),
                Phone = model.Phone,
                PasswordExpireDate = DateTime.Now.AddMonths(3),
                UserType = UserType.Member,
                Status = UserStatus.EmailNotVerified
            };

            await _unitOfWork.Repository<User>().Add(user);

            //kullanıcıya email gönderiliyor
            var emailVerifyTemplateModel = new TemplateModel
            {
                FullName = user.FullName,
                Url = $"{Global.WebUrl}email-verify/{user.HashCode}"
            };

            result = await _mailHelper.SendWithTemplate(new MailWithTemplateModel()
            {
                EmailAddress = model.EmailAddress,
                TemplateType = TemplateType.EmailVerificationLink,
                Data = emailVerifyTemplateModel
            });

            result.Message = "Email adresinize onay maili gönderilmiştir.";

            await _unitOfWork.Save();

            return result;
        }

        public async Task<ServiceResult> ForgotPassword(ForgotPasswordModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.EmailAddress == model.EmailAddress && !x.Deleted);

            if (user != null)
            {
                user.HashCode = Security.RandomBase64();
                user.Status = UserStatus.NotSetPassword;
                user.UpdatedDate = DateTime.Now;

                await _unitOfWork.Save();

                // kullanıcıya mail gönderiliyor
                result = await SendMailResetPassword(user);
            }

            result.Message = "Email adresiniz mevcutsa şifre belirleme linki gönderilmiştir.";

            return result;
        }

        public async Task<ServiceResult> ChangePassword(ChangePasswordModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            if (model.NewPassword != model.ReNewPassword)
            {
                throw new BadRequestException("Şifre alanları uyuşmamaktadır.");
            }

            var loginUser = _httpContext.HttpContext.User.Parse();

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.Id == loginUser.UserId && !x.Deleted);

            if (user == null)
            {
                throw new NotFoundException(AlertMessages.NotFound);
            }
            var oldPassword = Security.MD5Crypt(model.OldPassword);

            if (user.Password != oldPassword)
            {
                throw new BadRequestException("Mevcut şifreniz hatalıdır.");
            }

            user.Password = Security.MD5Crypt(model.NewPassword);
            user.PasswordExpireDate = DateTime.Now.AddMonths(3);
            user.UpdatedDate = DateTime.Now;
            user.HashCode = String.Empty;

            await _unitOfWork.Save();

            result.Message = AlertMessages.Put;

            return result;
        }

        public async Task<ServiceResult> EmailVerified(string code)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.HashCode == code && x.Status == UserStatus.EmailNotVerified);

            if (user == null)
            {
                result.StatusCode = HttpStatusCode.NotFound;
                return result;
            }

            user.HashCode = String.Empty;
            user.Status = UserStatus.Active;

            await _unitOfWork.Save();

            return result;
        }

        public async Task<ServiceResult> ResetPassword(ResetPasswordModel model)
        {
            ServiceResult result = new ServiceResult { StatusCode = HttpStatusCode.OK };

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.HashCode == model.Code && x.Status == UserStatus.NotSetPassword);

            if (user == null)
            {
                throw new BadRequestException(AlertMessages.UserNotFound);
            }

            if (model.NewPassword != model.ReNewPassword)
            {
                throw new BadRequestException("Şifre alanları uyuşmamaktadır.");
            }

            user.Password = Security.MD5Crypt(model.NewPassword);
            user.PasswordExpireDate = DateTime.Now.AddMonths(3);
            user.UpdatedDate = DateTime.Now;
            user.HashCode = String.Empty;
            user.Status = UserStatus.Active;

            await _unitOfWork.Save();

            result.Message = AlertMessages.SuccessResetPassword;

            return result;
        }

        private async Task<ServiceResult> SendMailResetPassword(User user)
        {
            var forgotPasswordTemplateModel = new TemplateModel
            {
                FullName = user.FullName,
                Url = $"{Global.WebUrl}set-password/{user.HashCode}"
            };

            var result = await _mailHelper.SendWithTemplate(new MailWithTemplateModel()
            {
                EmailAddress = user.EmailAddress,
                TemplateType = TemplateType.SetPasswordLink,
                Data = forgotPasswordTemplateModel
            });
            return result;
        }

        public async Task<ResetPasswordInfoModel> GetUserByCode(string code)
        {
            ResetPasswordInfoModel model = null;

            var user = await _unitOfWork.Repository<User>()
                .FirstOrDefault(x => x.HashCode == code && x.Status == UserStatus.NotSetPassword);

            if (user != null)
            {
                model = new ResetPasswordInfoModel
                {
                    FullName = $"{user.Name} {user.Surname}",
                    Code = code
                };
            }
            return model;
        }
    }
}
