using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ET.Models;

namespace ET.Services
{
    public class TransactionService
    {
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static readonly string FolderPath = Path.Combine(DesktopPath, "ET");
        private static readonly string TransactionsFilePath = Path.Combine(FolderPath, "transactions.json");
        private static readonly string DebtsFilePath = Path.Combine(FolderPath, "debts.json");
        private static readonly string CashFlowsFilePath = Path.Combine(FolderPath, "cashflows.json");

        private readonly List<Transaction> transactions;
        private readonly List<Debt> debts;
        private readonly List<CashFlow> cashFlows;

        public TransactionService()
        {
            transactions = LoadData<Transaction>(TransactionsFilePath);
            debts = LoadData<Debt>(DebtsFilePath);
            cashFlows = LoadData<CashFlow>(CashFlowsFilePath);
        }

        private List<T> LoadData<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from {filePath}: {ex.Message}");
                return new List<T>();
            }
        }

        private void SaveData<T>(List<T> data, string filePath)
        {
            EnsureFolderExists();
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File write error: {ex.Message}");
            }
        }

        private void EnsureFolderExists()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        public List<Transaction> GetTransactions() => transactions;

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await Task.FromResult(transactions);
        }

        public List<Debt> GetDebts() => debts;

        public async Task<List<Debt>> GetDebtsAsync()
        {
            return await Task.FromResult(debts);
        }

        public List<CashFlow> GetCashInflows() => cashFlows.Where(cf => cf.IsInflow).ToList();

        public async Task<List<CashFlow>> GetCashInflowsAsync()
        {
            return await Task.FromResult(cashFlows.Where(cf => cf.IsInflow).ToList());
        }

        public List<CashFlow> GetCashOutflows() => cashFlows.Where(cf => !cf.IsInflow).ToList();

        public async Task<List<CashFlow>> GetCashOutflowsAsync()
        {
            return await Task.FromResult(cashFlows.Where(cf => !cf.IsInflow).ToList());
        }

        public async Task AddCashInflowAsync(CashFlow cashFlow)
        {
            cashFlow.Id = GenerateId(cashFlows);
            cashFlow.IsInflow = true;
            cashFlows.Add(cashFlow);
            SaveData(cashFlows, CashFlowsFilePath);
            await Task.CompletedTask;
        }

        public async Task<string> AddCashOutflowAsync(CashFlow cashFlow)
        {
            decimal currentBalance = GetMainBalance();
            if (currentBalance < cashFlow.Amount)
            {
                return "Insufficient Amount";
            }

            cashFlow.Id = GenerateId(cashFlows);
            cashFlow.IsInflow = false;
            cashFlows.Add(cashFlow);
            SaveData(cashFlows, CashFlowsFilePath);
            await Task.CompletedTask;
            return string.Empty;
        }

        public async Task AddDebtAsync(Debt debt)
        {
            debt.Id = GenerateId(debts);
            debts.Add(debt);
            SaveData(debts, DebtsFilePath);
            await Task.CompletedTask;
        }

        public async Task<string> ClearDebtAsync(int userId)
        {
            decimal currentBalance = GetMainBalance();
            decimal totalDebt = GetTotalDebt();

            if (currentBalance < totalDebt)
            {
                return "Insufficient Amount to Clear Debt";
            }

            foreach (var debt in debts)
            {
                if (debt.UserId == userId)
                {
                    debt.IsCleared = true;
                }
            }
            SaveData(debts, DebtsFilePath);
            await Task.CompletedTask;
            return "Debt Cleared";
        }

        public async Task ClearDebtByIdAsync(int debtId)
        {
            var debt = debts.FirstOrDefault(d => d.Id == debtId);
            if (debt == null)
            {
                Console.WriteLine($"Debt with ID {debtId} not found.");
                return;
            }

            if (debt.IsCleared)
            {
                Console.WriteLine($"Debt with ID {debtId} is already cleared.");
                return;
            }

            debt.IsCleared = true;
            SaveData(debts, DebtsFilePath);

            Console.WriteLine($"Debt with ID {debtId} cleared successfully.");
            await Task.CompletedTask;
        }

        public async Task UpdateDebtAsync(Debt updatedDebt)
        {
            var debt = debts.FirstOrDefault(d => d.Id == updatedDebt.Id);
            if (debt != null)
            {
                debt.Amount = updatedDebt.Amount;
                debt.PaidAmount = updatedDebt.PaidAmount;
                debt.Date = updatedDebt.Date;
                debt.Description = updatedDebt.Description;
                debt.IsCleared = updatedDebt.IsCleared;
                SaveData(debts, DebtsFilePath);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateCashFlowAsync(CashFlow updatedCashFlow)
        {
            var cashFlow = cashFlows.FirstOrDefault(cf => cf.Id == updatedCashFlow.Id);
            if (cashFlow != null)
            {
                cashFlow.Amount = updatedCashFlow.Amount;
                cashFlow.Date = updatedCashFlow.Date;
                cashFlow.Category = updatedCashFlow.Category;
                cashFlow.IsInflow = updatedCashFlow.IsInflow;
                SaveData(cashFlows, CashFlowsFilePath);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateTransactionAsync(Transaction updatedTransaction)
        {
            var transaction = transactions.FirstOrDefault(t => t.Id == updatedTransaction.Id);
            if (transaction != null)
            {
                transaction.Amount = updatedTransaction.Amount;
                transaction.Debit = updatedTransaction.Debit;
                transaction.Credit = updatedTransaction.Credit;
                transaction.Date = updatedTransaction.Date;
                transaction.Description = updatedTransaction.Description;
                transaction.Type = updatedTransaction.Type;
                SaveData(transactions, TransactionsFilePath);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteDebtAsync(int debtId)
        {
            var debt = debts.FirstOrDefault(d => d.Id == debtId);
            if (debt != null)
            {
                debts.Remove(debt);
                SaveData(debts, DebtsFilePath);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteCashFlowAsync(int cashFlowId)
        {
            var cashFlow = cashFlows.FirstOrDefault(cf => cf.Id == cashFlowId);
            if (cashFlow != null)
            {
                cashFlows.Remove(cashFlow);
                SaveData(cashFlows, CashFlowsFilePath);
            }
            await Task.CompletedTask;
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = transactions.FirstOrDefault(t => t.Id == transactionId);
            if (transaction != null)
            {
                transactions.Remove(transaction);
                SaveData(transactions, TransactionsFilePath);
            }
            await Task.CompletedTask;
        }

        public decimal GetMainBalance()
        {
            decimal totalInflow = cashFlows.Where(cf => cf.IsInflow).Sum(cf => cf.Amount);
            decimal totalOutflow = cashFlows.Where(cf => !cf.IsInflow).Sum(cf => cf.Amount);
            decimal totalDebt = debts.Where(d => !d.IsCleared).Sum(d => d.Amount);
            return totalInflow - totalOutflow + totalDebt;
        }

        public async Task<decimal> GetMainBalanceAsync()
        {
            return await Task.FromResult(GetMainBalance());
        }

        public decimal GetTotalDebt()
        {
            return debts.Where(d => !d.IsCleared).Sum(d => d.Amount);
        }

        public async Task<decimal> GetTotalDebtAsync()
        {
            return await Task.FromResult(GetTotalDebt());
        }

        private int GenerateId<T>(List<T> items) where T : IEntity
        {
            return items.Count > 0 ? items.Max(i => i.Id) + 1 : 1;
        }
    }
}