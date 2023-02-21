using ShireBank.Shared.Constants;
using Grpc.Core;
using Grpc.Net.Client;
using NLog;
using ShireBank.Shared.Protos;

namespace ShireBank.Client;

internal static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly object HistoryPrintLock = new();
    
    private static async Task Main()
    {
        using var channel = GrpcChannel.ForAddress(Constants.BankFullAddress, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });
        
        Logger.Info("Starting tasks execution...");

        Task[] tasks =
        {
            TestCustomerOne(channel),
            TestCustomerTwo(channel),
            TestCustomerThree(channel)
        };
        Task.WaitAll(tasks);
        
        Logger.Info("Finished executing tasks. Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task TestCustomerOne(ChannelBase channel)
    {
        var customer = new Customer.CustomerClient(channel);
        var customerName = "Customer 1";

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        LogOpenCloseAccountAction(customerName, nameof(customer.OpenAccount));
        var accountId = await customer.OpenAccountAsync(new OpenAccountRequest
        {
            FirstName = "Henrietta",
            LastName = "Baggins",
            DebtLimit = 100.0m
        });
        
        if (!accountId.AccountId.HasValue) 
            throw new Exception("Failed to open account");
        
        LogTransactionAction(customerName, 500.0m, nameof(customer.Deposit));
        await customer.DepositAsync(new DepositRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 500.0m
        });
        
        await Task.Delay(TimeSpan.FromSeconds(10));
        
        LogTransactionAction(customerName, 500.0m, nameof(customer.Deposit));
        await customer.DepositAsync(new DepositRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 500.0m
        });
        
        LogTransactionAction(customerName, 1000.0m, nameof(customer.Deposit));
        await customer.DepositAsync(new DepositRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 1000.0m
        });
        
        LogTransactionAction(customerName, 2000.0m, nameof(customer.WithDraw));
        var withdrawResponse = await customer.WithDrawAsync(new WithdrawRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 2000.0m
        });
        
        if (2000.0m != withdrawResponse.Value)
            throw new Exception("Can't withdraw a valid amount");
        
        lock (HistoryPrintLock)
        {
            Logger.Info($"=== {customerName} ===");
        
            var getHistoryResponse = customer.GetHistory(new GetHistoryRequest
            {
                AccountId = accountId.AccountId.Value
            });
        
            foreach (var line in getHistoryResponse.History.Split("\n"))
            {
                Logger.Info(line);
            }
        }
        
        LogOpenCloseAccountAction(customerName, nameof(customer.CloseAccount));
        var closeAccountResponse = await customer.CloseAccountAsync(new CloseAccountRequest
        {
            AccountId = accountId.AccountId.Value
        });
        
        if (!closeAccountResponse.IsClosed)
            throw new Exception("Failed to close account");
    }

    private static async Task TestCustomerTwo(ChannelBase channel)
    {
        var customer = new Customer.CustomerClient(channel);
        var customerName = "Customer 2";

        var openAccountRequest = new OpenAccountRequest
        {
            FirstName = "Barbara",
            LastName = "Tuk",
            DebtLimit = 50.0m
        };

        LogOpenCloseAccountAction(customerName, nameof(customer.OpenAccount));
        var accountId = await customer.OpenAccountAsync(openAccountRequest);

        if (!accountId.AccountId.HasValue)
            throw new Exception("Failed to open account");

        Logger.Info($"{customerName} - testing second operation {nameof(customer.OpenAccount)}");
        openAccountRequest.DebtLimit = 500.0m;
        var secondAccountId = await customer.OpenAccountAsync(openAccountRequest);

        if (secondAccountId.AccountId.HasValue)
            throw new Exception("Opened account for the same name twice!");

        Logger.Info($"{customerName} - testing operation {nameof(customer.WithDraw)} over available limit");
        var withdrawResponse1 = await customer.WithDrawAsync(new WithdrawRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 2000.0m
        });

        if (50.0m != withdrawResponse1.Value)
            throw new Exception("Can only borrow up to debit limit only");

        await Task.Delay(TimeSpan.FromSeconds(10));

        var closeAccountRequest = new CloseAccountRequest
        {
            AccountId = accountId.AccountId.Value
        };
        
        Logger.Info($"{customerName} - testing operation {nameof(customer.CloseAccount)} with outstanding debt");
        if ((await customer.CloseAccountAsync(closeAccountRequest)).IsClosed)
            throw new Exception("Can't close the account with outstanding debt");
        
        LogTransactionAction(customerName, 100.0m, nameof(customer.Deposit));
        await customer.DepositAsync(new DepositRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 100.0m
        });
        
        Logger.Info($"{customerName} - testing operation {nameof(customer.CloseAccount)} with money still on account");
        if ((await customer.CloseAccountAsync(closeAccountRequest)).IsClosed)
            throw new Exception("Can't close the account before clearing all funds");
        
        LogTransactionAction(customerName, 50.0m, nameof(customer.WithDraw));
        var withdrawResponse2 = await customer.WithDrawAsync(new WithdrawRequest
        {
            AccountId = accountId.AccountId.Value,
            Amount = 50.0m
        });
        
        if (50.0m != withdrawResponse2.Value)
            throw new Exception("Can't withdraw a valid amount");

        lock (HistoryPrintLock)
        {
            Logger.Info($"=== {customerName} ===");
        
            var getHistoryResponse = customer.GetHistory(new GetHistoryRequest
            {
                AccountId = accountId.AccountId.Value
            });
        
            foreach (var line in getHistoryResponse.History.Split("\n"))
            {
                Logger.Info(line);
            }
        }
        
        LogOpenCloseAccountAction(customerName, nameof(customer.CloseAccount));
        var closeAccountResponse = await customer.CloseAccountAsync(new CloseAccountRequest
        {
            AccountId = accountId.AccountId.Value
        });
        
        if (!closeAccountResponse.IsClosed)
            throw new Exception("Failed to close account");
    }

    private static async Task TestCustomerThree(ChannelBase channel)
    {
        var customer = new Customer.CustomerClient(channel);
        var customerName = "Customer 3";
        
        LogOpenCloseAccountAction(customerName, nameof(customer.OpenAccount));
        var accountId = await customer.OpenAccountAsync(new OpenAccountRequest
        {
            FirstName = "Gandalf",
            LastName = "Grey",
            DebtLimit = 10000.0m
        });
        
        if (!accountId.AccountId.HasValue) 
            throw new Exception("Failed to open account");

        var tasks = new List<Task>();

        for (var i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                LogTransactionAction(customerName, 10.0m, nameof(customer.WithDraw));
                var withdrawResponse = await customer.WithDrawAsync(new WithdrawRequest
                {
                    AccountId = accountId.AccountId.Value,
                    Amount = 10.0m
                });
                
                if (10.0m != withdrawResponse.Value)
                    throw new Exception("Can't withdraw a valid amount");
            }));
        }

        for (var i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                LogTransactionAction(customerName, 10.0m, nameof(customer.Deposit));
                await customer.DepositAsync(new DepositRequest
                {
                    AccountId = accountId.AccountId.Value,
                    Amount = 10.0m
                });
            }));
        }

        Task.WaitAll(tasks.ToArray());

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        lock (HistoryPrintLock)
        {
            Logger.Info($"=== {customerName} ===");
        
            var getHistoryResponse = customer.GetHistory(new GetHistoryRequest
            {
                AccountId = accountId.AccountId.Value
            });
        
            foreach (var line in getHistoryResponse.History.Split("\n"))
            {
                Logger.Info(line);
            }
        }
        
        LogOpenCloseAccountAction(customerName, nameof(customer.CloseAccount));
        var closeAccountResponse = await customer.CloseAccountAsync(new CloseAccountRequest
        {
            AccountId = accountId.AccountId.Value
        });
        
        if (!closeAccountResponse.IsClosed)
            throw new Exception("Failed to close account");
    }

    private static void LogOpenCloseAccountAction(string customerName, string action)
    {
        Logger.Info($"{customerName} - testing operation {action}");
    }

    private static void LogTransactionAction(string customerName, decimal amount, string action)
    {
        Logger.Info($"{customerName} - testing operation {action} {amount}");
    }
}

