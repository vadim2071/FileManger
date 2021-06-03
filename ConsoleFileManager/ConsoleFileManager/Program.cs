using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ConsoleFileManager
{
    //перечисление возможных команд
    [Flags]
    enum CommandName
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
        public CommandName Name { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }

        //конструктор
        public Command(CommandName name, string arg1, string arg2)
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
            string FileConfig = "config.json"; // имя файла для хранения последнего каталога в котором работали
            string PathConfig = Directory.GetCurrentDirectory() + "\\"; //получаем путь откуда запустили программу, по нему будет сохраняться файл с данными
            string [] ConfigRead = { }; // данные текущего каталога после десериализации
            string ConfigSave; //данные о каталоге для сериализации

            Console.OutputEncoding = Encoding.UTF8;
            string CurentPath = Directory.GetCurrentDirectory(); // текущий каталог (не забыть считать его из файла конфигурации)
            string NewString = ""; // строка новой команды на выполнение
            Command NewCommand = new(CommandName.error, "",""); // для распознанной команда с аргументами

            if (!File.Exists(FileConfig)) //проверка существования файла данных, если нет создаем и записываем в него текущий каталог
            {
                File.Create(FileConfig).Close();
                ConfigSave = JsonSerializer.Serialize(CurentPath);
                File.AppendAllText(FileConfig, ConfigSave);
            }

            ConfigRead = File.ReadAllLines(FileConfig); //читаем данные из файла конфигурации
            //проверяем существование сохраненного каталога, если он есть сохраняем его как текущий каталог иначе начинаем с рабочего каталога программы
            if (Directory.Exists(JsonSerializer.Deserialize<string>(ConfigRead[0]))) CurentPath = cdDir(JsonSerializer.Deserialize<string>(ConfigRead[0])); 

            ListDir(CurentPath, 1); // вывод текущего дерева каталога

            while (NewCommand.Name != CommandName.quit)
            {
                Console.WriteLine(CurentPath); // вывод текущего каталога

                NewString = Console.ReadLine(); //получение новой команды
                NewCommand = ParseCommand(NewString.ToLower()); // перевод строки в "маленький регистр", парсинг/разбор команды

                switch (NewCommand.Name)
                {
                    case CommandName.dir:
                        if (NewCommand.Arg1 == "") NewCommand.Arg1 = CurentPath;
                        ListDir(NewCommand.Arg1, 2);
                        break;
                    case CommandName.cd:
                        CurentPath = cdDir(NewCommand.Arg1);
                        break;
                    case CommandName.copy:
                        FileCopy(NewCommand.Arg1, NewCommand.Arg2);
                        break;
                    case CommandName.del:
                        Delete(NewCommand.Arg1, NewCommand.Arg2);
                        break;
                    case CommandName.file:
                        FilePrint(NewCommand.Arg1);
                        break;
                    case CommandName.info:
                        GetFileInfo(NewCommand.Arg1);
                        break;
                    case CommandName.mkdir:
                        MakeDir(CurentPath, NewCommand.Arg1);
                        break;
                    case CommandName.quit:
                        //записываем последний каталог
                        File.Create(PathConfig + FileConfig).Close();
                        ConfigSave = JsonSerializer.Serialize(CurentPath);
                        File.AppendAllText(PathConfig + FileConfig, ConfigSave);
                        break;
                    case CommandName.error:
                        Console.WriteLine("ошибка, неправильная команда");
                        break;
                }


            }

            //метод получение информации по файлу
            static void GetFileInfo(string FileName) 
            {
                long FileSize;
                DateTime FileDateCreate;
                DateTime FileLastChange;
                FileInfo fileInfo = new FileInfo(FileName);
                string FileAtrReadOnly = "yes";


                if (File.Exists(FileName)) //проверка существования заданного файла
                {
                    FileSize = fileInfo.Length / 1024; //размер файла в KByte
                    FileDateCreate = fileInfo.CreationTime; // дата создания
                    FileLastChange = fileInfo.LastWriteTime; //дата последнего изменения
                    if (fileInfo.IsReadOnly) FileAtrReadOnly = "no";
                    Console.WriteLine("Файл {0} \n размер {1} KByte\n дата создания {2}\n дата последнего изменения {3}\n только для чтения {4}", FileName, FileSize, FileDateCreate, FileLastChange, FileAtrReadOnly);

                }
            }

            //метод удаления файла / каталога
            static void Delete (string DelElement, string TypeElement)
            {
                try
                {
                    if (TypeElement == "file") File.Delete(DelElement);
                    else Directory.Delete(DelElement, true);
                }
                catch
                {
                    Console.WriteLine("Ошибка удаления");
                }

            }

            //метод создания каталога
            static void MakeDir(string Path, string NameDir)
            {
                try
                {
                    Directory.CreateDirectory(Path + @"\" + NameDir);
                }
                catch
                {
                    Console.WriteLine("оказано в доступе");
                }
                
            }

            //метод просмотра содержимого файлов
            static void FilePrint(string Path)
            {
                StreamReader file = new(Path);
                try
                {
                    do
                    {
                        Console.WriteLine(file.ReadLine());
                    }
                    while (file.Peek() != -1);
                }
                catch
                {
                    Console.WriteLine("Файл пустой");
                }
                file.Close();
            }

            //метод копирования файла
            static void FileCopy(string PathFrom, string PathTo)
            {
                try
                {
                    File.Copy(PathFrom, PathTo, false);
                }
                catch
                {
                    Console.WriteLine("Ошибка");
                }
                
            }


            // метод смены каталога
            static string cdDir(string PathName)
            {
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть
                Directory.SetCurrentDirectory(PathName);
                return PathName;
            }


            // метод вывода списка каталогов
            static void ListDir(string PathName, int level) //PathName - родительский каталог, level - глубина вывода списка каталогов 
            {
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \
                string[] listDir = new string [1]; //для сохранения полученного списка каталогов
                
                try
                {
                    listDir = Directory.GetDirectories(PathName); //создаем массив содержащий список каталогов в PathName
                    int lenght = PathName.Length; // длина строки, содержащей путь к каталогу, список котрого выводим 
                    int NumList = listDir.Length; // количество строк в массиве = количество каталогов

                    string space = "";
                    if (level == 1) space = "\u2503 ";

                    for (int i = 0; i < NumList; i++)
                    {
                        if (i == NumList - 1) //Выводим список каталогов без пути к нему (вычитаем PathName) в заивисимости от того последний ли элемент в массиве  меняем знак перд выводом
                        {
                            Console.WriteLine(space + "\u2517\u2578" + listDir[i].Substring(lenght));
                            if (level > 1) ListDir(listDir[i], level - 1); //если не вывели всю глубину вложения, продолжаем выводить
                        }
                        else
                        {
                            Console.WriteLine(space + "\u2523\u2578" + listDir[i].Substring(lenght));
                            if (level > 1) ListDir(listDir[i], level - 1);
                        }
                    }
                    
                    if (level == 2) // выводим списко файлов если это не вложенный каатлоги
                    {
                        string[] listFile = Directory.GetFiles(PathName); //создаем массив содержащий список файлов в PathName
                        NumList = listFile.Length; // количество строк в массиве = количество файлов
                        for (int i = 0; i < NumList; i++) Console.WriteLine(space + "  " + listFile[i].Substring(lenght));
                    }
                }
                catch
                {
                    listDir[0] = "Отказано в доступе"; //в случае ошибки пишем - отказано в доступе
                    Console.WriteLine("\u2503 \u2517\u2578" + listDir[0]);
                }
            }


            //метод парсинга полученной команды, на входе строка введенной команды, на выходе получаем команду, аргумент 1, аргумент 2
            static Command ParseCommand(string CommandString)
            {
                Command NewCommand = new(CommandName.error, "", ""); //инициализация класса

                //разбираем введенную команду на подстроки используя знак пробела как разделитель, исключая дублирование пробелов
                string[] CommandArray = CommandString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (CommandArray.Length > 3) return NewCommand; // проверка на количество подстрок, их не может быть больше 3х (команда, аргумент 1, аргумент 2

                try
                {
                    switch (CommandArray[0])
                    {
                        case "dir":
                            NewCommand.Name = CommandName.dir;
                            if (CommandArray.Length == 2) //если присутствует аргумент
                            {
                                if (Directory.Exists(CommandArray[1])) NewCommand.Arg1 = CommandArray[1]; // если каталог существует, то его передаем, иначе возвращаем ошибку
                                else NewCommand.Name = CommandName.error;
                            }
                            break;

                        case "cd":
                            if (Directory.Exists(CommandArray[1]) & CommandArray.Length == 2) // если каталог существует и 1 аргумент, то его передаем, иначе возвращаем ошибку
                            {
                                NewCommand.Arg1 = CommandArray[1];
                                NewCommand.Name = CommandName.cd;
                            }
                            break;

                        case "copy":
                            if (CommandArray.Length == 3) //проверка что получено 1 команда и 2 аргумента
                            {
                                if (File.Exists(CommandArray[1])) //проверка существования исходного файла
                                {
                                    NewCommand.Arg1 = CommandArray[1];
                                    NewCommand.Arg2 = CommandArray[2];
                                    NewCommand.Name = CommandName.copy;
                                }
                                else if (File.Exists(Directory.GetCurrentDirectory() + @"\" + CommandArray[1]))  // первый аргумент может быть именем файла в текущей директории
                                {
                                    NewCommand.Arg1 = Directory.GetCurrentDirectory() + @"\" + CommandArray[1];
                                    NewCommand.Arg2 = CommandArray[2];
                                    NewCommand.Name = CommandName.copy;
                                }
                            }
                            break;

                        case "del":
                            if (CommandArray.Length == 2)
                            {
                                NewCommand.Name = CommandName.del;
                                NewCommand.Arg1 = CommandArray[1];
                                if (File.Exists(NewCommand.Arg1)) NewCommand.Arg2 = "file";
                                else NewCommand.Arg2 = "dir";
                            }
                            break;

                        case "file":
                            if (CommandArray.Length == 2 & File.Exists(CommandArray[1]))
                            {
                                NewCommand.Name = CommandName.file;
                                NewCommand.Arg1 = Directory.GetCurrentDirectory() + @"\" + CommandArray[1];
                            }
                            break;

                        case "info":
                            if (CommandArray.Length == 2 & File.Exists(CommandArray[1]))
                            {
                                NewCommand.Name = CommandName.info;
                                NewCommand.Arg1 = CommandArray[1];
                            }
                            break;

                        case "mkdir":
                            if (CommandArray.Length == 2)
                            {
                                NewCommand.Name = CommandName.mkdir;
                                NewCommand.Arg1 = CommandArray[1];
                            }
                            break;

                        case "quit":
                            NewCommand.Name = CommandName.quit;
                            break;

                        default:
                            NewCommand.Name = CommandName.error;
                            break;
                    }

                }
                catch
                {
                    NewCommand.Name = CommandName.error;
                }

                return NewCommand;

            }
        }
    }
}
