using System;
using System.Text;

public class Kysy
{
    public static void Main()
    {
        string yhdistetty = KysySyote();

        Console.WriteLine($"Yhdistetyt syötteet: \"{yhdistetty}\"");
    }

    public static string KysySyote(string viesti = "Anna merkkijono (tyhjä syöte lopettaa): ")
    {
        StringBuilder sb = new StringBuilder();

        while (true)
        {
            Console.Write(viesti);
            string syote = Console.ReadLine();

            if (syote == "") // Jos syöte on tyhjä, lopeta silmukka
            {
                break;
            }

            sb.Append(syote); // Lisää syöte StringBuilderiin
        }

        return sb.ToString(); // Palauta yhdistetty merkkijono
    }
}