using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WeightLossTracker.Data;
using Xunit;

namespace WeightLossTracker.Tests;

public class UtcDateTimeConverterTests
{
    private readonly Func<DateTime, DateTime> _convertFromProvider;

    public UtcDateTimeConverterTests()
    {
        var converter = new UtcDateTimeConverter();
        _convertFromProvider = (Func<DateTime, DateTime>)
            converter.ConvertFromProviderExpression.Compile();
    }

    [Fact]
    public void ConvertFromProvider_SetsKindToUtc()
    {
        var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Unspecified);
        var result = _convertFromProvider(input);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ConvertFromProvider_PreservesDateAndTimeValue()
    {
        var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Unspecified);
        var result = _convertFromProvider(input);
        Assert.Equal(input.Ticks, result.Ticks);
    }

    [Fact]
    public void ConvertFromProvider_AlreadyUtc_RemainsUtc()
    {
        var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Utc);
        var result = _convertFromProvider(input);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ConvertToProvider_PassesThroughUnchanged()
    {
        var converter = new UtcDateTimeConverter();
        var convertToProvider = (Func<DateTime, DateTime>)
            converter.ConvertToProviderExpression.Compile();
        var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Utc);
        var result = convertToProvider(input);
        Assert.Equal(input, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ConvertToProvider_LocalKind_PassesThroughUnchanged()
    {
        var converter = new UtcDateTimeConverter();
        var convertToProvider = (Func<DateTime, DateTime>)
            converter.ConvertToProviderExpression.Compile();
        var input = new DateTime(2026, 4, 25, 19, 0, 0, DateTimeKind.Local);
        var result = convertToProvider(input);
        Assert.Equal(input.Ticks, result.Ticks);
    }
}
