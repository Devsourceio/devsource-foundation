using DevSource.Foundation.Primitives;

namespace DevSource.Foundation.Tests;

public sealed class PrimitivesResultTests
{
    [Fact]
    public void Result_Success_CreatesSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Result_Failure_WithSingleError_CreatesFailedResult()
    {
        // Arrange
        var error = new Error("failure", "message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Contains(error, result.Errors);
    }

    [Fact]
    public void Result_Failure_NormalizesDistinctErrors()
    {
        // Arrange
        var duplicated = new Error("duplicate", "message");
        var unique = new Error("unique", "message");

        // Act
        var resultFromParams = Result.Failure(duplicated, duplicated, unique);
        var resultFromEnumerable = Result.Failure(new[] { duplicated, duplicated, unique });

        // Assert
        Assert.Equal(2, resultFromParams.Errors.Count);
        Assert.Equal(2, resultFromEnumerable.Errors.Count);
    }

    [Fact]
    public void Result_Failure_ThrowsForInvalidArguments()
    {
        // Arrange
        var nullErrorAction = () =>
        {
            _ = Result.Failure((Error)null!);
        };
        var nullArrayAction = () =>
        {
            _ = Result.Failure((Error[])null!);
        };
        var nullEnumerableAction = () =>
        {
            _ = Result.Failure((IEnumerable<Error>)null!);
        };
        var emptyErrorsAction = () =>
        {
            _ = Result.Failure(Array.Empty<Error>());
        };

        // Act / Assert
        Assert.Throws<ArgumentNullException>(nullErrorAction);
        Assert.Throws<ArgumentNullException>(nullArrayAction);
        Assert.Throws<ArgumentNullException>(nullEnumerableAction);
        Assert.Throws<ArgumentException>(emptyErrorsAction);
    }

    [Fact]
    public void Result_Constructor_ThrowsForInvalidStateCombinations()
    {
        // Arrange
        var successWithErrorsAction = () =>
        {
            _ = new TestResult(true, [new Error("code", "message")]);
        };
        var failureWithoutErrorsAction = () =>
        {
            _ = new TestResult(false, []);
        };

        // Act / Assert
        Assert.Throws<InvalidOperationException>(successWithErrorsAction);
        Assert.Throws<InvalidOperationException>(failureWithoutErrorsAction);
    }

    [Fact]
    public void ResultOfT_Success_ExposesValue()
    {
        // Act
        var result = Result<string>.Success("foundation");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("foundation", result.Value);
    }

    [Fact]
    public void ResultOfT_Failure_ExposesErrorsAndThrowsOnValueAccess()
    {
        // Arrange
        var error = new Error("failure", "message");

        // Act
        var failed = Result<string>.Failure(error);

        // Assert
        Assert.False(failed.IsSuccess);
        Assert.Single(failed.Errors);
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = failed.Value;
        });
    }

    [Fact]
    public void ResultOfT_Failure_NormalizesDistinctErrors()
    {
        // Arrange
        var duplicated = new Error("duplicate", "message");
        var unique = new Error("unique", "message");

        // Act
        var resultFromParams = Result<int>.Failure(duplicated, duplicated, unique);
        var resultFromEnumerable = Result<int>.Failure(new[] { duplicated, duplicated, unique });

        // Assert
        Assert.Equal(2, resultFromParams.Errors.Count);
        Assert.Equal(2, resultFromEnumerable.Errors.Count);
    }

    [Fact]
    public void ResultOfT_Failure_ThrowsForInvalidArguments()
    {
        // Arrange
        var nullErrorAction = () =>
        {
            _ = Result<string>.Failure((Error)null!);
        };
        var nullArrayAction = () =>
        {
            _ = Result<string>.Failure((Error[])null!);
        };
        var nullEnumerableAction = () =>
        {
            _ = Result<string>.Failure((IEnumerable<Error>)null!);
        };
        var emptyErrorsAction = () =>
        {
            _ = Result<string>.Failure(Array.Empty<Error>());
        };

        // Act / Assert
        Assert.Throws<ArgumentNullException>(nullErrorAction);
        Assert.Throws<ArgumentNullException>(nullArrayAction);
        Assert.Throws<ArgumentNullException>(nullEnumerableAction);
        Assert.Throws<ArgumentException>(emptyErrorsAction);
    }

    private sealed class TestResult : Result
    {
        public TestResult(bool isSuccess, IReadOnlyCollection<Error> errors) : base(isSuccess, errors)
        {
        }
    }
}
