using Grpc.Net.Client;
using Manage.Data.Public;
using Manage.Data.Reminder.Repository;
using Microsoft.EntityFrameworkCore;

namespace Manage.Reminder.Background
{
    public class Remind : BackgroundService
    {
        private readonly ILogger<Remind> _logger;
        private readonly IServiceProvider _service;
        private readonly string _gRPCUser;

        public Remind(ILogger<Remind> logger, IServiceProvider service, string gRPCUser)
        {
            _logger = logger;
            _service = service;
            _gRPCUser = gRPCUser;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting remind background service");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckReminders();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        private async Task CheckReminders()
        {
            using (var scope = _service.CreateScope())
            {
                IReminder _reminder = scope.ServiceProvider.GetRequiredService<IReminder>();
                ISendMail _email = scope.ServiceProvider.GetRequiredService<ISendMail>();
                var reminders = await _reminder.GetAll().Where(w => (w.Date - DateTime.Now).TotalSeconds <= 60).ToListAsync();
                var users = await GetUsersEmail(reminders.Select(w => w.UserSubscription.UserId).Distinct());
                foreach (var reminder in reminders)
                {
                    if (users.ContainsKey(reminder.UserSubscription.UserId))
                    {
                        _email.Send(new string[] { users[reminder.UserSubscription.UserId] }, reminder.Title, reminder.Description);
                        _logger.LogInformation($"Send reminder ({reminder.Title}) to ({users[reminder.UserSubscription.UserId]})");
                    }
                }

            }
        }
        private async Task<Dictionary<int,string>> GetUsersEmail(IEnumerable<int> userids)
        {
            var emails = new Dictionary<int, string>();
            foreach (var id in userids)
            {
                using var channel = GrpcChannel.ForAddress(_gRPCUser);
                var client = new Users.UsersClient(channel);
                var reply = await client.SendUserAsync(
                    new UserRequest { Id = 1 });
                if (string.IsNullOrEmpty(reply.Email))
                {
                    emails[id] = reply.Email;
                }
            }
            return emails;
        }
    }
}
