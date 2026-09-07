namespace TaxOmbud.Domain.Enums;

/// <summary>
/// Describes how a complaint was received by the Tax Ombud office.
/// Set at Stage 1 (Intake) and displayed as an informational badge throughout the case lifecycle.
/// </summary>
public enum IntakeChannel
{
    OnlinePortal = 1,
    CallCentre = 2,
    Email = 3,
    WalkIn = 4,
    Letter = 5
}
