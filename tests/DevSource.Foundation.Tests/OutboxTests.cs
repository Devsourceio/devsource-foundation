using System.Text.Json;
using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;

namespace DevSource.Foundation.Tests;

public sealed class OutboxTests
{
    [Fact]
    public void OutboxMessageFactory_CreatesSerializedMessageWithDiscriminator()
    {
        var occurredOnUtc = new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

        var message = OutboxMessageFactory.Create(
            new CustomerRegistered(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            occurredOnUtc);

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(typeof(CustomerRegistered).FullName, message.EventType);
        Assert.Equal(occurredOnUtc, message.OccurredOnUtc);
        Assert.Equal(
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            JsonDocument.Parse(message.Payload).RootElement.GetProperty("CustomerId").GetString());
        Assert.Null(message.ProcessedOnUtc);
        Assert.Equal(0, message.RetryCount);
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
    }

    [Fact]
    public void OutboxMessage_TracksFailureAndSuccessfulProcessing()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(),
            "CustomerRegistered",
            "{}",
            DateTimeOffset.UtcNow);

        message.MarkFailed("broker unavailable");
        Assert.Equal(1, message.RetryCount);
        Assert.Equal("broker unavailable", message.LastError);
        Assert.Equal(OutboxMessageStatus.Failed, message.Status);

        message.Retry();
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Null(message.LastError);

        var processedOnUtc = DateTimeOffset.UtcNow;
        message.MarkProcessed(processedOnUtc);

        Assert.Equal(processedOnUtc, message.ProcessedOnUtc);
        Assert.Equal(1, message.RetryCount);
        Assert.Null(message.LastError);
        Assert.Equal(OutboxMessageStatus.Published, message.Status);
    }

    [Fact]
    public void OutboxMessage_RejectsInvalidEnvelopeValues()
    {
        Assert.Throws<ArgumentException>(() => new OutboxMessage(
            Guid.Empty, "Event", "{}", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => new OutboxMessage(
            Guid.NewGuid(), " ", "{}", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => new OutboxMessage(
            Guid.NewGuid(), "Event", " ", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => new OutboxMessage(
            Guid.NewGuid(), "Event", "{}", DateTimeOffset.UtcNow).MarkFailed(" "));
    }

    private sealed record CustomerRegistered(Guid CustomerId);

    [Fact]
    public void OutboxMessage_RetryRejectsMessagesThatDidNotFail()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), "Event", "{}", DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => message.Retry());
    }
}
