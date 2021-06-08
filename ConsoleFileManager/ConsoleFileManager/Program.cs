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
        //static int CountPage; // счетчик выводимых строк в команде dir
        //static int Page = 50; // сколько строк выводить за 1 раз при команде dir
        static void Main(string[] args)
        {
            int Page = 50; // сколько строк выводить за 1 раз при команде dir
            Console.OutputEncoding = Encoding.UTF8; //Для корректного вывода псевдографики
            string FileConfig = "config.json"; // имя файла для хранения последнего каталога в котором работали
            string PathConfig = Directory.GetCurrentDirectory() + "\\"; //получаем путь откуда запустили программу, по нему будет сохраняться файл с данными
            string [] ConfigRead = { }; // данные текущего каталога после десериализации из файла конфигурации

            string[] DirList = { }; //массив каталогов, подкаталогов и файлов полсе команды dir

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
            if (Directory.Exists(JsonSerializer.Deserialize<string>(ConfigRead[0]))) CurentPath = JsonSerializer.Deserialize<string>(ConfigRead[0]);
            cdDir(CurentPath);
            //ListDir(CurentPath, 1); // вывод текущего дерева каталога без вывода вложенных каталогов

            while (NewCommand.Name != CommandName.quit)
            {
                Console.WriteLine(CurentPath); // вывод текущего каталога

                NewString = Console.ReadLine(); //получение новой команды
                Console.Clear();
                Console.WriteLine(CurentPath);
                Console.WriteLine(NewString);
                NewCommand = ParseCommand(NewString); // разбираем введенную строку команды

                switch (NewCommand.Name)
                {
                    case CommandName.dir:
                        if (NewCommand.Arg1 == "") NewCommand.Arg1 = CurentPath; //если аргумента нет то выводим список каталогов в текущем каталоге
                        //CountPage = Page;
                        //ListDir(NewCommand.Arg1, 2, false);
                        Array.Resize(ref DirList, 0); // очищаем массив от предыдущих результатов
                        Dir(NewCommand.Arg1, ref DirList);
                        DirPrint(ref DirList, Page);
                        break;
                    case CommandName.cd:
                        cdDir(NewCommand.Arg1);
                        CurentPath = Directory.GetCurrentDirectory();
                        break;
                    case CommandName.copy:
                        FileCopy(NewCommand.Arg1, NewCommand.Arg2);
                        break;
                    case CommandName.del:
                        Delete(NewCommand.Arg1);
                        break;
                    case CommandName.file:
                        FileInfo(NewCommand.Arg1);
                        break;
                    case CommandName.info:
                        GetInfo(NewCommand.Arg1);
                        break;
                    case CommandName.mkdir:
                        MakeDir(NewCommand.Arg1);
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

            //метод получения списка каталогов, подкаталогов и файлов в каталоге - версия 2
            static void Dir(String PathName, ref String[] DirList)
            {
                string[] CurentDirList = { }; //для сохранения полученного списка каталогов
                string[] CurentFileList = { }; //для сохранения полученного списка файлов в текущем каталоге
                string[] Level2DirList = { }; //для хранения списка подкаталогов
                int Lengt;
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \ (необходимо если вводим имя диска без \)

                CurentDirList = Directory.GetDirectories(PathName); //получаем список каталогов в каталоге PathName
                CurentFileList = Directory.GetFiles(PathName); //получаем список файлов в каталоге PathName

                Lengt = DirList.Length + 1;

                for (int i = 0; i < CurentDirList.Length; i++) //записываем список каталогов
                {
                    Array.Resize(ref DirList, Lengt + i);

                    Lengt++;

                    DirList[Lengt - 1] = CurentDirList[i];

                    Level2DirList = Directory.GetDirectories(CurentDirList[i]);

                    for (int c = 0; c < Level2DirList.Length; c++) //записываем список подкаталогов
                    {
                        Lengt++;
                        Array.Resize(ref DirList, Lengt + i);
                        DirList[Lengt + i - 1] = Level2DirList[c];
                    }
                }

                for (int i = 0; i < CurentFileList.Length; i++)
                {
                    Lengt++;
                    Array.Resize(ref DirList, Lengt - 1);
                    DirList[Lengt - 1] = CurentFileList[i];
                }
            }

            // метод вывода списка каталогов на экран
            static void DirPrint(ref String[] DirList, int Page)
            {
                ConsoleKeyInfo key;
                int lenght = DirList.Length;
                int CountPage = lenght / Page;

                for(int c = 0; c < lenght; c++) //выводим все элементы массива
                {
                    for (int p = 0; p < CountPage; p++) //выводим все страницы
                    {
                        for (int i = 0; i < Page & p < lenght; i++) //выводим все элементы страницы
                        {
                            Console.WriteLine(DirList[(p * Page) + i]);
                        }

                        Console.WriteLine("Страница {0} из {1}, листать - стрелка вверх/вниз, выход - q", p + 1, Page);
                        do
                        {
                            key = Console.ReadKey();
                            if (key.Key == ConsoleKey.UpArrow) p = p == 0 ? 0 : (p - 1);
                            if (key.Key == ConsoleKey.DownArrow) p = p == Page ? Page : (p + 1);
                        }
                        while (key.Key != ConsoleKey.Q);
                    }
                }
            }


            /*// метод вывода списка каталогов (версия 1)/ PathName - родительский каталог, level - глубина вывода списка каталогов,
            // LastDir - признак что вызываем вывод список каталогов для последнего родительского каталога
            static void ListDir(string PathName, int level, bool LastDir)
            {
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \ (необходимо если вводим имя диска без \)

                string[] listDir = new string[1]; //для сохранения полученного списка каталогов
                int lenght = PathName.Length; // длина строки, содержащей путь к каталогу, список котрого выводим
                string LeftSpace = ""; // нужно когда выводится подкаталоги (второй уровень вложенности)

                try
                {
                    listDir = Directory.GetDirectories(PathName); //создаем массив содержащий список каталогов в PathName
                }
                catch
                {
                    listDir[0] = PathName + "Отказано в доступе"; //в случае ошибки чтения списка вложенных каталогов пишем - отказано в доступе
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
            }*/

            //метод разбора полученной команды, на входе строка введенной команды, на выходе получаем команду, аргумент 1, аргумент 2
            static Command ParseCommand(string CommandString)
            {
                Command NewCommand = new(CommandName.error, "", ""); //инициализация класса
                //разбираем введенную команду на подстроки используя знак пробела как разделитель, исключая дублирование пробелов
                string[] CommandArray = CommandString.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // проверка на количество аргументов, их не может быть больше 3х (команда, аргумент 1, аргумент 2) или без команды
                if (CommandArray.Length > 3 || CommandArray.Length == 0) return NewCommand;
                else if (CommandArray.Length == 2) NewCommand.Arg1 = CommandArray[1];
                else if (CommandArray.Length == 3)
                {
                    NewCommand.Arg1 = CommandArray[1];
                    NewCommand.Arg2 = CommandArray[2];
                }

                switch (CommandArray[0])
                {
                    case "dir":
                        NewCommand.Name = CommandName.dir;
                        break;

                    case "cd":
                        NewCommand.Name = CommandName.cd;
                        break;

                    case "copy":
                        NewCommand.Name = CommandName.copy;
                        break;

                    case "del":
                        NewCommand.Name = CommandName.del;
                        break;

                    case "file":
                        NewCommand.Name = CommandName.file;
                        break;

                    case "info":
                        NewCommand.Name = CommandName.info;
                        break;

                    case "mkdir":
                        NewCommand.Name = CommandName.mkdir;
                        break;

                    case "quit":
                        NewCommand.Name = CommandName.quit;
                        break;

                    default:
                        NewCommand.Name = CommandName.error;
                        break;
                }
                return NewCommand;
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
                    Size = DirSize(Name) / 1024;
                    Console.WriteLine("Каталог {0} \n размер {1} KByte \n дата создания {2}\n дата последнего изменения {3}", Name, Size, DateCreate, LastChange);
                }
                else Console.WriteLine("Ошибка! Объект не найден");
            }

            //метод подсчета размера каталога
            static long DirSize(String path)
            {
                long Size = 0;
                DirectoryInfo DirPath = new DirectoryInfo(path);

                try
                {
                    DirectoryInfo[] DirList = DirPath.GetDirectories(); //получаем список каталогов в текущем каталоге
                    FileInfo[] FileList = DirPath.GetFiles(); // получаем список файлов в текущем каталоге

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
            static void Delete (string DelElement)
            {
                //если второй символ не ':' (используется в обозначени диска), предполагаем что DelElement содержит имя удаляемого элемента в текущем каталоге
                if (DelElement[1] != ':') DelElement = Path.Combine(Directory.GetCurrentDirectory(), DelElement); 

                try
                {
                    if (File.Exists(DelElement))
                    {
                        File.Delete(DelElement);
                        return;
                    }
                }
                catch(UnauthorizedAccessException)
                {
                    Console.WriteLine("Ошибка! У вас нет прав на удаление этого объекта");
                }
                catch (IOException)
                {
                    Console.WriteLine("Ошибка! Файл используется другим процессом или файл не найден");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Ошибка в аргументе");
                }
                catch (NotSupportedException)
                {
                    Console.WriteLine("параметр задан в недопустимом формате");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка! Что-то пошло не так: " + ex);
                }

                try
                {
                    Directory.Delete(DelElement);
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Ошибка! У вас нет прав на удаление этого объекта");
                }
                catch (IOException)
                {
                    Console.WriteLine("Ошибка! В каталоге файл используется другим процессом или каталог не найден \nили каталог доступен только для чтения или содержит файл только для чтения");
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Ошибка в аргументе");
                }
                catch (NotSupportedException)
                {
                    Console.WriteLine("параметр задан в недопустимом формате");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка! Что-то пошло не так: " + ex);
                }
            }

            //метод создания каталога
            static void MakeDir(string NewDir)
            {
                try
                {
                    //если аргумент не содержит знак : (используется при обозначении пути к диску) предполагаем что NewDir это имя нового каталога в текущем каталоге
                    if (NewDir[1] != ':') NewDir = Path.Combine(Directory.GetCurrentDirectory(), NewDir); 
                    Directory.CreateDirectory(NewDir);
                }
                catch(IOException)
                {
                    Console.WriteLine("Ошибка! Каталог, заданный параметром path, является файлом или недопустимый путь");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Ошибка! отсутствует необходимое разрешение");
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("Ошибка! не задан новый католог");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Что-то пошло не так {0}", ex);
                }

            }

            //метод просмотра содержимого файлов
            static void FileInfo(string Path)
            {
                if (File.Exists(Path))
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
                else Console.WriteLine("файл {0} не найден", Path);
                
            }

            //метод копирования файла / каталога
            static void FileCopy(string PathFrom, string PathTo)
            {
                if (File.Exists(PathFrom))
                {
                    try
                    {
                        File.Copy(PathFrom, PathTo, true);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.WriteLine("Каталог не найден");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Что-то пошло не так " + ex);
                    }
                }
                else if (Directory.Exists(PathFrom))
                {
                    DirCopy(PathFrom, PathTo);
                }
                else Console.WriteLine("Ошибка! Неверно указан источник копирования");
            }

            // Метод копирования каталога
            static void DirCopy(string PathFrom, string PathTo)
            {
                DirectoryInfo DirPathSource = new DirectoryInfo(PathFrom);
                DirectoryInfo DirPathDestination = new DirectoryInfo(PathTo);
                DirectoryInfo[] DirList = DirPathSource.GetDirectories(); //получаем список каталогов в копируемом каталоге
                FileInfo[] FileList = DirPathSource.GetFiles(); // получаем список файлов в копируемом каталоге
                if (PathFrom == PathTo)
                {
                    Console.WriteLine("Ошибка! Нельзя копировать каталог в самого себя");
                    return;
                }
                try
                {
                    Directory.CreateDirectory(PathTo); // создание целевого каталога
                }
                catch(IOException)
                {
                    Console.WriteLine("Ошибка! каталог в который копируется является файлом или недопустимый путь");
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Ошибка! У вас нет прав для копирования");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Что-то пошло не так" + ex);
                    return;
                }

                foreach (FileInfo file in FileList) file.CopyTo(Path.Combine(DirPathDestination.FullName, file.Name)); //копируем все файлы
                foreach (DirectoryInfo Dir in DirList) DirCopy(Dir.FullName, Path.Combine(DirPathDestination.FullName, Dir.Name)); //создаем все существующией каталоги в целевом каталоге
            }

            // метод смены каталога
            static void cdDir(string PathName)
            {
                try
                {
                    if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \
                    Directory.SetCurrentDirectory(PathName);
                }
                catch(FileNotFoundException)
                {
                    Console.WriteLine("Ошибка! путь не найден");
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("Ошибка! каталог не найден");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Что-то пошло не так" + ex);
                }
            }

        }
    }
}
