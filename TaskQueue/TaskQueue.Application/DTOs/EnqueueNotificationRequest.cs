namespace TaskQueue.Application.DTOs;

public record EnqueueNotificationRequest
(
    string RecipientEmail,
    string RecipientName,
    string Subject,
    string Body,
    string Channel = "email"
);