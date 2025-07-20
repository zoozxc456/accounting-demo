using Accounting.Domain.ValueObjects;

namespace Accounting.UnitTest.Domain;

[TestFixture]
public class MoneyTests
{
    [TestCase(null, "幣別不可為 null")]
    [TestCase(" ", "幣別不可為空白")]
    [TestCase("US", "幣別長度應為 3")]
    [TestCase("USDD", "幣別長度應為 3")]
    public void Create_WhenCurrencyIsInvalid_ShouldThrowArgumentException(string? currency, string expectedMessage)
    {
        Assert.Throws<ArgumentException>(() => new Money(currency, 100m), expectedMessage);
    }

    [Test]
    public void Create_WhenAmountIsNegative_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        // 驗證金額為負數的情況
        Assert.Throws<ArgumentException>(() => new Money("TWD", -100m));
    }

    [Test]
    public void Equality_WhenAmountAndCurrencyAreSame_ShouldBeEqual()
    {
        // Arrange
        var moneyA = new Money("USD", 10.5m);
        var moneyB = new Money("USD", 10.5m);

        Assert.Multiple(() =>
        {
            Assert.That(moneyA == moneyB, Is.True);
            Assert.That(moneyA.Equals(moneyB), Is.True);
        });
    }

    [Test]
    public void Operator_WhenAddingDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var moneyA = new Money("TWD", 100m);
        var moneyB = new Money("USD", 50m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => { _ = moneyA + moneyB; }, "不同幣別的金額無法進行運算。");
    }
}