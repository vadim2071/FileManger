using System;

namespace ConsoleFileManager
{
    class Program
    {
        //перечисление возможных команд
        enum CommandList
        {
            dir,
            mkdir,
            copy,
            del,
            info,
            file
        };
        static void Main(string[] args)
        {
            // dir, mkdir, copy, del, info, file
            //массив возможных команд
            string[] command = new string[6] {"dir", "mkdir", "copy", "del", "info", "file"};

            while (true)
            {
                Console.WriteLine("Введите команду");
                string input = Console.ReadLine();
                if (!ParseCommand(input)) Console.WriteLine("ошибка");
            }
            
            
            static bool ParseCommand(string CommandString)
            {
                //разбираем введенную команду на подстроки используя как разделитель знак пробела, исключая дублирование пробелов
                string[] CommandArray = CommandString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (CommandArray.Length > 3) return false; // проверка на количество подстрок, их не может быть болше 3х (команда, аргумент 1, аргумент 2
                for (int i= 0; i < CommandArray.Length; i++)
                {
                    Console.WriteLine(CommandArray[i]);
                }
                return true;
                
            }
            class command 
        }
    }
}
