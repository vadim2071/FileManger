using System;
using System.IO;

namespace ConsoleFileManager
{
    class Program
    {
        //перечисление возможных команд
        [Flags]
        enum CommandList
        {
            dir,
            mkdir,
            copy,
            del,
            info,
            file,
            quit
        };
        static void Main(string[] args)
        {
            string CurentPath = Directory.GetCurrentDirectory(); // текущая директория (не забыть считать ее из файла конфигурации)
            string NewCommand = ""; // строка новой команды на выполнение
            CommandList comm; //рспознанная команда




            while (true)
            {
                // вывод дерева каталогов
                NewCommand = Console.ReadLine();

                // парсинг, разбор команды
                if (!ParseCommand(NewCommand)) Console.WriteLine("ошибка");
                comm = CommandList.copy;


                switch (comm)
                {
                    case CommandList.dir:
                        break;
                    case CommandList.copy:
                        break;
                    case CommandList.del:
                        break;
                    case CommandList.file:
                        break;
                    case CommandList.info:
                        break;
                    case CommandList.mkdir:
                        break;
                    case CommandList.quit:
                        break;
                }


            }


            static bool ParseCommand(string CommandString)
            {
                //разбираем введенную команду на подстроки используя знак пробела как разделитель, исключая дублирование пробелов
                string[] CommandArray = CommandString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (CommandArray.Length > 3) return false; // проверка на количество подстрок, их не может быть болше 3х (команда, аргумент 1, аргумент 2
                for (int i = 0; i < CommandArray.Length; i++)
                {
                    Console.WriteLine(CommandArray[i]);
                }
                return true;
            }

            static void DirTree(string Path)
            {

            }

            static string GetFileInfo(string Path, string FileName) //метод получение информации по файлу
            {
                if (File.Exists(Path + FileName)) //проверка существования заданного файла
                {
                    string FileDate = File.GetCreationTime(Path + FileName).ToShortDateString(); //дата создания файла
                    FileAttributes atributes = File.GetAttributes(Path + FileName); //атрибуты файла
                    int FileSize = (Path + FileName).Length; //размер файла

                    return "  ";
                }
                else
                {
                    return "False";
                }

            }


            
        }
        class FileInfo
        {

        }
    }
}
