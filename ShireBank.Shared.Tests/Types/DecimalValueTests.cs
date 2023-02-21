using NUnit.Framework;
using ShireBank.Shared.Types;

namespace ShireBank.Shared.Tests.Types;

public class DecimalValueTests
{
    [Test]
    public void Should_Cast_To_Decimal_Value_Type()
    {
        // Arrange
        var expected = 666.999m;
        var decimalValue = new DecimalValue(666, 999000000);

        // Act
        decimal actual = decimalValue;

        // Assert
        Assert.AreEqual(expected, actual);
    }
}