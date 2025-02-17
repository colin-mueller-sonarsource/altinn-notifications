﻿using Altinn.Notifications.Functions.Configurations;
using Altinn.Notifications.Functions.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Altinn.Notifications.Functions.Integrations
{
    public class NotificationsClient : INotifications
    {
        private readonly HttpClient _client;
        private readonly IToken _token;
        private readonly ILogger<INotifications> _logger;

        public NotificationsClient(
            HttpClient client,
             IOptions<PlatformSettings> settings,
            ILogger<INotifications> logger)
        {
            _logger = logger;
            client.BaseAddress = new Uri(settings.Value.ApiNotificationsEndpoint);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client = client;
        }

        public async Task<List<string>> GetOutboundEmails()
        {

            string path = "outbound/email";
            string token = string.Empty; // await _token.GeneratePlatformToken();
            HttpResponseMessage res = await _client.GetAsync(path, token);

            List<string> outboundEmails = new();

            if (!res.IsSuccessStatusCode)
            {
                return outboundEmails;
            }

            var responseString = await res.Content.ReadAsStringAsync();
            outboundEmails.AddRange(JsonSerializer.Deserialize<List<string>>(responseString));

            _logger.LogInformation($" // NotificationsClient // GetOutboundEmails // {outboundEmails.Count} emails pending");

            return outboundEmails;
        }

        public async Task<List<string>> GetOutboundSMS()
        {
            string path = "outbound/sms";
            string token = string.Empty; // await _token.GeneratePlatformToken();
            HttpResponseMessage res = await _client.GetAsync(path, token);
            List<string> outboundSMS = new();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Something went terribly wrong!");
                return outboundSMS;
            }

            var responseString = await res.Content.ReadAsStringAsync();
            outboundSMS.AddRange(JsonSerializer.Deserialize<List<string>>(responseString));
            return outboundSMS;
        }


        public async Task TriggerSendTarget(string targetId)
        {
            _logger.LogInformation($" // NotificationsClient // TriggerSendTarget // Posting target id {targetId}");
            string path = "send/";
            string token = string.Empty; // await _token.GeneratePlatformToken();
            HttpResponseMessage res = await _client.PostAsync(path, JsonContent.Create(targetId), token);
            _logger.LogInformation($" // NotificationsClient // TriggerSendTarget //  status code: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError($" // NotificationsClient // TriggerSendTarget // Failed with status code: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");
            }
        }
    }
}
