using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _apiKey;
    private readonly string _secretKey;
    private readonly IConfiguration _config;
    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _config = configuration;
        _logger = logger;
        _apiKey = configuration["MailjetConfig:MailjetApiKey"];
        _secretKey = configuration["MailjetConfig:MailjetSecretKey"];
    }

    public async Task<bool> SendEmail(EmailModel emailModel)
    {
        try
        {
            var dbSettings = _config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()!;

            MailjetClient client = new MailjetClient(_apiKey, _secretKey);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource
            };

            var confirmLink = $"dbSettings.Token.Issuer/api/authenticate/confirm-email?email={emailModel.ToEmail}";
            var emailModelBody = @"
                Hello.

                Please confirm your email address by clicking the link below

                " + confirmLink + @"

                Thank You !
                Regards
                ";


            var email = new TransactionalEmailBuilder()
                   .WithFrom(new SendContact(emailModel.From))
                   .WithSubject(emailModel.Subject)
                   .WithHtmlPart(emailModelBody)
                   .WithTo(new SendContact(emailModel.ToEmail))
                   .Build();

            var response = await client.SendTransactionalEmailAsync(email);

            var message = response.Messages[0];

            bool result = message.Status.ToLower() == "success";

            _logger.LogInformation("Email sent successfully.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email.");

            return false;
        }
    }

    public async Task<string> FormatEmailTemplate(string fromEmail, string subject, string toEmail, string message)
    {
        string filePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"Files/email.html"));
        string template = File.ReadAllText($"{filePath}");

        template = template.Replace("{fromEmail}", fromEmail);
        template = template.Replace("{subject}", subject);
        template = template.Replace("{message}", message);

        return template;
    }
}

