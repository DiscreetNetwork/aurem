namespace Aurem.Shared
{
    class Utils<T>
    {
        public static void Shuffle(List<T> nodes)
        {
            Random rng = new Random();
            int n = nodes.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = nodes[k];
                nodes[k] = nodes[n];
                nodes[n] = value;
            }
        }
    }

    class Dbg
    {
        /// <summary>
        /// A prints the value provided, along with a message.
        /// </summary>
        public static void A(string msg, Object value)
        {
            Console.WriteLine("{msg}: {value}");
        }
    }
}
