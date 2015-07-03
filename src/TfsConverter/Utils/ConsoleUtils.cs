using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TfsGource.Utils
{
    public class ConsoleUtils
    {
        public static string ReadPassword()
        {
            Stack<string> passbits = new Stack<string>();
            //keep reading
            for (ConsoleKeyInfo cki = Console.ReadKey(true); cki.Key != ConsoleKey.Enter; cki = Console.ReadKey(true))
            {
                if (cki.Key == ConsoleKey.Backspace)
                {
                    //rollback the cursor and write a space so it looks backspaced to the user
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    passbits.Pop();
                }
                else
                {
                    Console.Write("*");
                    passbits.Push(cki.KeyChar.ToString());
                }
            }
            string[] pass = passbits.ToArray();
            Array.Reverse(pass);
            return string.Join(string.Empty, pass);
        }

        public static bool ReadBoolInput(string message, bool defaultValue)
        {
            Console.WriteLine(message);
            string input = Console.ReadLine();
            if (!String.IsNullOrEmpty(input))
            {
                if (input.ToLower().StartsWith("t") || input.ToLower().StartsWith("y"))
                {
                    return true;
                }
                return false;
            }
            return defaultValue;
        }
    }
}
