using System;
using System.IO;

namespace ConsoleFileManager
{
    //перечисление возможных команд
    [Flags]
    enum CommandList
    {
        dir = 1,
        mkdir = 2,
        copy = 3,
        del = 4,
        info = 5,
        file = 6,
        quit = 7,
        error = 8,
        cd = 9
    };
    class Command
    {
        public CommandList Name { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }

        //конструктор
        public Command(CommandList name, string arg1, string arg2)
        {
            Name = name;
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            string CurentPath = Directory.GetCurrentDirectory(); // текущий каталог (не забыть считать его из файла конфигурации)
            string NewString = ""; // строка новой команды на выполнение
            Command NewCommand = new Command(CommandList.error, "",""); // для распознанной команда с аргументами

            while (NewCommand.Name != CommandList.quit)
            {
                // вывод дерева каталогов
                Console.WriteLine(CurentPath);
                
                NewString = Console.ReadLine(); //получение новой команды

                NewCommand = ParseCommand(NewString.ToLower()); // перевод строки в "маленький регистр", парсинг/разбор команды

                switch (NewCommand.Name)
                {
                    case CommandList.dir:
                        if (NewCommand.Arg1 == "") NewCommand.Arg1 = CurentPath;
                        ListDir(NewCommand.Arg1, 2);
                        break;
                    case CommandList.cd:
                        Console.WriteLine("cd смена директории");
                        break;
                    case CommandList.copy:
                        Console.WriteLine("copy копирование");
                        break;
                    case CommandList.del:
                        Console.WriteLine("del удаление");
                        break;
                    case CommandList.file:
                        Console.WriteLine("file посмотреть содержимое файла");
                        break;
                    case CommandList.info:
                        Console.WriteLine("info посмотреть содержимое файла");
                        break;
                    case CommandList.mkdir:
                        Console.WriteLine("mkdir создать каталог");
                        break;
                    case CommandList.quit:
                        break;
                    case CommandList.error:
                        Console.WriteLine("ошибка, неправильная команда");
                        break;
                }


            }

            // метод вывода списка каталогов
            static void ListDir(string PathName, int level) //PathName - родительский каталог, level - глубина вывода списка каталогов 
            {
                if (PathName.Substring(PathName.Length - 1) != "\\") PathName = PathName + "\\"; // в строке содержащей путь последний символ должен быть \
                string[] listDir = Directory.GetDirectories(PathName);
                int lenght = PathName.Length;
                for (int i = 0; i < listDir.Length; i++)
                {
                    Console.WriteLine(listDir[i].Substring(lenght));
                }
            }

            //метод парсинга полученной команды, на вохде строка введенной команды, на выходе получаем команду, аргумент 1, аргумент 2
            static Command ParseCommand(string CommandString)
            {
                Command NewCommand = new Command(CommandList.error, "", ""); //инициализация класса

                //разбираем введенную команду на подстроки используя знак пробела как разделитель, исключая дублирование пробелов
                string[] CommandArray = CommandString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (CommandArray.Length > 3) // проверка на количество подстрок, их не может быть больше 3х (команда, аргумент 1, аргумент 2
                {
                    NewCommand.Name = CommandList.error;
                    return NewCommand;
                }

                try
                {
                    switch (CommandArray[0])
                    {
                        case "dir":
                            NewCommand.Name = CommandList.dir;
                            if (CommandArray.Length > 1) //если присутствует аргумент
                            {
                                if (Directory.Exists(CommandArray[1])) // если каталог существует, то его передаем, иначе возвращаем ошибку
                                {
                                    NewCommand.Arg1 = CommandArray[1];
                                }
                                else
                                {
                                    NewCommand.Name = CommandList.error;
                                }
                            }
                            break;

                        case "cd":
                            NewCommand.Name = CommandList.cd;
                            NewCommand.Arg1 = CommandArray[1];
                            if (CommandArray.Length > 2) NewCommand.Name = CommandList.error;
                            break;
                        case "copy":
                            NewCommand.Name = CommandList.copy;
                            NewCommand.Arg1 = CommandArray[1];
                            NewCommand.Arg2 = CommandArray[2];
                            break;
                        case "del":
                            NewCommand.Name = CommandList.del;
                            NewCommand.Arg1 = CommandArray[1];
                            if (CommandArray.Length > 2) NewCommand.Name = CommandList.error;
                            break;
                        case "file":
                            NewCommand.Name = CommandList.file;
                            NewCommand.Arg1 = CommandArray[1];
                            if (CommandArray.Length > 2) NewCommand.Name = CommandList.error;
                            break;
                        case "info":
                            NewCommand.Name = CommandList.info;
                            NewCommand.Arg1 = CommandArray[1];
                            if (CommandArray.Length > 2) NewCommand.Name = CommandList.error;
                            break;
                        case "mkdir":
                            NewCommand.Name = CommandList.mkdir;
                            NewCommand.Arg1 = CommandArray[1];
                            if (CommandArray.Length > 2) NewCommand.Name = CommandList.error;
                            break;
                        case "quit":
                            NewCommand.Name = CommandList.quit;
                            break;
                        default:
                            NewCommand.Name = CommandList.error;
                            break;
                    }

                }
                catch
                {
                    NewCommand.Name = CommandList.error;
                }

                return NewCommand;

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
    }
}
