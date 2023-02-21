using Grpc.Core;
using ShireBank.Repository.Enums;
using ShireBank.Repository.Repositories.Interfaces;
using ShireBank.Shared.Protos;

namespace ShireBank.Server.Services;

public class CustomerService : Customer.CustomerBase
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankTransactionRepository _bankTransactionRepository;

    public CustomerService(IBankAccountRepository bankAccountRepository, IBankTransactionRepository bankTransactionRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _bankTransactionRepository = bankTransactionRepository;
    }
    
    public override async Task<OpenAccountResponse> OpenAccount(OpenAccountRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.FirstName))
            throw new RpcException(new Status(StatusCode.Aborted, "First name is required"));

        if (string.IsNullOrEmpty(request.LastName))
            throw new RpcException(new Status(StatusCode.Aborted, "Last name is required"));

        if (string.IsNullOrEmpty(request.DebtLimit.ToString()))
            throw new RpcException(new Status(StatusCode.Aborted, "Debt limit is required"));

        var account = await _bankAccountRepository.OpenAccount(request.FirstName, request.LastName, request.DebtLimit);

        return new OpenAccountResponse
        {
            AccountId = account.Id
        };
    }

    public override async Task<CloseAccountResponse> CloseAccount(CloseAccountRequest request, ServerCallContext context)
    {
        var isClosed = await _bankAccountRepository.CloseAccount(request.AccountId);

        return new CloseAccountResponse
        {
            IsClosed = isClosed
        };
    }

    public override async Task<WithdrawResponse> WithDraw(WithdrawRequest request, ServerCallContext context)
    {
        if (request.Amount <= 0)
            throw new RpcException(new Status(StatusCode.Aborted, "Withdraw amount has to be positive"));

        var withdrawnAmount = await _bankAccountRepository.Withdraw(request.AccountId, request.Amount);
        await _bankTransactionRepository.Create(request.AccountId, -withdrawnAmount, BankTransactionType.Withdraw);

        return new WithdrawResponse
        {
            Value = withdrawnAmount
        };
    }

    public override async Task<DepositResponse> Deposit(DepositRequest request, ServerCallContext context)
    {
        if (request.Amount <= 0)
            throw new RpcException(new Status(StatusCode.Aborted, "Deposit amount has to be positive"));

        await _bankAccountRepository.Deposit(request.AccountId, request.Amount);
        await _bankTransactionRepository.Create(request.AccountId, request.Amount, BankTransactionType.Deposit);

        return new DepositResponse();
    }

    public override async Task<GetHistoryResponse> GetHistory(GetHistoryRequest request, ServerCallContext context)
    {
        var transactions = await _bankTransactionRepository.GetAll(request.AccountId);

        return new GetHistoryResponse
        {
            History = string.Join("\n", transactions.Select(x => x.ToString()))
        };
    }
}