using Domain.Documents;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests.Entities;

public class DocumentTests
{
    private static Document CreateDocument(
        Guid? userId = null,
        string fileName = "file.pdf",
        string filePath = "/files/file.pdf",
        string fileHash = "abc123hash",
        int pageCount = 10)
    {
        return new Document(
            userId ?? Guid.NewGuid(),
            fileName,
            filePath,
            fileHash,
            pageCount);
    }

    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var userId = Guid.NewGuid();
        var fileName = "file.pdf";
        var filePath = "/files/file.pdf";
        var fileHash = "abc123hash";
        var pageCount = 10;

        var document = new Document(userId, fileName, filePath, fileHash, pageCount);
        document.UserId.Should().Be(userId);
        document.FileName.Should().Be(fileName);
        document.FilePath.Should().Be(filePath);
        document.FileHash.Should().Be(fileHash);
        document.PageCount.Should().Be(pageCount);
        document.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        document.Podcasts.Should().BeEmpty();
    }
    [Fact]
    public void AddPodcast_Should_Add_New_Podcast()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);
        var podcast = document.AddPodcast(
            userId,
            PodcastMode.Full,
            topic: null,
            startPage: null,
            endPage: null,
            PodcastStatus.Pending);
        document.Podcasts.Should().HaveCount(1);
        document.Podcasts.Should().Contain(podcast);
    }

    [Fact]
    public void AddPodcast_Should_Return_Podcast_With_Correct_Properties()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);

        var podcast = document.AddPodcast(
            userId,
            PodcastMode.Query,
            topic: "AI",
            startPage: 1,
            endPage: 5,
            PodcastStatus.Pending);

        podcast.UserId.Should().Be(userId);
        podcast.DocumentId.Should().Be(document.Id);
        podcast.Mode.Should().Be(PodcastMode.Query);
        podcast.Topic.Should().Be("AI");
        podcast.StartPage.Should().Be(1);
        podcast.EndPage.Should().Be(5);
        podcast.Status.Should().Be(PodcastStatus.Pending);
    }

    [Fact]
    public void AddPodcast_Should_Allow_Different_Modes()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);
        document.AddPodcast(userId, PodcastMode.Full, null, null, null, PodcastStatus.Pending);
        document.AddPodcast(userId, PodcastMode.Query, "AI", 1, 5, PodcastStatus.Pending);
        document.Podcasts.Should().HaveCount(2);
    }
}