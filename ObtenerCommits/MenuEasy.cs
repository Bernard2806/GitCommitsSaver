using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObtenerCommits
{
    public static class MenuEasy
    {
        // Funcion de otro repositiorio de mi persona

        public static int MostrarMenu(string[] Opciones, string InitalText = "Seleccione una opción del menu:")
        {
            bool loop = true;
            int counter = 0;
            ConsoleKeyInfo Tecla;

            Console.CursorVisible = false; // Con esto hacemos que el cursor no se muestre en consola

            while (loop)
            {
                Console.Clear();
                Console.WriteLine(InitalText + System.Environment.NewLine);
                string SeleccionActual = string.Empty;
                int Destacado = 0;

                Array.ForEach(Opciones, element =>
                {
                    if (Destacado == counter)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine(" > " + element + " < ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        SeleccionActual = element;
                    }
                    else
                    {
                        Console.WriteLine(element);
                    }
                    Destacado++;
                });

                Tecla = Console.ReadKey(true);
                if (Tecla.Key == ConsoleKey.Enter)
                {
                    loop = false;
                    break;
                }
                switch (Tecla.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (counter < Opciones.Length - 1) counter++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (counter > 0) counter--;
                        break;
                    default:
                        break;
                }
            }
            return counter;
        }

    }
}
