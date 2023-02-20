using ShireBank.Repository.Enums;

namespace ShireBank.Repository.Models;

public class BankTransaction
{
    public BankTransaction()
    {
    }
    
    public BankTransaction(BankAccount account, decimal value)
    {
        Account = account;
        Value = value;
    }
    
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal Value { get; }
    public BankTransactionType Type { get; }
    public BankAccount Account { get; }

    public override string ToString()
    {
        // TO DO: apply transaction type
        return $"{Type} - transaction {Id} at {CreatedAt} for {Value}";
    }
}