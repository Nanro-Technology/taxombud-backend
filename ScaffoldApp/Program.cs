using System;
using System.IO;

class Program
{
    static void Main()
    {
        string baseDir = @"c:\Projects\taxombud\src\TaxOmbud.Application\Features";

        // Wallet Fixes
        string f = Path.Combine(baseDir, "Wallet", "Commands", "RequestWithdrawal", "RequestWithdrawalCommands.cs");
        string content = File.ReadAllText(f);
        content = content.Replace("wallet.Balance", "wallet.BalanceNgn");
        content = content.Replace("TransactionType = \"WithdrawalRequest\"", "Type = \"debit\", Reference = \"WithdrawalRequest\"");
        content = content.Replace("Status = \"Pending\",", "");
        content = content.Replace("Date = DateTime.UtcNow", "");
        File.WriteAllText(f, content);

        f = Path.Combine(baseDir, "Wallet", "Commands", "ProcessWithdrawal", "ProcessWithdrawalCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("tx.Status", "tx.Type"); // Using Type as status workaround or just comment out
        content = content.Replace("tx.Type = \"Completed\";", "");
        content = content.Replace("tx.Type = \"Rejected\";", "");
        content = content.Replace("wallet.Balance", "wallet.BalanceNgn");
        File.WriteAllText(f, content);

        // HrRequests Fixes
        f = Path.Combine(baseDir, "HrRequests", "Commands", "SubmitLoanRequest", "SubmitLoanRequestCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("StaffId = request.StaffId", "UserId = request.StaffId");
        content = content.Replace("RepaymentMonths = request.RepaymentMonths", "TermMonths = request.RepaymentMonths");
        content = content.Replace("DateRequested = DateTime.UtcNow", "");
        File.WriteAllText(f, content);

        f = Path.Combine(baseDir, "HrRequests", "Commands", "SubmitLeaveRequest", "SubmitLeaveRequestCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("StaffId = request.StaffId", "UserId = request.StaffId");
        content = content.Replace("Reason = request.Reason,", "Days = (int)(request.EndDate - request.StartDate).TotalDays,");
        content = content.Replace("DateRequested = DateTime.UtcNow", "");
        File.WriteAllText(f, content);

        // Payroll Fixes
        f = Path.Combine(baseDir, "Payroll", "Commands", "CreateSalaryProfile", "CreateSalaryProfileCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("StaffId = request.StaffId", "UserId = request.StaffId");
        content = content.Replace("BaseSalary = request.BaseSalary", "Basic = request.BaseSalary");
        content = content.Replace("EffectiveDate = DateTime.UtcNow", "EffectiveFrom = DateTime.UtcNow");
        File.WriteAllText(f, content);

        f = Path.Combine(baseDir, "Payroll", "Commands", "RunPayroll", "RunPayrollCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("ProcessedAt = DateTime.UtcNow", "PostedAt = DateTime.UtcNow");
        File.WriteAllText(f, content);

        f = Path.Combine(baseDir, "Payroll", "Commands", "ApprovePayroll", "ApprovePayrollCommands.cs");
        content = File.ReadAllText(f);
        content = content.Replace("run.ProcessedAt = DateTime.UtcNow;", "run.ApprovedAt = DateTime.UtcNow;");
        File.WriteAllText(f, content);
    }
}
