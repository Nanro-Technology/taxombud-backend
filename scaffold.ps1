 = 'c:\Projects\taxombud\src\TaxOmbud.Application\Features'

function Create-Feature ($module, $action, $type) {
    $featuresDir = 'c:\Projects\taxombud\src\TaxOmbud.Application\Features'
    $dir = "$featuresDir\$module\$type\$action"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $className = "$action$type"
    $responseClass = "$actionResponse"
    
    $content = "using MediatR;
using TaxOmbud.Application.Common.Models;
using System.Collections.Generic;

namespace TaxOmbud.Application.Features.$module.$type.$action;

public record $className : IRequest<Result<$responseClass>>
{
}

public class $responseClass
{
    public bool Success { get; set; }
}

public class $($className)Handler : IRequestHandler<$className, Result<$responseClass>>
{
    public async Task<Result<$responseClass>> Handle($className request, System.Threading.CancellationToken cancellationToken)
    {
        return Result<$responseClass>.Success(new $responseClass { Success = true });
    }
}
"
    Set-Content -Path "\$($className).cs" -Value $content
}

# Payroll Module
Create-Feature -module 'Payroll' -action 'GetPayrollPeriods' -type 'Queries'
Create-Feature -module 'Payroll' -action 'GetSalaryProfiles' -type 'Queries'
Create-Feature -module 'Payroll' -action 'GetRemittances' -type 'Queries'
Create-Feature -module 'Payroll' -action 'RunPayroll' -type 'Commands'
Create-Feature -module 'Payroll' -action 'ApprovePayroll' -type 'Commands'
Create-Feature -module 'Payroll' -action 'CreateSalaryProfile' -type 'Commands'

# Wallet Module
Create-Feature -module 'Wallet' -action 'GetWalletBalance' -type 'Queries'
Create-Feature -module 'Wallet' -action 'GetWalletTransactions' -type 'Queries'
Create-Feature -module 'Wallet' -action 'RequestWithdrawal' -type 'Commands'
Create-Feature -module 'Wallet' -action 'ProcessWithdrawal' -type 'Commands'

# HR Requests Module
Create-Feature -module 'HrRequests' -action 'GetLeaveRequests' -type 'Queries'
Create-Feature -module 'HrRequests' -action 'GetLoanRequests' -type 'Queries'
Create-Feature -module 'HrRequests' -action 'GetEwaRequests' -type 'Queries'
Create-Feature -module 'HrRequests' -action 'SubmitLeaveRequest' -type 'Commands'
Create-Feature -module 'HrRequests' -action 'ApproveLeaveRequest' -type 'Commands'
Create-Feature -module 'HrRequests' -action 'SubmitLoanRequest' -type 'Commands'

