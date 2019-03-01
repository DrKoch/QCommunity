using QuantaculaCore;
using System;
using System.Drawing;

namespace QuantaculaIndicators
{
    public class ZLEMA : IndicatorBase
    {
        //parameterless constructor
        public ZLEMA() : base()
        {
        }

        //for code based construction
        public ZLEMA(TimeSeries source, Int32 period, Double gain, double addToPeriod = 1.0)
            : base()
        {
            _addToPeriod = addToPeriod;
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = gain;

            Populate();
        }

        //static Series method
        public static ZLEMA Series(TimeSeries source, Int32 period, Double gain)
        {
            return new ZLEMA(source, period, gain);
        }

        //name
        public override string Name
        {
            get
            {
                return "ZLEMA";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "ZLEMA";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "Zero Lag EMA by John Ehlers and Ric Way. Try a gain value ~10% of period length for good results. 0.0 gain == EMA (no reduced lag)";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 0, 128, 255);
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.Line;
            }
        }

        //it's a smoother
        public override bool IsSmoother
        {
            get
            {
                return true;
            }
        }

        //populate
        // https://www.mesasoftware.com/papers/ZeroLag.pdf
        // EC = a*(Price + gain*(Price – EC[1])) + (1 – a)*EC[1];
        // a = alpha = 2 / (period + 1)
        // 
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double gain = Parameters[2].AsDouble;
            DateTimes = source.DateTimes;
            if (period <= 0)
                return;
            if (source.FirstValidIndex == source.Count)
                return;

            //calculate exponent (alpha)
            double exponent = 2.0 / (_addToPeriod + period);

            //initial value
            double sum = source[source.FirstValidIndex];

            //go forward
            for (var n = source.FirstValidIndex; n < source.Count; n++)
            {
                if (Double.IsNaN(source[n]))
                    continue;
                double val = source[n];
                double difference = exponent * (val + gain * (val - sum) - sum);
                sum += difference;
                Values[n] = sum;
            }

        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 14);
            AddParameter("Gain", ParameterTypes.Double, 1.4);
        }

        //private members
        private double _addToPeriod = 1.0;

    }
}