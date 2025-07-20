using Accounting.Domain.Aggregates.Account;

namespace Accounting.UnitTest.Domain;

[TestFixture]
public class AccountTests
{
    [Test]
    public void Create_WhenAccountNameIsNotEmpty_ShouldSucceed()
    {
        var account = Account.Create("現金", AccountType.Asset, 1000);

        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(account.Name, Is.EqualTo("現金"));
            Assert.That(account.Type, Is.EqualTo(AccountType.Asset));
            Assert.That(account.Balance, Is.EqualTo(1000m));
        });
    }

    [Test]
    public void Create_WhenAccountNameIsEmpty_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Account.Create("", AccountType.Asset), "科目名稱不可為空。 (Parameter 'name')");
    }

    [Test]
    public void Create_WhenInitialBalanceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => Account.Create("測試科目", AccountType.Asset, -100));
    }

    [Test]
    public void CreateWithId_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => Account.CreateWithId(Guid.Empty, "測試科目", AccountType.Asset));
    }

    [Test]
    public void CreateWithId_WhenAccountNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => Account.CreateWithId(Guid.NewGuid(), "", AccountType.Asset, 100));
    }

    [Test]
    public void CreateWithId_WhenInitialBalanceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => Account.CreateWithId(Guid.NewGuid(), "測試科目", AccountType.Asset, -100));
    }

    [Test]
    public void Debit_ShouldIncreaseBalance_ForAssetAccount()
    {
        // Arrange
        var account = Account.Create("庫存現金", AccountType.Asset, 1000);
        var expectedBalance = 1100m;

        // Act
        account.Debit(100);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(expectedBalance));
    }

    [Test]
    public void Credit_ShouldDecreaseBalance_ForAssetAccount()
    {
        // Arrange
        var account = Account.Create("庫存現金", AccountType.Asset, 1000);
        var expectedBalance = 900m;

        // Act
        account.Credit(100);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(expectedBalance));
    }

    [Test]
    public void UpdateName_WhenNewNameIsValid_ShouldChangeName()
    {
        // Arrange
        var account = Account.Create("舊名稱", AccountType.Asset);
        var newName = "新名稱";

        // Act
        account.UpdateName(newName);

        // Assert
        Assert.That(account.Name, Is.EqualTo(newName));
    }

    [Test]
    public void UpdateName_WhenNewNameIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var account = Account.Create("舊名稱", AccountType.Asset);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => account.UpdateName(null));
            Assert.Throws<ArgumentException>(() => account.UpdateName(" "));
        });
    }
}