namespace ShireBank.Shared;

public static class Constants
{
    public const string BankBaseAddress = "http://localhost";
    public const int BankBasePort = 6999;
    public static string BankFullAddress => $"{BankBaseAddress}:{BankBasePort}";
}