using DevSource.Foundation.Primitives;

namespace DevSource.Foundation.Tests;

public sealed class PrimitivesErrorGuardMaybeTests
{
    [Fact]
    public void Error_ConstructsAndFormatsSuccessfully()
    {
        // Act
        var error = new Error("code", "message");

        // Assert
        Assert.Equal("code", error.Code);
        Assert.Equal("message", error.Message);
        Assert.Equal("code: message", error.ToString());
        Assert.Equal("none", Error.None.Code);
    }

    [Theory]
    [InlineData(null, "message", "code")]
    [InlineData(" ", "message", "code")]
    [InlineData("code", null, "message")]
    [InlineData("code", " ", "message")]
    public void Error_ThrowsForInvalidArguments(string? code, string? message, string invalidParam)
    {
        // Act
        var action = () =>
        {
            _ = new Error(code!, message!);
        };

        // Assert
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal(invalidParam, exception.ParamName);
    }

    [Fact]
    public void Error_Equality_UsesCodeOnly()
    {
        // Arrange
        var left = new Error("same", "first");
        var same = new Error("same", "second");
        var different = new Error("different", "first");

        // Act
        var equalsSelf = left!.Equals(left);
        var equalsTyped = left!.Equals(same);
        var equalsObject = left!.Equals((object)same);
        var equalsDifferentObject = left!.Equals(new object());
        Error? nullError = null;
        var equalsNull = left!.Equals(nullError);
        var equalsDifferent = left!.Equals(different);
        var equalOperator = left! == same!;
        var notEqualOperator = left! != different!;
        var leftHashCode = left!.GetHashCode();
        var sameHashCode = same!.GetHashCode();

        // Assert
        Assert.True(equalsSelf);
        Assert.True(equalsTyped);
        Assert.True(equalsObject);
        Assert.False(equalsDifferentObject);
        Assert.False(equalsNull);
        Assert.False(equalsDifferent);
        Assert.True(equalOperator);
        Assert.True(notEqualOperator);
        Assert.Equal(leftHashCode, sameHashCode);
    }

    [Fact]
    public void Guard_NotNull_ReturnsValue_WhenValid()
    {
        // Arrange
        var instance = new object();

        // Act
        var result = Guard.NotNull(instance, "instance");

        // Assert
        Assert.Same(instance, result);
    }

    [Fact]
    public void Guard_NotNull_ThrowsWithDefaultMessage_WhenNull()
    {
        // Act
        var action = () =>
        {
            _ = Guard.NotNull<object>(null, "value");
        };

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(action);

        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Parameter 'value' cannot be null.", exception.Message);
    }

    [Fact]
    public void Guard_NotNull_ThrowsWithCustomMessage_WhenNull()
    {
        // Act
        var action = () =>
        {
            _ = Guard.NotNull<object>(null, "value", "custom");
        };

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(action);

        Assert.Contains("custom", exception.Message);
    }

    [Fact]
    public void Guard_NotEmpty_ReturnsValue_WhenValid()
    {
        // Act
        var result = Guard.NotEmpty("foundation", "value");

        // Assert
        Assert.Equal("foundation", result);
    }

    [Fact]
    public void Guard_NotEmpty_ThrowsWithExpectedMessage_WhenInvalid()
    {
        // Arrange
        var defaultAction = () =>
        {
            _ = Guard.NotEmpty(" ", "value");
        };
        var customAction = () =>
        {
            _ = Guard.NotEmpty(null, "value", "custom");
        };

        // Act
        var defaultException = Assert.Throws<ArgumentException>(defaultAction);
        var customException = Assert.Throws<ArgumentException>(customAction);

        // Assert
        Assert.Equal("value", defaultException.ParamName);
        Assert.Contains("Parameter 'value' cannot be null, empty, or whitespace.", defaultException.Message);
        Assert.Contains("custom", customException.Message);
    }

    [Fact]
    public void Guard_NotDefault_ReturnsValue_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = Guard.NotDefault(id, "id");

        // Assert
        Assert.Equal(id, result);
    }

    [Fact]
    public void Guard_NotDefault_ThrowsWithExpectedMessage_WhenInvalid()
    {
        // Arrange
        var defaultAction = () =>
        {
            _ = Guard.NotDefault(Guid.Empty, "id");
        };
        var customAction = () =>
        {
            _ = Guard.NotDefault(0, "value", "custom");
        };

        // Act
        var defaultException = Assert.Throws<ArgumentException>(defaultAction);
        var customException = Assert.Throws<ArgumentException>(customAction);

        // Assert
        Assert.Equal("id", defaultException.ParamName);
        Assert.Contains("Parameter 'id' cannot be the default value.", defaultException.Message);
        Assert.Contains("custom", customException.Message);
    }

    [Fact]
    public void Maybe_FactoryAndImplicitNone_WorkAsExpected()
    {
        // Arrange
        Maybe<string> noneFromMarker = Maybe.None;

        // Act
        var maybe = Maybe.From("foundation");

        // Assert
        Assert.False(noneFromMarker.HasValue);
        Assert.Equal("foundation", maybe.Value);
        Assert.True(maybe.HasValue);
    }

    [Fact]
    public void Maybe_Value_Throws_WhenNoValueIsPresent()
    {
        // Arrange
        var maybe = Maybe<int>.None;

        // Act
        var action = () =>
        {
            _ = maybe.Value;
        };

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("Cannot access value when Maybe has no value.", exception.Message);
    }

    [Fact]
    public void Maybe_From_Throws_WhenValueIsNull()
    {
        // Act
        var action = () =>
        {
            _ = Maybe<string>.From(null!);
        };

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Maybe_Equality_AndHashCode_WorkForAllBranches()
    {
        // Arrange
        var left = Maybe<int>.From(42);
        var same = Maybe<int>.From(42);
        var different = Maybe<int>.From(7);
        var none = Maybe<int>.None;

        // Act
        var equalsTyped = left.Equals(same);
        var equalsObject = left.Equals((object)same);
        var equalsDifferentObject = left.Equals("foundation");
        var equalsDifferent = left.Equals(different);
        var equalsNone = left.Equals(none);
        var leftHashCode = left.GetHashCode();
        var sameHashCode = same.GetHashCode();
        var noneHashCode = none.GetHashCode();
        var equalOperator = left == same;
        var notEqualOperator = left != different;

        // Assert
        Assert.True(equalsTyped);
        Assert.True(equalsObject);
        Assert.False(equalsDifferentObject);
        Assert.False(equalsDifferent);
        Assert.False(equalsNone);
        Assert.Equal(leftHashCode, sameHashCode);
        Assert.Equal(0, noneHashCode);
        Assert.True(equalOperator);
        Assert.True(notEqualOperator);
    }
}
