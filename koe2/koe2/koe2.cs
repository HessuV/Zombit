using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace koe2;

using System;

public class Kirjaimet
{

    public static void Main()
    {
        string[] rivit = { "Kissa kävelee Alajärvenkaduilla. Se ihmetteleemaailman menoa." };
        int kirjaimia = MontakoKirjainta(rivit, 'a');
        Console.WriteLine(kirjaimia);

    }

    public static int MontakoKirjainta(string[] rivit, char etsittavaKirjain)
    {
        int maara = 0;
        foreach (string rivi in rivit)
        {
            foreach (char kirjain in rivi)
            {
                if (Char.ToLower(kirjain) == (etsittavaKirjain))
                {
                    maara++;
                }
            }
            
        }

        return maara;
        
    }
}