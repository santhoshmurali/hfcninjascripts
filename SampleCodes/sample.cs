#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class HFCSimpleCPR : Indicator
	{
		private double varpp,varBottpmCentralPivot,varTopCentralPivot,varS1,varR1, varS2, varR2, varS3, varR3, varS4, varR4,h1,c1, l1;
		private SimpleFont errFont;
		private SimpleFont CPRPercentFont;
		private int weekDay;
		private double varUpprSpan,varLowSpan,varCprSpan;
		
		private Brush brushR2,brushR3,brushR4;
		private Brush brushS2,brushS3,brushS4;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Simple Central Pivot Range";
				Name										= "HFCSimpleCPR";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				Displacement								= 1;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				//Configure Brushes
				brushR2 = new SolidColorBrush(Color.FromArgb(190,243,114,44));
				brushR2.Freeze();

				brushR3 = new SolidColorBrush(Color.FromArgb(190,248,150,30));
				brushR3.Freeze();				
				
				brushR4 = new SolidColorBrush(Color.FromArgb(190,249,199,79));
				brushR4.Freeze();				

				brushS2 = new SolidColorBrush(Color.FromArgb(190,144,190,109));
				brushS2.Freeze();

				brushS3 = new SolidColorBrush(Color.FromArgb(190,67,190,139));
				brushS3.Freeze();				
				
				brushS4 = new SolidColorBrush(Color.FromArgb(190,87,117,144));
				brushS4.Freeze();	
				
				PSpan = true;
				
				AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Dot, "CprPP");
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 1), PlotStyle.Dot, "CprTopCentralPivot");
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 1), PlotStyle.Dot, "CprBottomCentralPivot");
				
				AddPlot(new Stroke(Brushes.Chartreuse, 1), PlotStyle.Hash, "CprS1");
				
				AddPlot(new Stroke(Brushes.OrangeRed, 1), PlotStyle.Hash, "CprR1");
				
				AddPlot(new Stroke(brushS2, 1), PlotStyle.Hash, "CprS2");
				
				AddPlot(new Stroke(brushR2, 1), PlotStyle.Hash, "CprR2");
								
				AddPlot(new Stroke(brushS3, 1), PlotStyle.Hash, "CprS3");
				
				AddPlot(new Stroke(brushR3, 1), PlotStyle.Hash, "CprR3");
								
				AddPlot(new Stroke(brushS4, 1), PlotStyle.Hash, "CprS4");
				
				AddPlot(new Stroke(brushR4, 1), PlotStyle.Hash, "CprR4");
				
				//Parameters for Setting Support and Resistance
				//R1S1
				pR1 = true;
				pS1 = true;
				
				//R2S2
				pR2 = false;
				pS2 = false;
				
				//R3S3
				pR3 = false;
				pS3 = false;
				
				//R4S4
				pR4 = false;
				pS4 = false;				

			}
			else if (State == State.Configure)
			{
			AddDataSeries(BarsPeriodType.Day,1);
			}
			
			else if (State == State.DataLoaded)
			{

				CPRPercentFont = new SimpleFont("Arial", 12) { Size = 12, Bold = true };
				if(BarsPeriod.BarsPeriodType != BarsPeriodType.Minute)
				{
					errFont = new SimpleFont("Courier New", 12) { Size = 30, Bold = true };
					Draw.TextFixed(this,"Error","Error : Select Only Minute Period Type for HFCSimpleCPR Range to Work!",TextPosition.BottomLeft,Brushes.Red,errFont,Brushes.Red,Brushes.Yellow,60);
					return;
				}
			}
			
		
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBars[1]<1)
				return;
			
			h1 = Highs[1][0];
			c1 = Closes[1][0];
			l1 = Lows[1][0];
			
			if(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
			{
			varpp = (h1+l1+c1)/3;
			varBottpmCentralPivot = (h1 + l1)/2;
			varTopCentralPivot = (varpp - varBottpmCentralPivot)+varpp;
			
			
			varS1 = 2 * varpp - h1;
			varR1 = 2 * varpp - l1;		
			
			//S2 = pp-(h-l)	
			varS2 = varpp - (h1 - l1);
			
			//R2 = pp + (h-l)
			varR2 = varpp + (h1 - l1);
				
			//S3 = S1 - (h - l)
			varS3 = varS1 - (h1 - l1);	
				
			//R3 = 	R1 + (h - l)
			varR3 = varR1 + (h1 - l1);
				
			//S4 = S3 - (S1 - S2)
			varS4 = varS3 - (varS1 - varS2);	
				
			//R4 = 	R4 + (r2 - r1)
			varR4 = varR3 + (varR2 - varR1);
				
				if (PSpan)
				{
					//Percentage Calclulation	
					varUpprSpan = Math.Round(((varR1 - varpp)/(varR1-varS1))*100,2);
					varLowSpan = Math.Round(((varpp - varS1)/(varR1-varS1))*100,2);
					varCprSpan = Math.Round((Math.Log(varTopCentralPivot/varBottpmCentralPivot)*100),2);
					if (varCprSpan < 0)
					{
						varCprSpan = varCprSpan*-1;
					}
					Draw.Text(this,Times[1][0].Date.ToString()+"U",varUpprSpan.ToString()+"%" + "\n",0,varR1+TickSize);
					Draw.Text(this,Times[1][0].Date.ToString()+"CPR",false,varCprSpan.ToString() + "%",0,varpp+TickSize,0,Brushes.White,CPRPercentFont,TextAlignment.Center,Brushes.Gray,Brushes.Crimson,50);
					Draw.Text(this,Times[1][0].Date.ToString()+"L","\n" + varLowSpan.ToString()+"%",0,varS1-TickSize);
				
					
				}

				
			CprPP[0] = varpp;
			CprTopCentralPivot[0] = varTopCentralPivot;
			CprBottomCentralPivot[0] = varBottpmCentralPivot;
			
			if (pS1)
			{
			CprS1[0] = varS1;
			}
			if (pR1)
			{
			CprR1[0] = varR1;
			}
			
			if (pS2)
			{
				CprS2[0] = varS2;
			}
			if (pR2)
			{
				CprR2[0] = varR2;
			}	
			
			if (pS3)
			{
				CprS3[0] = varS3;
			}
			if (pR3)
			{
				CprR3[0] = varR3;
			}
			
			if (pS4)
			{
				CprS4[0] = varS4;
			}
			if (pR4)
			{
				CprR4[0] = varR4;
			}
			
			}
		}

		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="Display Span %", Order=1, GroupName="Display Spans")]
		public bool PSpan
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="R1", Order=1, GroupName="Resistance Pivot Points")]
		public bool pR1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="R2", Order=2, GroupName="Resistance Pivot Points")]
		public bool pR2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="R3", Order=3, GroupName="Resistance Pivot Points")]
		public bool pR3
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="R4", Order=4, GroupName="Resistance Pivot Points")]
		public bool pR4
		{ get; set; }				
		
		[NinjaScriptProperty]
		[Display(Name="S1", Order=1, GroupName="Support Pivot Points")]
		public bool pS1
		{ get; set; }						

		[NinjaScriptProperty]
		[Display(Name="S2", Order=2, GroupName="Support Pivot Points")]
		public bool pS2
		{ get; set; }			

		[NinjaScriptProperty]
		[Display(Name="S3", Order=3, GroupName="Support Pivot Points")]
		public bool pS3
		{ get; set; }						

		[NinjaScriptProperty]
		[Display(Name="S4", Order=4, GroupName="Support Pivot Points")]
		public bool pS4
		{ get; set; }			
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprPP
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprTopCentralPivot
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprBottomCentralPivot
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprS1
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprR1
		{
			get { return Values[4]; }
		}
		

		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprS2
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprR2
		{
			get { return Values[6]; }
		}		
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprS3
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprR3
		{
			get { return Values[8]; }
		}				


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprS4
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CprR4
		{
			get { return Values[10]; }
		}						
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HFCSimpleCPR[] cacheHFCSimpleCPR;
		public HFCSimpleCPR HFCSimpleCPR(bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			return HFCSimpleCPR(Input, pSpan, pR1, pR2, pR3, pR4, pS1, pS2, pS3, pS4);
		}

		public HFCSimpleCPR HFCSimpleCPR(ISeries<double> input, bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			if (cacheHFCSimpleCPR != null)
				for (int idx = 0; idx < cacheHFCSimpleCPR.Length; idx++)
					if (cacheHFCSimpleCPR[idx] != null && cacheHFCSimpleCPR[idx].PSpan == pSpan && cacheHFCSimpleCPR[idx].pR1 == pR1 && cacheHFCSimpleCPR[idx].pR2 == pR2 && cacheHFCSimpleCPR[idx].pR3 == pR3 && cacheHFCSimpleCPR[idx].pR4 == pR4 && cacheHFCSimpleCPR[idx].pS1 == pS1 && cacheHFCSimpleCPR[idx].pS2 == pS2 && cacheHFCSimpleCPR[idx].pS3 == pS3 && cacheHFCSimpleCPR[idx].pS4 == pS4 && cacheHFCSimpleCPR[idx].EqualsInput(input))
						return cacheHFCSimpleCPR[idx];
			return CacheIndicator<HFCSimpleCPR>(new HFCSimpleCPR(){ PSpan = pSpan, pR1 = pR1, pR2 = pR2, pR3 = pR3, pR4 = pR4, pS1 = pS1, pS2 = pS2, pS3 = pS3, pS4 = pS4 }, input, ref cacheHFCSimpleCPR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HFCSimpleCPR HFCSimpleCPR(bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			return indicator.HFCSimpleCPR(Input, pSpan, pR1, pR2, pR3, pR4, pS1, pS2, pS3, pS4);
		}

		public Indicators.HFCSimpleCPR HFCSimpleCPR(ISeries<double> input , bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			return indicator.HFCSimpleCPR(input, pSpan, pR1, pR2, pR3, pR4, pS1, pS2, pS3, pS4);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HFCSimpleCPR HFCSimpleCPR(bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			return indicator.HFCSimpleCPR(Input, pSpan, pR1, pR2, pR3, pR4, pS1, pS2, pS3, pS4);
		}

		public Indicators.HFCSimpleCPR HFCSimpleCPR(ISeries<double> input , bool pSpan, bool pR1, bool pR2, bool pR3, bool pR4, bool pS1, bool pS2, bool pS3, bool pS4)
		{
			return indicator.HFCSimpleCPR(input, pSpan, pR1, pR2, pR3, pR4, pS1, pS2, pS3, pS4);
		}
	}
}

#endregion
