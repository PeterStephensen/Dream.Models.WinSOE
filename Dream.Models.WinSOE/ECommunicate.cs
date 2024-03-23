namespace Dream.Models.WinSOE
{
    public enum ECommunicate
    {
        Yes,
        No,
        Ok,
        ThankYou,
        YouAreFired,
        //YouAreHiredInStartup,
        //YouAreCustomerInStartup,
        AvertiseGood,
        AvertiseJob,
        JobApplication,
        IQuit,
        Death,
        CanIBuy,
        Inheritance,
        Statistics,
        PayCorporateTax,
        Initialize  // Only used during initialization
    }

    public enum EStatistics
    {
        FirmCloseNatural,
        FirmCloseTooBig,
        FirmCloseNegativeProfit,
        FirmCloseZeroEmployment,
        FirmNew,
        Death,
        Profit,
        CouldNotFindSupplier,
        CouldNotFindFirmWithGoods,
        CouldNotFindOpenFirm,
        ChangeShopInSearchForShop,
        ChangeShopInBuyFromShopNull,
        ChangeShopInBuyFromShopLookingForGoods,
        SuccesfullTrade,
        SuccesfullTradeNonZero,
        ZeroBudget,
        BuyFromShop,
        Inheritance,
        JobFromUnemployment,
        JobFromJob,
        JobFromUnemploymentAdvertise,
        JobFromJobAdvertise
    }

    public class Message
    {

        #region Public fields
        public ECommunicate ComID;
        public object Object;
        #endregion

    }

    public enum EShock
    {
        Base,
        Productivity,
        ProductivityAR1,
        Tsunami,
        ProductivitySector0,
        LaborSupply
    }


}