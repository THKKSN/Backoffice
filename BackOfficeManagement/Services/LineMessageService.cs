using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BackOfficeManagement.Data;
using BackOfficeManagement.Interface;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeManagement.Services
{
    public class LineMessageService : ILineMessageRepository
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;

        // private readonly string UrlPushMessage;
        // private readonly string LineGroupId;
        // private readonly string ChannelAccessToken;

        public LineMessageService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ApplicationDbContext context
            )
        {
            _clientFactory = clientFactory;
            _context = context;

            // UrlPushMessage = configuration["Thailuxtrip:LineOA:UrlPushMessage"] ?? string.Empty;
            // LineGroupId = configuration["Thailuxtrip:LineOA:GroupId"] ?? string.Empty;
            // ChannelAccessToken = configuration["Thailuxtrip:LineOA:ChannelAccessToken"] ?? string.Empty;
        }

        public async Task<bool> SendLINEMessage()
        {
            try
            {
                using (var client = new HttpClient())
                {

                    //เอาไปทำต่อ
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> SendMessageToGroup(string groupId, string message)
        {
            try
            {
                var lineConfig = await _context.LineConfig
                    .Where(x => x.line_group_id == groupId).FirstOrDefaultAsync();

                if (lineConfig == null)
                    throw new Exception("ไม่พบ Line Group ที่ต้องการส่ง");

                using var client = _clientFactory.CreateClient();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    lineConfig.url_push_message
                );

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", lineConfig.channel_access_token);

                var payload = new
                {
                    to = lineConfig.line_group_id,
                    messages = new[]
                    {
                        new
                        {
                            type = "text",
                            text = message
                        }
                    }
                };

                request.Content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LINE error: {ex.Message}");
                return false;
            }
        }


    }
}
