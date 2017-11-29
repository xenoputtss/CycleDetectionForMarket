using System;
using System.Collections.Generic;
using System.Linq;

namespace Find_Short_In_Markets_POC
{
    class Program
    {
        public static double BitfinexFee = .00200;

        static void Main(string[] args)
        {
            var pairs = new List<TradePair>()
            {
                new TradePair(){Left=Currency.BTC,Right=Currency.USD, Value = 9992, IsLeftToRight=true},
                new TradePair(){Left=Currency.LTC,Right=Currency.BTC, Value = 0.009869, IsLeftToRight=true},
                new TradePair(){Left=Currency.LTC,Right=Currency.USD, Value = 98.41, IsLeftToRight=true}
            };

            FindIncreaseingTrade(pairs);

            Console.ReadKey();
        }

        public static void FindIncreaseingTrade(List<TradePair> pairs)
        {
            var cycles = FindCycles(pairs);

            foreach (var c in cycles)
            {
                Console.WriteLine();
                Console.Write($"{(c.First().IsLeftToRight ? c.First().Left : c.First().Right)} ");
                foreach (var tp in c)
                    Console.Write(tp);

                var cycleIncrease = CalculateIncreaseFromCycle(c);
                Console.Write(cycleIncrease);
            }

            //Calculate left to right ratio

            //Return results if something is greater than 1
        }

        private static double CalculateIncreaseFromCycle(List<TradePair> c)
        {
            double increase = 1;
            foreach (var trade in c)
            {
                if (trade.IsLeftToRight)
                    increase *= trade.Value;
                else
                    increase *= 1.0 / trade.Value;

                //Apply Fee
                increase *= 1 - BitfinexFee;
            }
            return increase;
        }

        private static List<List<TradePair>> FindCycles(List<TradePair> pairs)
        {
            var cycles = new List<List<TradePair>>();
            //Find all Cycles
            foreach (var start in EnumUtil.GetValues<Currency>())
            {
                //Console.WriteLine(start);
                //Foreach pair where start is part of the trade
                foreach (var trade1 in pairs.Where(p => p.Left == start || p.Right == start))
                {
                    trade1.IsLeftToRight = trade1.Left == start;
                    //Console.WriteLine($" {trade1}");

                    //foreach pair where the other part of trade1 is part of another pair, while start is also not part of the pair
                    var search1 = trade1.IsLeftToRight ? trade1.Right : trade1.Left;
                    foreach (var trade2 in pairs.Where(p => (p.Left == search1 || p.Right == search1) && p.Right != start && p.Left != start))
                    {
                        trade2.IsLeftToRight = trade2.Left == search1;
                        //Console.WriteLine($"  {trade2}");

                        //foreach pair where trade2.right is the left and start is the right
                        var search2 = trade2.IsLeftToRight ? trade2.Right : trade2.Left;
                        foreach (var trade3 in pairs.Where(p => (p.Left == search2 || p.Right == search2) && (p.Left == start || p.Right == start)))
                        {
                            trade3.IsLeftToRight = trade3.Left == search2;
                            //Console.WriteLine($"   {trade3}");
                            //Here we should have, if any, all 3 pair cycles
                            //Console.WriteLine($"{trade1}{trade2}{trade3}");
                            cycles.Add(new List<TradePair>() { trade1.Clone(), trade2.Clone(), trade3.Clone() });
                        }
                    }
                }
            }

            return cycles;
        }
    }

    public class TradePair
    {
        public Currency Left { get; set; }
        public Currency Right { get; set; }

        //value represents how many of Right you would get for 1 of Left
        public double Value { get; set; }
        public bool IsLeftToRight { get; set; }

        public override string ToString()
        {
            return $"{Left}{Right}" + (IsLeftToRight ? "=>" : "<=");
        }
        public TradePair Clone()
        {
            return new TradePair()
            {
                Left = this.Left,
                Right = this.Right,
                Value = this.Value,
                IsLeftToRight = this.IsLeftToRight
            };
        }
    }

    public enum Currency
    {
        BTC,
        LTC,
        ETH,
        USD
    }

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}
