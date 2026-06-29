using Domain.Enums;
using Domain.Podcasts;
using Domain.Podcasts.Events;
using FluentAssertions;

public class PodcastTests
{
    private static Podcast CreatePodcast(
        Guid? userId = null,
        Guid? documentId = null,
        PodcastMode mode = PodcastMode.Full,
        PodcastStatus status = PodcastStatus.Pending)
    {
        return new Podcast(
            userId ?? Guid.NewGuid(),
            documentId ?? Guid.NewGuid(),
            mode,
            topic: null,
            startPage: null,
            endPage: null,
            status);
    }

    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var userId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        var podcast = new Podcast(
            userId, documentId,
            PodcastMode.Full,
            topic: null, startPage: null, endPage: null,
            PodcastStatus.Pending);

        podcast.UserId.Should().Be(userId);
        podcast.DocumentId.Should().Be(documentId);
        podcast.Mode.Should().Be(PodcastMode.Full);
        podcast.Status.Should().Be(PodcastStatus.Pending);
        podcast.ScriptPath.Should().BeNull();
        podcast.AudioPath.Should().BeNull();
        podcast.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void SetPaths_Should_Set_Script_And_Audio_Paths()
    {
        var podcast = CreatePodcast();
        podcast.SetPaths("script.txt", "audio.mp3");
        podcast.ScriptPath.Should().Be("script.txt");
        podcast.AudioPath.Should().Be("audio.mp3");
    }

    [Theory]
    [InlineData("", "audio.mp3")]
    [InlineData("   ", "audio.mp3")]
    [InlineData(null, "audio.mp3")]
    public void SetPaths_Should_Throw_When_ScriptPath_Is_Invalid(string? scriptPath, string audioPath)
    {

        var podcast = CreatePodcast();
        Action act = () => podcast.SetPaths(scriptPath!, audioPath);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("scriptPath");
    }

    [Theory]
    [InlineData("script.txt", "")]
    [InlineData("script.txt", "   ")]
    [InlineData("script.txt", null)]
    public void SetPaths_Should_Throw_When_AudioPath_Is_Invalid(string scriptPath, string? audioPath)
    {

        var podcast = CreatePodcast();
        Action act = () => podcast.SetPaths(scriptPath, audioPath!);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("audioPath");
    }

    [Fact]
    public void UpdateStatus_Should_Change_Status()
    {
        var podcast = CreatePodcast();
        podcast.UpdateStatus(PodcastStatus.Processing);
        podcast.Status.Should().Be(PodcastStatus.Processing);
    }

    [Fact]
    public void UpdateStatus_Completed_Should_Clear_ErrorMessage()
    {

        var podcast = CreatePodcast();
        podcast.UpdateStatus(PodcastStatus.Failed, "some error");
        podcast.UpdateStatus(PodcastStatus.Completed);
        podcast.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_Failed_Should_Set_ErrorMessage()
    {
        var podcast = CreatePodcast();
        podcast.UpdateStatus(PodcastStatus.Failed, "Generation failed");

        podcast.Status.Should().Be(PodcastStatus.Failed);
        podcast.ErrorMessage.Should().Be("Generation failed");
    }

    [Fact]
    public void UpdateStatus_Should_Raise_PodcastStatusChangedDomainEvent()
    {
        var podcast = CreatePodcast();

        podcast.UpdateStatus(PodcastStatus.Completed);

        podcast.DomainEvents.Should().ContainSingle()
               .Which.Should().BeOfType<PodcastStatusChangedDomainEvent>();
    }
}