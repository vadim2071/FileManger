using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ConsoleFileManager
{
    //перечисление возможных команд
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
        static int CountPage; // счетчик выводимых строк в команде dir
        static int Page = 50; // сколько строк выводить за 1 раз при команде dir
        static void Main(string[] args)
        {
            //int Page = 20; // сколько строк выводить за 1 раз при команде dir
            Console.OutputEncoding = Encoding.UTF8;
            string FileConfig = "config.json"; // имя файла для хранения последнего каталога в котором работали
            string PathConfig = Directory.GetCurrentDirectory() + "\\"; //получаем путь откуда запустили программу, по нему будет сохраняться файл с данными
            string [] ConfigRead = { }; // данные текущего каталога после десериализации из файла конфигурации

            string CurentPath = Directory.GetCurrentDirectory(); // текущий каталог 
            string NewString = ""; // строка новой команды на выполнение введенной пользователем
            Command NewCommand = new(CommandName.error, "",""); // для распознанной команда с аргументами

            if (!File.Exists(FileConfig)) //проверка существования файла данных, если нет создаем и записываем в него текущий каталог
            {
                File.Create(FileConfig).Close();
                File.AppendAllText(FileConfig, JsonSerializer.Serialize(CurentPath));
            }

            ConfigRead = File.ReadAllLines(FileConfig); //читаем данные из файла конфигурации
            //проверяем существование сохраненного каталога, если он есть сохраняем его как текущий каталог иначе начинаем с рабочего каталога программы
            if (Directory.Exists(JsonSerializer.Deserialize<string>(ConfigRead[0]))) CurentPath = cdDir(JsonSerializer.Deserialize<string>(ConfigRead[0])); 
            //ListDir(CurentPath, 1); // вывод текущего дерева каталога без вывода вложенных каталогов

            while (NewCommand.Name != CommandName.quit)
            {
                Console.WriteLine(CurentPath); // вывод текущего каталога

                NewString = Console.ReadLine(); //получение новой команды
                Console.Clear();
                Console.WriteLine(CurentPath);
                Console.WriteLine(NewString);
                NewCommand = ParseCommand(NewString.ToLower()); // перевод строки в "маленький регистр", и вызов метода для разбора команды

                switch (NewCommand.Name)
                {
                    case CommandName.dir:
                        if (NewCommand.Arg1 == "") NewCommand.Arg1 = CurentPath; //если аргумента нет то выводим списсок катаолгов в текущем каталоге
                        CountPage = Page;
                        ListDir(NewCommand.Arg1, 2, false);
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
                        GetInfo(NewCommand.Arg1);
                        break;
                    case CommandName.mkdir:
                        MakeDir(CurentPath, NewCommand.Arg1);
                        break;
                    case CommandName.quit:
                        //записываем последний каталог
                        File.Create(PathConfig + FileConfig).Close();
                        File.AppendAllText(PathConfig + FileConfig, JsonSerializer.Serialize(CurentPath));
                        break;
                    case CommandName.error:
                        Console.WriteLine("ошибка, неправильная команда");
                        break;
                }


            }

            //метод получение информации по файлу/каталогу
            static void GetInfo(string Name) 
            {
                long Size;
                DateTime DateCreate;
                DateTime LastChange;
                string FileAtrReadOnly = "no";


                if (File.Exists(Name)) //проверка существования заданного файла
                {
                    FileInfo fileInfo = new FileInfo(Name);
                    Size = fileInfo.Length / 1024; //размер файла в KByte
                    DateCreate = fileInfo.CreationTime; // дата создания
                    LastChange = fileInfo.LastWriteTime; //дата последнего изменения
                    if (fileInfo.IsReadOnly) FileAtrReadOnly = "Yes";
                    Console.WriteLine("Файл {0} \n размер {1} KByte\n дата создания {2}\n дата последнего изменения {3}\n атрибут файла только для чтения - {4}", Name, Size, DateCreate, LastChange, FileAtrReadOnly);
                }
                else if (Directory.Exists(Name))
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(Name);
                    DateCreate = DirInfo.CreationTime;
                    LastChange = DirInfo.LastWriteTime;
                    Size = DirSize(Name)/1024;
                    Console.WriteLine("Каталог {0} \n размер {1} KByte \n дата создания {2}\n дата последнего изменения {3}", Name, Size, DateCreate, LastChange);
                }
            }


            //метод подсчета размера каталога
            static long DirSize(String path)
            {
                long Size = 0;
                DirectoryInfo DirPath = new DirectoryInfo(path);

                try
                {
                    DirectoryInfo[] DirList = DirPath.GetDirectories(); //получаем список каталогов в текущем каталоге
                    FileInfo[] FileList = DirPath.GetFiles(); // получаем списко файлов в текущем каталоге

                    foreach (FileInfo file in FileList) Size = Size + file.Length; //подсчет размера всех файлов в текущем каталоге
                    foreach (DirectoryInfo Dir in DirList) Size = Size + DirSize(Dir.FullName); //подсчет размера всех файлов для каждого катлога в текущем каталоге

                    return Size;
                }
                catch
                {
                    return Size;
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
                catch(UnauthorizedAccessException)
                {
                    Console.WriteLine("Ошибка! У вас нет прав на удаление этого объекта");
                }
                catch (IOException)
                {
                    Console.WriteLine("Ошибка! \n - Каталог является текущим рабочим каталогом приложения или Каталог не пустой.\n - Каталог доступен только для чтения или содержит файл, доступный только для чтения. \n Каталог используется другим процессом.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Ошибка! Что-то пошло не так: " + ex);
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

            //метод копирования файла / каталога
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


            // метод вывода списка каталогов/ PathName - родительский каталог, level - глубина вывода списка каталогов,
            // LastDir - признак что вызываем вывод список каталогов для последнего родительского каталога
            static void ListDir(string PathName, int level, bool LastDir) 
            {
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \
                string[] listDir = new string[1]; //для сохранения полученного списка каталогов
                int lenght = PathName.Length; // длина строки, содержащей путь к каталогу, список котрого выводим
                string LeftSpace = ""; // нужно когда выводится подкаталоги (второй уровень вложенности)

                try 
                {
                    listDir = Directory.GetDirectories(PathName); //создаем массив содержащий список каталогов в PathName
                }
                catch
                {
                    listDir[0] = PathName +  "Отказано в доступе"; //в случае ошибки чтения списка вложенных каталогов пишем - отказано в доступе
                }

                int AmountDir = listDir.Length; // количество строк в массиве = количество каталогов

                for (int i = 0; i < AmountDir; i++)
                {
                    if (level == 1 & !LastDir) LeftSpace = "\u2503 "; // если выводим подкаталоги, нужно сдвинуть их вывод
                    else if (level == 2) LeftSpace = ""; 
                    else LeftSpace = "  ";
                    //выводим элементы массива, обрезая родительские каталоги 
                    if (i != AmountDir - 1) Console.WriteLine(LeftSpace + "\u2523\u2578" + listDir[i].Substring(lenght)); //вывод с первого до предпоследнего элемента
                    else
                    {
                        Console.WriteLine(LeftSpace + "\u2517\u2578" + listDir[i].Substring(lenght)); //вывод последнего элемента
                        LastDir = true;
                    }
                    CountPage--; //вывели строчку уменьшили счетчик выведенных строк
                    if (CountPage == 0)
                    {
                        Console.WriteLine("для продолжения нажмите клавишу");
                        Console.ReadKey();
                        Console.Clear();
                        CountPage = Page;
                    }

                    if (level > 1) ListDir(listDir[i], level - 1, LastDir); // если требуется вывести подкаталог еще, то вызываем метод для вывода второго уровня каталогов
                }

                if (level == 2) // выводим список файлов если это не вложенные каталоги
                {
                    string[] listFile = Directory.GetFiles(PathName); //создаем массив содержащий список файлов в PathName
                    AmountDir = listFile.Length; // количество строк в массиве = количество файлов
                    for (int i = 0; i < AmountDir; i++) Console.WriteLine(LeftSpace + "  " + listFile[i].Substring(lenght));
                    CountPage--; //вывели строчку уменьшили счетчик выведенных строк

                    if (CountPage == 0)
                    {
                        Console.WriteLine("для продолжения нажмите клавишу");
                        Console.ReadKey();
                        Console.Clear();
                        CountPage = Page;
                    }
                }
            }
                
            //метод разбора полученной команды, на входе строка введенной команды, на выходе получаем команду, аргумент 1, аргумент 2
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
                                // если каталог существует, то его передаем, иначе возвращаем ошибку
                                if (Directory.Exists(CommandArray[1])) NewCommand.Arg1 = CommandArray[1]; 
                                else NewCommand.Name = CommandName.error;
                            }
                            break;

                        case "cd":
                            // если каталог существует и 1 аргумент, то его передаем, иначе возвращаем ошибку
                            if (Directory.Exists(CommandArray[1]) & CommandArray.Length == 2) 
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
                            if (CommandArray.Length == 2 & (File.Exists(CommandArray[1]) || Directory.Exists(CommandArray[1])))
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
