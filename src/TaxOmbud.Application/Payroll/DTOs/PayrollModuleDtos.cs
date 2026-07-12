using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.Payroll.DTOs;

public record SaveSalaryProfileCommand(
    Guid? Id, 
    Guid UserId, 
    decimal Basic, 
    string Allowances, 
    string Deductions, 
    DateTimeOffset EffectiveFrom, 
    string Currency, 
    string Status
);

public record CreateStatutoryDeductionCommand(
    string Name, 
    string Code, 
    string Country, 
    bool IsEmployee, 
    bool IsEmployer, 
    string Status
);

public record CreateStatutoryRuleCommand(
    string AppliesTo, 
    string Basis, 
    decimal RateOrAmount, 
    string RateOrAmountStr, 
    DateTime EffectiveDate, 
    DateTime? EndDate
);

public record SavePayoutProviderCommand(
    Guid? Id, 
    string Name, 
    string ProviderCode, 
    string Type, 
    string Adapter, 
    string Country, 
    string Currency, 
    string? PublicKey, 
    string? SecretKey, 
    string? WebhookSecret, 
    string? Notes, 
    string Status
);

public record CreatePayrollPeriodCommand(
    string Name, 
    DateTimeOffset StartDate, 
    DateTimeOffset EndDate, 
    string Currency
);

public record SchedulerConfigDto(
    string PeriodType, 
    string RunDay, 
    string NotifyLead, 
    string DefaultCurrency, 
    bool AutoReview, 
    bool AutoApprove, 
    bool AutoPost, 
    bool NotifyCreate, 
    string Notes, 
    bool IsEnabled
);

public record ValidationResultDto(
    string Status, 
    int EmployeesCount, 
    decimal EstimatedGross, 
    string ValidatedAt, 
    List<ValidationErrorDto> Errors, 
    List<ValidationErrorDto> Warnings, 
    List<ValidationErrorDto> Info
);

public record ValidationErrorDto(
    string Code, 
    string Message, 
    string? FixUrl
);
