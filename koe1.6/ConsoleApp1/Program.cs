private static void Main()
{
    Console.WriteLine(RajoitettuSumma(new int[] { 1, -2, 3, 9 }, 4));
    // Pitäisi tulostaa 2
}

private static int RajoitettuSumma(int[] taulukko, int raja)
{
    int summa = 0;
    foreach (int t in taulukko)
    {
        if (t != raja) summa += t;
    }
    return summa;
}