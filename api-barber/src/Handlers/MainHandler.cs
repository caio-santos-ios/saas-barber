namespace api_barber.Services
{
    public class MailHandler(HttpClient http)
    {
        private readonly string apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        private readonly string fromEmail = Environment.GetEnvironmentVariable("RESEND_EMAIL") ?? "";

        public async Task<string> SendMail(string recipient, string subject, string body)
        {
            try
            {
                var payload = new
                {
                    from = fromEmail,
                    to = new[] { recipient },
                    subject,
                    html = body
                };

                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new("Bearer", apiKey);

                var res = await http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
