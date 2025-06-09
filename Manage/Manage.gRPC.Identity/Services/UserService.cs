using Grpc.Core;
using Manage.Data.Identity.Repository;

namespace Manage.gRPC.Identity.Services
{
    public class UserService : Users.UsersBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUser _user;
        public UserService(ILogger<UserService> logger, IUser user)
        {
            _logger = logger;
            _user = user;
        }

        public override async Task<UserResponse> SendUser(UserRequest request, ServerCallContext context)
        {
            var user = await _user.GetByID(request.Id);
            return await Task.FromResult(new UserResponse
            {
                Phonenumber = user.Phonenumber,
                Username = user.Username,
                Email = user.Email != null ? user.Email : "",
                Validation = user.Validation,
                LastLogin = user.LastLoginDate.ToShortDateString()
            });
        }
    }
}
