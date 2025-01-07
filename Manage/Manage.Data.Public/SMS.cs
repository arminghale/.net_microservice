using System.Text;
using System.Text.Json;

namespace Manage.Data.Public
{
    //https://raygansms.com

    //services.AddSingleton<ISMS>(provider => new SMS(clientUsername,clientPassword,clientPhonenumber));
    public interface ISMS
    {
        Task<int> SmsCode(string phonenumber, string footer = "");
        Task<bool> ValidateCode(string code, string phonenumber);
        void SmsSend(string message, string[] phonenumbers);
        Task<string> PostData(string inputJson);
    }
    public class SMS: ISMS
    {
        private readonly string clientUsername;
        private readonly string clientPassword;
        private readonly string clientPhonenumber;
        public SMS(string clientUsername, string clientPassword, string clientPhonenumber)
        {
            this.clientUsername = clientUsername;
            this.clientPassword = clientPassword;
            this.clientPhonenumber = clientPhonenumber;
        }
        public async Task<int> SmsCode(string phonenumber, string footer = "")
        {
            var client = new HttpClient();
            var inputJson = new { Username = clientUsername, Password = clientPassword, Mobile = phonenumber, Footer = footer };
            var Content = new StringContent(JsonSerializer.Serialize(inputJson), Encoding.UTF8, "application/json");
            //HttpResponseMessage response = await client.PostAsync("https://raygansms.com/AutoSendCode.ashx", Content);
            HttpResponseMessage response = await client.GetAsync($"https://raygansms.com/AutoSendCode.ashx?Username={clientUsername}&Password={clientPassword}&Mobile={phonenumber}&Footer={footer}");
            var result = await response.Content.ReadAsStringAsync();
            return int.Parse(result);
        }
        public async Task<bool> ValidateCode(string code, string phonenumber)
        {
            var client = new HttpClient();
            var inputJson = new { Username = clientUsername, Password = clientPassword, Mobile = phonenumber, Code = code };
            var Content = new StringContent(JsonSerializer.Serialize(inputJson), Encoding.UTF8, "application/json");
            //HttpResponseMessage response = await client.PostAsync("https://raygansms.com/CheckSendCode.ashx", Content);
            HttpResponseMessage response = await client.GetAsync($"https://raygansms.com/login/CheckSendCode.ashx?Username={clientUsername}&Password={clientPassword}&Mobile={phonenumber}&Code={code}");
            var result = await response.Content.ReadAsStringAsync();
            return bool.Parse(result);
        }

        public async void SmsSend(string message, string[] phonenumbers)
        {
            object input = new
            {
                PhoneNumber = clientPhonenumber,
                Message = message,
                UserGroupID = Guid.NewGuid(),
                Mobiles = phonenumbers,
                SendDateInTimeStamp = new SendSms().GetTimeStamp(DateTime.Now, DateTimeKind.Local)
            };

            string inputJson = JsonSerializer.Serialize(input);
            string res = await PostData(inputJson);
        }
        public async Task<string> PostData(string inputJson)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://smspanel.trez.ir/api/smsAPI/SendMessage/");
            //request.Headers.Add("Content-type", "application/json");
            string token = new SendSms().Base64Encode(clientUsername + ":" + clientPassword);
            request.Headers.Add("Authorization", string.Format("Basic {0}", token));
            request.Content = new StringContent(inputJson, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }


        private class SendSms
        {

            // انکد کردن به بیس 64
            public string Base64Encode(string plainText)
            {
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainTextBytes);
            }

            public long GetTimeStamp(DateTime dt, DateTimeKind dtk)
            {
                long unixTimestamp = (long)dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, dtk)).TotalSeconds;
                return unixTimestamp;
            }


        }

        private class SendMessageResult
        {
            public required string Code { get; set; }
            public required string Message { get; set; }
            public required string Result { get; set; }
        }
    }
}
