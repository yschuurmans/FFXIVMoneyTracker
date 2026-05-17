using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVMoneyTracker.Models;

namespace FFXIVMoneyTracker.Windows
{
    public class MoneyGraph : Window
    {
        private int daysShown;

        public MoneyGraph(Plugin plugin, PluginUI pluginUI) : base(plugin, pluginUI)
        {
            daysShown = Math.Max(plugin.Configuration.GraphDaysShown, 1);
        }

        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Money graph", ref this.visible))
            {
                if (plugin.CurrentCharacter == null)
                {
                    ImGui.Text("No character loaded");
                }
                else
                {
                    daysShown = Math.Max(daysShown, 1);
                    int configuredDaysShown = daysShown;
                    if (ImGui.InputInt("Amount of days shown", ref configuredDaysShown, 5, 30, default, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        daysShown = Math.Max(configuredDaysShown, 1);
                        plugin.Configuration.GraphDaysShown = daysShown;
                        plugin.Configuration.Save();
                    }

                    daysShown = Math.Max(daysShown, 1);

                    var transactions = plugin.CurrentCharacter.Transactions.ToArray();
                    long average = transactions.Length > 0
                        ? (long)transactions.Average(x => x.Total)
                        : 0;

                    string unitName = "gil";
                    float divisionFactor = 1;
                    if (average > 1000 && average < 1000000)
                    {
                        divisionFactor = 1000;
                        unitName = "thousand gil";
                    }
                    if (average > 1000000)
                    {
                        divisionFactor = 1000000;
                        unitName = "million gil";
                    }

                    Vector2 childScale = new Vector2(ImGui.GetWindowWidth() - 15, ImGui.GetWindowHeight() - 100);
                    DateTime now = DateTime.Now;
                    DateTime cutoff = DateTime.Now.AddDays(-daysShown);
                    var filteredTransactions = transactions
                        .Where(x => x.TimeStamp > cutoff)
                        .ToArray();

                    float[] graphData = filteredTransactions
                        .Select(x => (float)Math.Round(x.Total / divisionFactor, 3))
                        .ToArray();

                    if (graphData.Length > 1 && childScale.X > 0 && childScale.Y > 0)
                    {
                        var axisScale = GetNiceAxisScale(graphData.Max());
                        var dateAxisScale = GetDateAxisScale(cutoff, now);

                        DrawGraph(childScale, filteredTransactions, graphData, axisScale, dateAxisScale, unitName, divisionFactor);
                    }
                    else if (transactions.Length == 0)
                    {
                        ImGui.Text("No transaction history is available yet.");
                    }
                    else
                    {
                        ImGui.Text("Not enough data points or invalid graph size to show a graph. Please wait until more data is collected.");
                    }
                }



            ImGui.End();

            }
        }

        private static void DrawGraph(Vector2 graphSize, IReadOnlyList<MoneyTransaction> transactions, IReadOnlyList<float> graphData, AxisScale axisScale, DateAxisScale dateAxisScale, string unitName, float divisionFactor)
        {
            ImGui.BeginChild("##MoneyGraphPlot", graphSize, false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoSavedSettings);

            float leftMargin = 72f;
            float bottomMargin = 28f;
            float topMargin = 8f;
            float rightMargin = 8f;

            Vector2 plotOrigin = ImGui.GetCursorScreenPos() + new Vector2(leftMargin, topMargin);
            Vector2 plotSize = new Vector2(
                Math.Max(graphSize.X - leftMargin - rightMargin, 1f),
                Math.Max(graphSize.Y - bottomMargin - topMargin, 1f));
            var drawList = ImGui.GetWindowDrawList();

            Vector2 plotEnd = plotOrigin + plotSize;
            uint borderColor = ImGui.GetColorU32(ImGuiCol.Border);
            uint guideColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
            drawList.AddRect(plotOrigin, plotEnd, borderColor);

            float graphMin = 0f;
            float valueRange = Math.Max(axisScale.UpperBound - graphMin, 1f);
            string axisUnitSuffix = GetAxisUnitSuffix(divisionFactor);

            for (int tickIndex = 0; tickIndex <= axisScale.DivisionCount; tickIndex++)
            {
                float tickValue = axisScale.TickStep * tickIndex;
                float tickRatio = tickValue / valueRange;
                float y = plotEnd.Y - (plotSize.Y * tickRatio);

                drawList.AddLine(new Vector2(plotOrigin.X, y), new Vector2(plotEnd.X, y), guideColor, 1f);

                string amountLabel = FormatAxisLabel(tickValue, axisUnitSuffix);
                drawList.AddText(new Vector2(plotOrigin.X - leftMargin + 4f, y - 8f), guideColor, amountLabel);
            }

            foreach (DateTime tickDate in dateAxisScale.TickDates)
            {
                float xRatio = GetTimeRatio(tickDate, dateAxisScale.Start, dateAxisScale.End);
                float x = plotOrigin.X + (plotSize.X * xRatio);

                drawList.AddLine(new Vector2(x, plotOrigin.Y), new Vector2(x, plotEnd.Y), guideColor, 1f);

                string dateLabel = FormatDateAxisLabel(tickDate, dateAxisScale.LabelFormat);
                drawList.AddText(new Vector2(x - 10f, plotEnd.Y + 4f), guideColor, dateLabel);
            }

            Vector2? previousPoint = null;
            for (int index = 0; index < graphData.Count; index++)
            {
                float normalizedValue = (graphData[index] - graphMin) / valueRange;
                float xRatio = GetTimeRatio(transactions[index].TimeStamp, dateAxisScale.Start, dateAxisScale.End);
                float x = plotOrigin.X + (plotSize.X * xRatio);
                float y = plotEnd.Y - (normalizedValue * plotSize.Y);
                Vector2 point = new Vector2(x, y);

                if (previousPoint.HasValue)
                {
                    drawList.AddLine(previousPoint.Value, point, ImGui.GetColorU32(ImGuiCol.PlotHistogram), 2f);
                }

                float hitSize = 10f;
                Vector2 hitTopLeft = point - new Vector2(hitSize * 0.5f, hitSize * 0.5f);

                ImGui.SetCursorScreenPos(hitTopLeft);
                ImGui.InvisibleButton($"##graphPoint{index}", new Vector2(hitSize, hitSize));

                drawList.AddCircleFilled(point, 3.5f, ImGui.GetColorU32(ImGuiCol.Text));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"{transactions[index].Total:#,##0} gil");
                    ImGui.Text(transactions[index].TimeStamp.ToString("dd/MM/yyyy HH:mm:ss"));
                    ImGui.EndTooltip();
                }

                previousPoint = point;
            }

            drawList.AddText(plotOrigin + new Vector2(4f, 4f), guideColor, unitName);
            ImGui.EndChild();
        }

        private readonly struct AxisScale
        {
            public AxisScale(float upperBound, float tickStep, int divisionCount)
            {
                UpperBound = upperBound;
                TickStep = tickStep;
                DivisionCount = divisionCount;
            }

            public float UpperBound { get; }
            public float TickStep { get; }
            public int DivisionCount { get; }
        }

        private readonly struct DateAxisScale
        {
            public DateAxisScale(DateTime start, DateTime end, DateTime[] tickDates, string labelFormat)
            {
                Start = start;
                End = end;
                TickDates = tickDates;
                LabelFormat = labelFormat;
            }

            public DateTime Start { get; }
            public DateTime End { get; }
            public DateTime[] TickDates { get; }
            public string LabelFormat { get; }
        }

        private static AxisScale GetNiceAxisScale(float maxValue)
        {
            if (maxValue <= 0f)
            {
                return new AxisScale(1f, 1f, 1);
            }

            float targetStep = maxValue / 5f;
            float exponent = (float)Math.Floor(Math.Log10(targetStep));
            float baseStep = (float)Math.Pow(10, exponent);

            float[] fractions = { 1f, 2f, 2.5f, 4f, 5f, 10f };
            AxisScale? bestScale = null;
            float bestScore = float.MaxValue;

            for (int exponentOffset = -1; exponentOffset <= 1; exponentOffset++)
            {
                float currentBaseStep = baseStep * (float)Math.Pow(10, exponentOffset);
                foreach (float fraction in fractions)
                {
                    float tickStep = fraction * currentBaseStep;
                    if (tickStep <= 0f)
                    {
                        continue;
                    }

                    int divisionCount = Math.Max(1, (int)Math.Ceiling(maxValue / tickStep));
                    float upperBound = tickStep * divisionCount;

                    float score = Math.Abs(divisionCount - 5);
                    if (divisionCount < 4 || divisionCount > 6)
                    {
                        score += 10f;
                    }

                    score += (upperBound - maxValue) / Math.Max(maxValue, 1f);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestScale = new AxisScale(upperBound, tickStep, divisionCount);
                    }
                }
            }

            return bestScale ?? new AxisScale(maxValue, maxValue / 5f, 5);
        }

        private static DateAxisScale GetDateAxisScale(DateTime start, DateTime end)
        {
            if (end <= start)
            {
                end = start.AddDays(1);
            }

            int weekDivisions = (int)Math.Ceiling((end - start).TotalDays / 7d);
            if (weekDivisions < 10)
            {
                var tickDates = new List<DateTime>();
                DateTime tickDate = start;

                while (tickDate <= end)
                {
                    tickDates.Add(tickDate);
                    tickDate = tickDate.AddDays(7);
                }

                if (tickDates.Count == 0 || tickDates[^1] < end)
                {
                    tickDates.Add(end);
                }

                return new DateAxisScale(start, end, tickDates.ToArray(), "dd/MM");
            }

            var monthlyTicks = new List<DateTime>();
            DateTime monthlyTick = new DateTime(start.Year, start.Month, 1);
            if (monthlyTick < start)
            {
                monthlyTick = monthlyTick.AddMonths(1);
            }

            while (monthlyTick <= end)
            {
                monthlyTicks.Add(monthlyTick);
                monthlyTick = monthlyTick.AddMonths(1);
            }

            if (monthlyTicks.Count == 0)
            {
                monthlyTicks.Add(start);
            }

            return new DateAxisScale(start, end, monthlyTicks.ToArray(), "MMM yy");
        }

        private static float GetTimeRatio(DateTime value, DateTime start, DateTime end)
        {
            double totalSeconds = (end - start).TotalSeconds;
            if (totalSeconds <= 0d)
            {
                return 0f;
            }

            double elapsedSeconds = (value - start).TotalSeconds;
            return (float)Math.Clamp(elapsedSeconds / totalSeconds, 0d, 1d);
        }

        private static string FormatDateAxisLabel(DateTime value, string labelFormat)
        {
            return value.ToString(labelFormat);
        }

        private static string GetAxisUnitSuffix(float divisionFactor)
        {
            if (divisionFactor >= 1_000_000f)
            {
                return "mil";
            }

            if (divisionFactor >= 1_000f)
            {
                return "k";
            }

            return "gil";
        }

        private static string FormatAxisLabel(float value, string unitSuffix)
        {
            if (unitSuffix == "gil")
            {
                return $"{value:0} {unitSuffix}";
            }

            return $"{value:0} {unitSuffix}";
        }
    }
}
