﻿using ShireBank.Repository.Enums;
using ShireBank.Repository.Models;

namespace ShireBank.Repository.Repositories.Interfaces;

public interface IBankTransactionRepository
{
    Task<BankTransaction> Create(uint accountId, decimal amount, BankTransactionType transactionType);
    Task<IEnumerable<BankTransaction>> GetAll(uint accountId);
}