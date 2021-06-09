using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Configuration;
using System.Xml;
using Microsoft.Extensions.Configuration;
using System.Resources;
using System.Reflection;

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
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; //Для корректного вывода псевдографики


            int Page = 30; // сколько строк выводить за 1 раз при команде dir
            string FileConfig = "config.json"; // имя файла для хранения последнего каталога в котором работали
            string PathConfig = Directory.GetCurrentDirectory() + "\\"; //получаем путь откуда запустили программу, по нему будет сохраняться файл с данными
            string [] ConfigRead = { }; // данные текущего каталога после десериализации из файла конфигурации


            string[] DirList = { }; //массив каталогов, подкаталогов и файлов полсе команды dir
            string CurentPath = Directory.GetCurrentDirectory(); // текущий каталог 
            string NewString = ""; // строка новой команды на выполнение введенной пользователем
            Command NewCommand = new(CommandName.error, "",""); // для распознанной команда с аргументами

            var config
            // работа с файлом конфигурции
            Configuration roaming = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            var FileMap = new ExeConfigurationFileMap { ExeConfigFilename = roaming.FilePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(FileMap, ConfigurationUserLevel.None);


            if (!File.Exists(FileConfig)) //проверка существования файла данных, если нет создаем и записываем в него текущий каталог
            {
                File.Create(FileConfig).Close();
                File.AppendAllText(FileConfig, JsonSerializer.Serialize(CurentPath));
            }

            ConfigRead = File.ReadAllLines(FileConfig); //читаем данные из файла конфигурации
            //проверяем существование сохраненного каталога, если он есть сохраняем его как текущий каталог иначе начинаем с рабочего каталога программы
            if (Directory.Exists(JsonSerializer.Deserialize<string>(ConfigRead[0]))) CurentPath = JsonSerializer.Deserialize<string>(ConfigRead[0]);
            
            cdDir(CurentPath);


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
                        Copy(NewCommand.Arg1, NewCommand.Arg2);
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
                int length;
                if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\"; // в строке содержащей путь последний символ должен быть \ (необходимо если вводим имя диска без \)

                try
                {
                    CurentDirList = Directory.GetDirectories(PathName); //получаем список каталогов в каталоге PathName
                }
                catch(UnauthorizedAccessException)
                {
                    Array.Resize(ref DirList, 1); //каталог недоступен, возвращаем список из 1 строки "Отказ в доступе"
                    DirList[0] = "Отказано в доступе";
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    Array.Resize(ref DirList, 1); //каталог недоступен, возвращаем список из 1 строки "Отказ в доступе"
                    DirList[0] = "такой каталог не существует";
                    return;
                }
                catch (Exception ex)
                {
                    Array.Resize(ref DirList, 1); //каталог недоступен, возвращаем список из 1 строки "Отказ в доступе"
                    DirList[0] = "что-то пошло не так " + ex;
                    return;
                }

                CurentFileList = Directory.GetFiles(PathName); //получаем список файлов в каталоге PathName

                length = DirList.Length;

                for (int i = 0; i < CurentDirList.Length; i++) //записываем список каталогов
                {
                    length++;

                    Array.Resize(ref DirList, length);

                    if(i < CurentDirList.Length - 1) DirList[length - 1] = "\u2523\u2578" + Path.GetFileName(CurentDirList[i]);
                    else DirList[length - 1] = "\u2517\u2578" + Path.GetFileName(CurentDirList[i]);

                    try
                    {
                        Level2DirList = Directory.GetDirectories(CurentDirList[i]);
                        for (int c = 0; c < Level2DirList.Length; c++) //записываем список подкаталогов каждого каталога
                        {
                            length++;
                            Array.Resize(ref DirList, length);

                            string space;
                            space = i == (CurentDirList.Length - 1) ? " ": "\u2503";
                            // в зависмости от положения подкаталога в списе (последний или нет) рисуем впереди разные символы псевдографики  
                            if (c < Level2DirList.Length - 1) DirList[length - 1] = space + " \u2523\u2578" + Level2DirList[c];
                            else DirList[length - 1] = space + " \u2517\u2578" + Level2DirList[c];
                        }
                    }
                    catch
                    {
                        length++;
                        Array.Resize(ref DirList, length);

                        string space;
                        space = i == (CurentDirList.Length - 1) ? " " : "\u2503";
                        DirList[length - 1] = space + " \u2517\u2578Отказано в доступе";
                    }
                }

                for (int i = 0; i < CurentFileList.Length; i++) // записываем список файлов 
                {
                    length++;
                    Array.Resize(ref DirList, length);
                    DirList[length - 1] = "  " + Path.GetFileName(CurentFileList[i]);
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
                    for (int p = 0; p < CountPage & c < Page*CountPage; p++) //выводим страницы по очереди
                    {
                        Console.Clear();
                        for (int i = 0; i < Page; i++) //выводим все элементы страницы
                        {
                            Console.WriteLine(DirList[(p * Page) + i]);
                        }

                        Console.WriteLine("Страница {0} из {1}, листать - стрелка вверх/вниз, выход - q", p + 1, CountPage);
                        do
                        {
                            key = Console.ReadKey(true);
                            if(key.Key == ConsoleKey.UpArrow)   // если нажата клавиша вверх
                            {                                   // уменьшаем счетчик страниц на 1
                                p = p == 0 || p == 1 ? -1 : (p - 2);       // если это первая или вторая страница, то -1 (счетчик в цикле увеличит на 1)
                                c = p == 0 || p == 1 ? 0 : (c - Page);    // счетчик строк уменьшаем на размер страницы если эта не первая страница
                                break;
                            }else if (key.Key == ConsoleKey.DownArrow)  // если нажата клавиша вниз
                            {                                           // 
                                //p++;                                    // увеличиваем счетчик страниц на 1
                                c = c + Page;                           // счетчик строк увеличиваем на размер страницы
                                break;
                            }else if(key.Key == ConsoleKey.Q)   // если нажата клавиша Q
                            {
                                c = lenght;                     // считаем что дальше ничего смотреть не будем
                            }
                        }
                        while (key.Key != ConsoleKey.Q);
                    }

                    if (c < lenght) Console.WriteLine(DirList[c]); // выводим (если не нажали q) последние строки массива, количесвто которых меньше страницы
                }
            }


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

                                                                
            static void GetInfo(string Name) //метод получение информации по файлу/каталогу
            {
                long Size;
                DateTime DateCreate;
                DateTime LastChange;
                string FileAtrReadOnly = "no";


                if (File.Exists(Name))                          //проверка существования заданного файла
                {
                    FileInfo fileInfo = new FileInfo(Name);
                    Size = fileInfo.Length / 1024;              //размер файла в KByte
                    DateCreate = fileInfo.CreationTime;         // дата создания
                    LastChange = fileInfo.LastWriteTime;        //дата последнего изменения
                    if (fileInfo.IsReadOnly) FileAtrReadOnly = "Yes";
                    Console.WriteLine("Файл {0} \n размер {1} KByte\n дата создания {2}\n дата последнего изменения {3}\n атрибут файла только для чтения - {4}", Name, Size, DateCreate, LastChange, FileAtrReadOnly);
                }
                else if (Directory.Exists(Name))                //проверка существования каталога
                {
                    DirectoryInfo DirInfo = new DirectoryInfo(Name);
                    DateCreate = DirInfo.CreationTime;          // дата создания
                    LastChange = DirInfo.LastWriteTime;         //дата последнего изменения
                    Size = DirSize(Name) / 1024;                //размер каталога в KByte
                    Console.WriteLine("Каталог {0} \n размер {1} KByte \n дата создания {2}\n дата последнего изменения {3}", Name, Size, DateCreate, LastChange);
                }
                else Console.WriteLine("Ошибка! Объект не найден");
            }

                                                                
            static long DirSize(String path) //метод подсчета размера каталога
            {
                long Size = 0;
                DirectoryInfo DirPath = new DirectoryInfo(path);

                try
                {
                    DirectoryInfo[] DirList = DirPath.GetDirectories(); //получаем список каталогов в текущем каталоге
                    FileInfo[] FileList = DirPath.GetFiles();           // получаем список файлов в текущем каталоге

                    foreach (FileInfo file in FileList) Size = Size + file.Length;              //подсчет размера всех файлов в текущем каталоге
                    foreach (DirectoryInfo Dir in DirList) Size = Size + DirSize(Dir.FullName); //подсчет размера всех файлов для каждого катлога в текущем каталоге

                    return Size;
                }
                catch
                {
                    return Size;
                }
                
            }

 
            static void Delete (string DelElement)  //метод удаления файла / каталога
            {
                                                                //если второй символ не ':' (используется в обозначени диска),
                                                                //предполагаем что DelElement содержит имя удаляемого элемента в текущем каталоге
                if (DelElement[1] != ':') DelElement = Path.Combine(Directory.GetCurrentDirectory(), DelElement); 

                try
                {
                    if (File.Exists(DelElement))
                    {
                        File.Delete(DelElement);
                        return;
                    }else if (Directory.Exists(DelElement))
                    {
                        Directory.Delete(DelElement, true);
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

               
            static void MakeDir(string NewDir)  //метод создания каталога
            {
                try
                {
                                                                //если аргумент не содержит знак : (используется при обозначении пути к диску)
                                                                //предполагаем что NewDir это имя нового каталога в текущем каталоге
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

                                      
            static void FileInfo(string Path)   //метод просмотра содержимого файлов
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


            static void Copy(string PathFrom, string PathTo)    //метод копирования файла / каталога
            {
                if (PathFrom == PathTo)
                {
                    Console.WriteLine("Ошибка! Нельзя копировать файл или каталог в самого себя");
                    return;
                }

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

                        
            static void DirCopy(string PathFrom, string PathTo) // Метод копирования каталога
            {
                DirectoryInfo DirPathSource = new DirectoryInfo(PathFrom);
                DirectoryInfo DirPathDestination = new DirectoryInfo(PathTo);
                DirectoryInfo[] DirList = DirPathSource.GetDirectories();       //получаем список каталогов в копируемом каталоге
                FileInfo[] FileList = DirPathSource.GetFiles();                 // получаем список файлов в копируемом каталоге
                
                try
                {
                    Directory.CreateDirectory(PathTo);          // создание целевого каталога
                    foreach (FileInfo file in FileList) file.CopyTo(Path.Combine(DirPathDestination.FullName, file.Name));              //копируем все файлы
                    foreach (DirectoryInfo Dir in DirList) DirCopy(Dir.FullName, Path.Combine(DirPathDestination.FullName, Dir.Name)); //создаем все существующией каталоги в целевом каталоге
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
            }

                    
            static void cdDir(string PathName)  // метод смены каталога
            {
                try
                {
                    if (PathName.Substring(PathName.Length - 1) != @"\") PathName += @"\";  // в строке содержащей путь последний символ должен быть \
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

            /*// метод вывода списка каталогов сразу в консоль с рекурсией  без массива без пейджинга  (версия 1)/ PathName - родительский каталог, level - глубина вывода списка каталогов,
            // LastDir - признак что вызываем вывод список каталогов для последнего родительского каталога
            необходимо
            //static int CountPage; // счетчик выводимых строк в команде dir
            //static int Page = 50; // сколько строк выводить за 1 раз при команде dir
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
        }
    }
}
