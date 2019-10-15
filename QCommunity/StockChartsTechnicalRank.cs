using QuantaculaCore;
using QuantaculaIndicators;
using System;

namespace QCommunity
{
    public class StockChartsTechnicalRank : IndicatorBase
    {
        public StockChartsTechnicalRank() : base()
        { }

        public StockChartsTechnicalRank(TimeSeries source, int multiplier, int smoothingPeriod, BarHistory bars) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = multiplier;
            Parameters[2].Value = smoothingPeriod;
            Parameters[3].Value = bars;
            Populate();
        }
        public override string Name { get { return "StockCharts Technical Rank"; } }

        public override string Abbreviation { get { return "SCTR"; } }

        public override string HelpDescription { get { return "The SCTR is a numerical score that gives a stock kind of a trend strength ranking. Stocks are assigned a score based on six key indicators covering different timeframes. The following ranking can be used if smoothing is ignored: 0-9.99 Weakest - 10-19.99 Weaker - 20-29.99 Weak - 30-39.99 Below Average - 40-59.99 Average - 60-69.99 Above Average - 70-79.99 Strong - 80-89.99 Stronger - 90-99.99 Strongest."; } }

        public override string PaneTag { get { return "SCTR"; } }

        public override PlotStyles DefaultPlotStyle { get { return PlotStyles.Line; } }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Multiplier", ParameterTypes.Int32, 1);
            AddParameter("SmoothingPeriod", ParameterTypes.Int32, 1);
            AddParameter("Bars", ParameterTypes.BarHistory, null);
        }

        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 multiplier = Parameters[1].AsInt;
            Int32 smoothingPeriod = Parameters[2].AsInt;
            BarHistory bars = Parameters[3].AsBarHistory;

            TimeSeries sctrTimeSeries = new TimeSeries(source.DateTimes);

            DateTimes = source.DateTimes;

            if (source.Count < 200 + 1)
                return;

            EMA slowEma = new EMA(source, 200);
            EMA mediumEma = new EMA(source, 50);
            EMA fastEma = new EMA(source, 9);
            ROC slowRoc = new ROC(source, 125);
            ROC mediumRoc = new ROC(source, 20);
            RSI fastRsi = new RSI(source, 14);

            for (int n = 0; n < source.Count; n++)
            {
                double slowEmaValue = ((bars.Close[n] - slowEma[n]) / slowEma[n]) * 100;
                double mediumEmaValue = (bars.Close[n] - mediumEma[n]) / mediumEma[n];
                double fastEmaValue = ((bars.Close[n] - fastEma[n]) / fastEma[n]) * 100;

                double slowValue = 60 * 0.01 * ((slowEmaValue + slowRoc[n]) / 2);
                double mediumValue = 30 * 0.01 * ((mediumEmaValue + mediumRoc[n]) / 2);
                double fastValue = 10 * 0.01 * ((fastEmaValue + ((fastRsi[n]) - 50)) / 2);

                double sctrValue = 50 + (multiplier * (slowValue + mediumValue + fastValue));
                sctrTimeSeries[n] = sctrValue >= 0.1 && sctrValue <= 99.9 ? sctrValue : sctrValue > 99.9 ? 99.9 : sctrValue < 0.1 ? 0.1 : 0;
                Values[n] = SMA.Calculate(n, sctrTimeSeries, smoothingPeriod);
            }
        }
    }
}