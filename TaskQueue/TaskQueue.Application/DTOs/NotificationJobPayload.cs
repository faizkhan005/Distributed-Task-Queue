namespace TaskQueue.Application.DTOs;

public record NotificationJobPayload
(
    string RecipientEmail,
    string RecipientName,
    string Subject,
    string Body,
    string Channel = "email"  // email , sms, push, etc.
);
