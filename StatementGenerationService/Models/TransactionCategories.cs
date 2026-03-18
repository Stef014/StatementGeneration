using System.ComponentModel;

namespace StatementGenerationService.Models.Enums;

public enum TransactionCategories
{
    [Description("Groceries")]
    Groceries = 1,
    [Description("Digital Payments")]
    DigitalPayments,
    [Description("Fuel")]
    Fuel,
    [Description("Loans")]
    Loans,
    [Description("Cash Withdrawal")]
    CashWithdrawal,
    [Description("Takeaways")]
    Takeaways,
    [Description("Clothing & Shoes")]
    ClothingAndShoes,
    [Description("Betting / Lottery")]
    BettingOrLottery,
    [Description("Cellphone")]
    Cellphone,
    [Description("Vehicle Maintenance")]
    VehicleMaintenance,
    [Description("Furniture & Appliances")]
    FurnitureAndAppliances,
    [Description("Telephone")]
    Telephone,
    [Description("Pharmacy")]
    Pharmacy,
    [Description("Other Personal & Family")]
    OtherPersonalAndFamily,
    [Description("Doctors & Therapists")]
    DoctorsAndTherapists,
    [Description("Personal Care")]
    PersonalCare,
    [Description("Parking")]
    Parking
}