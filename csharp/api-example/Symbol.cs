namespace ApiExample;

public class Symbol
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public double BidMarkup { get; set; }
    public double AskMarkup { get; set; }
    public int Throttling { get; set; } = 2;
    public double SpreadBalance { get; set; }
    public int DecimalPlaces { get; set; } = 4;
    public int LotSize { get; set; } = 100000;
    public double TickValue { get; set; }
    public double TickSize { get; set; }
    public string TradeMode { get; set; } = "Full";
    public double MinOrderSize { get; set; } = 0.001;
    public double MaxOrderSize { get; set; } = 10000;
    public double OrderSizeIncrement { get; set; } = 0.001;
    public double MaxPosition { get; set; }
    public bool AllowNegativePrices { get; set; }
    public bool AllowRealtimeQuotes { get; set; } = true;
    public int MarketDepth { get; set; } = 25;
    public double MinSpreadFilter { get; set; }
    public double MaxSpreadFilter { get; set; }
    public int SoftFilter { get; set; } = 100;
    public int SoftFilterTickCount { get; set; } = 10;
    public int HardFilter { get; set; } = 500;
    public int HardFilterTickCount { get; set; } = 10;
    public int DiscardFilter { get; set; } = 500;
    public double GapDiff { get; set; }
    public int GapTickCount { get; set; }
    public double MaxPriceLimit { get; set; }
    public double MinPriceLimit { get; set; }
    public int BaseCurrencyDecimalPlaces { get; set; } = 2;
    public int ProfitCurrencyDecimalPlaces { get; set; } = 2;
    public int MarginCurrencyDecimalPlaces { get; set; } = 2;
    public string MarginCalcMode { get; set; } = "Forex";
    public string ProfitCalcMode { get; set; } = "Forex";
    public int StopsLevel { get; set; } = 5;
    public int FreezeLevel { get; set; }
    public bool AllowMarketOrders { get; set; } = true;
    public bool AllowLimitOrders { get; set; } = true;
    public bool AllowStopOrders { get; set; } = true;
    public bool AllowStopLimitOrders { get; set; } = true;
    public bool AllowStopLossOrders { get; set; } = true;
    public bool AllowTakeProfitOrders { get; set; } = true;
    public bool AllowCloseByOrders { get; set; } = true;
    public string ChartMode { get; set; } = "BidPrice";
    public int QuotesTimeout { get; set; } = 600;
    public double InitialMargin { get; set; } = 0.0;
    public double HedgedMargin { get; set; } = 100000;
    public double MaintenanceMargin { get; set; } = 0.0;
    public double InitialBuyMarginRate { get; set; } = 1.0;
    public double InitialSellMarginRate { get; set; } = 1.0;
    public double InitialBuyLimitMarginRate { get; set; } = 1.0;
    public double InitialSellLimitMarginRate { get; set; }
    public double InitialBuyStopMarginRate { get; set; }
    public double InitialSellStopMarginRate { get; set; }
    public double InitialBuyStopLimitMarginRate { get; set; }
    public double InitialSellStopLimitMarginRate { get; set; }
    public double MaintenanceBuyMarginRate { get; set; }
    public double MaintenanceSellMarginRate { get; set; }
    public double MaintenanceBuyLimitMarginRate { get; set; }
    public double MaintenanceSellLimitMarginRate { get; set; }
    public double MaintenanceBuyStopMarginRate { get; set; }
    public double MaintenanceSellStopMarginRate { get; set; }
    public double MaintenanceBuyStopLimitMarginRate { get; set; }
    public double MaintenanceSellStopLimitMarginRate { get; set; }
    public double LiquidityMarginRate { get; set; }
    public double Percentage { get; set; }
    public string BaseCurrency { get; set; } = "";
    public string ProfitCurrency { get; set; } = "";
    public string MarginCurrency { get; set; } = "";
    public bool MarginUseLargerLeg { get; set; }
    public bool RecalculateMarginRateAtEod { get; set; }
    public bool MarginCheckOnSlTp { get; set; }
    public bool AllowFok { get; set; } = true;
    public bool AllowIoc { get; set; } = true;
    public bool AllowGtc { get; set; } = true;
    public bool AllowGtd { get; set; } = true;
    public bool AllowDayOrders { get; set; }
    public bool AllowValidForMilliseconds { get; set; }
    public double SwapLong { get; set; }
    public double SwapShort { get; set; }
    public int SwapRateSunday { get; set; }
    public int SwapRateMonday { get; set; } = 1;
    public int SwapRateTuesday { get; set; } = 1;
    public int SwapRateWednesday { get; set; } = 3;
    public int SwapRateThursday { get; set; } = 1;
    public int SwapRateFriday { get; set; } = 1;
    public int SwapRateSaturday { get; set; }
    public int SwapYearDays { get; set; }
    public string SwapMode { get; set; } = "Points";
    public long StartDateTime { get; set; }
    public long EndDateTime { get; set; }
    public string Exchange { get; set; } = "";
    public string Country { get; set; } = "";
    public string Webpage { get; set; } = "";
    public string UnderlyingAsset { get; set; } = "";
    public string Isin { get; set; } = "";
    public string Cfi { get; set; } = "";

    public List<Session> QuoteSessions { get; set; } = [];
    public List<Session> TradeSessions { get; set; } = [];
}