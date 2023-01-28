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
}
