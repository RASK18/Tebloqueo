namespace Tebloqueo.Tests;

public sealed class BlockingStatusTests
{
    [Theory]
    [InlineData("", 1, 0)]
    [InlineData("1.1.1.1", 1, 1)]
    [InlineData("1.1.1.1\n2.2.2.2", 1, 2)]
    [InlineData("1.1.1.1\n2.2.2.2\n2001:db8::1", 1, 3)]
    public void FromContentCountsValidAddresses(string content, int expectedState, int expectedCount)
    {
        var result = BlockingStatus.FromContent(content);

        Assert.Equal((BlockingState)expectedState, result.State);
        Assert.Equal(expectedCount, result.IpCount);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData(10, 1)]
    [InlineData(11, 2)]
    public void FromContentChangesStateAboveThreshold(int count, int expectedState)
    {
        var content = string.Join('\n', Enumerable.Range(1, count).Select(index => $"192.0.2.{index}"));

        var result = BlockingStatus.FromContent(content);

        Assert.Equal((BlockingState)expectedState, result.State);
        Assert.Equal(count, result.IpCount);
    }

    [Fact]
    public void FromContentIgnoresBlankLines()
    {
        var result = BlockingStatus.FromContent("\r\n  \r\n1.1.1.1\r\n\r\n");

        Assert.Equal(BlockingState.No, result.State);
        Assert.Equal(1, result.IpCount);
    }

    [Fact]
    public void FromContentRejectsAnyInvalidNonEmptyLine()
    {
        var result = BlockingStatus.FromContent("1.1.1.1\nesto-no-es-una-ip");

        Assert.Equal(BlockingState.Unknown, result.State);
        Assert.Null(result.IpCount);
        Assert.NotNull(result.Error);
    }
}
