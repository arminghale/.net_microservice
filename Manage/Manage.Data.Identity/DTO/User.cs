using Manage.Data.Public;
using System.Diagnostics.CodeAnalysis;

namespace Manage.Data.Identity.DTO.User
{
    public record UserList
    {
        [SetsRequiredMembers]
        public UserList(Models.User user)
        {
            Id = user.Id;
            Phonenumber = user.Phonenumber;
            Email = user.Email;
            Username = user.Username;
            Unavailable = user.Delete;

            CreateDateShamsi = DateDifference.MiladiToShamsi(user.CreateDate);

        }

        public int Id { get; init; }
        public string? Phonenumber { get; init; }
        public string? Email { get; init; }
        public required string Username { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }

    }
    public record UserOneItself
    {
        public UserOneItself(Models.User user)
        {
            Id = user.Id;
            Phonenumber = user.Phonenumber;
            Username = user.Username;
            Email = user.Email;
            EmailValidation = user.EmailValidation;
            PhonenumberValidation = user.PhonenumberValidation;
            Validation = user.Validation;

            CreateDateShamsi = DateDifference.MiladiToShamsi(user.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(user.LastUpdateDate);
            LastLoginDateShamsi = DateDifference.MiladiToShamsiWithHour(user.LastLoginDate);

        }

        public int Id { get; init; }
        public string? Phonenumber { get; init; }
        public string Username { get; init; }
        public string? Email { get; init; }
        public string EmailValidation { get; init; }
        public string PhonenumberValidation { get; init; }
        public string Validation { get; init; }
        public string CreateDateShamsi { get; init; }
        public string LastUpdateDateShamsi { get; init; }
        public string LastLoginDateShamsi { get; init; }
    }
    public record UserOneAdmin
    {
        public UserOneAdmin(Models.User user)
        {
            Id = user.Id;
            Roles = user.UserRoles.Select(w => new { 
                Role = w.Role.Title,
                RoleId = w.RoleId,
                Tenant = w.Tenant.Title,
                TenantId = w.TenantId,
            }).ToArray();
            Phonenumber = user.Phonenumber;
            Username = user.Username;
            Email = user.Email;
            EmailValidation = user.EmailValidation;
            PhonenumberValidation = user.PhonenumberValidation;
            Validation = user.Validation;
            Unavailable = user.Delete;

            CreateDateShamsi = DateDifference.MiladiToShamsi(user.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(user.LastUpdateDate);
            LastLoginDateShamsi = DateDifference.MiladiToShamsiWithHour(user.LastLoginDate);

        }
        public int Id { get; init; }
        public object[] Roles { get; set; }
        public string? Phonenumber { get; init; }
        public string Username { get; init; }
        public string? Email { get; init; }
        public string EmailValidation { get; init; }
        public string PhonenumberValidation { get; init; }
        public string Validation { get; init; }
        public bool Unavailable { get; init; } = false;
        public string CreateDateShamsi { get; init; }
        public string LastUpdateDateShamsi { get; init; }
        public string LastLoginDateShamsi { get; init; }
    }

}
