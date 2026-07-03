namespace TaxOmbud.Application.Taxpayers.DTOs;

public record VerifyTaxpayerCommand(Guid TaxpayerId, bool IsVerified) ;

public record VerifyTaxpayerRequest(bool IsVerified);
