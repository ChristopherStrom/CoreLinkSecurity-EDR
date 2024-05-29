using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;

public class ContactModel : PageModel
{
    private readonly IConfiguration _configuration;

    public ContactModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    [Required]
    public string Name { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [BindProperty]
    [Required]
    public string Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Get email settings from configuration
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"]);
        var senderName = emailSettings["SenderName"];
        var senderEmail = emailSettings["SenderEmail"];
        var senderPassword = emailSettings["SenderPassword"];

        // Construct the email message
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
        emailMessage.To.Add(new MailboxAddress("CoreLink Security", senderEmail));
        emailMessage.Subject = "Contact Us Form Submission";
        emailMessage.Body = new TextPart("plain")
        {
            Text = $"Name: {Name}\nEmail: {Email}\nMessage: {Message}"
        };

        // Send the email
        using (var client = new MailKit.Net.Smtp.SmtpClient())  // Specify the full namespace
        {
            await client.ConnectAsync(smtpServer, smtpPort, false);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        // Show a confirmation message or redirect to a thank-you page
        TempData["SuccessMessage"] = "Your message has been sent successfully!";
        return RedirectToPage("Contact");
    }
}
