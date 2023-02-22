using ShireBank.Repository.Models;

namespace ShireBank.Repository.Repositories.Interfaces;

public interface IBankAccountRepository
{
    Task<BankAccount> OpenAccount(string firstName, string lastName, decimal debtLimit);
    Task<bool> CloseAccount(uint accountId);
    Task<decimal> Withdraw(uint accountId, decimal amount);
    Task Deposit(uint accountId, decimal amount);
    Task<BankAccount?> GetAccount(uint accountId);
}