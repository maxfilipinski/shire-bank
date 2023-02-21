namespace ShireBank.Repository.Models;

public class BankAccount
{
    public uint Id { get; private set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public decimal DebtLimit { get; set; }
    public decimal Balance { get; set; }
    public IEnumerable<BankTransaction> Transactions { get; private set; }
}