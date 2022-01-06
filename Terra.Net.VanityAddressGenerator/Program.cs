using System.Text.RegularExpressions;
using Terra.Net.Crypto;
using Terra.Net.Crypto.Implemetations;

namespace Terra.Net.VanityAddress
{
    public static class Program
    {       
        static  Regex regex;
        static ulong count = 0;
        static ulong generated = 0;
        private static readonly Timer _timer = new Timer(PrintStats, null, TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(60));
        private static DateTime _date = DateTime.UtcNow;
        private static void PrintStats(object? state)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Generated {count} total and {generated} addreses. APS: {(int)(count / DateTime.UtcNow.Subtract(_date).TotalSeconds)}/s");
        }
        public static void Main(params string[] args)
        {    
            if(args.Length == 0)
            {
                regex = new Regex(@"(\w)\1{6,}");
            }
            else
            {
                regex = new Regex(args[0]);
            }
            While(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, () => generated < 10, b => Run());
        }
        private static void Run()
        {
            Interlocked.Increment(ref count);
            var mnm = new MnemonicKey();

            var address = Bech32.Encode("terra", mnm.RawAddress);

            if (regex.IsMatch(address))
            {
                Interlocked.Increment(ref generated);
                File.WriteAllText($"{address}.txt", mnm.Mnemonic);
            }
        }

        public static void While(ParallelOptions parallelOptions, Func<bool> condition,
    Action<ParallelLoopState> body)
        {
            Parallel.ForEach(Infinite(), parallelOptions, (ignored, loopState) =>
            {
                if (condition()) body(loopState);
                else loopState.Stop();
            });
        }

        private static IEnumerable<bool> Infinite()
        {
            while (true) yield return true;
        }
    }
}
