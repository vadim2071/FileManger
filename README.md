# FileManger
Console File Manager
Список команд:
quit - выйти из программы

dir disk_name:\dir_name	выводит содержимое указанного каталога, если путь не указан, то выводит список содержимого текущего каталога


mkdir disk_name:\dir_name\new_dir_name	создает каталог с именем new_dir_name в disk_name:\dir_name\

copy disk_name:\dir_name\sorce_file_name disk_name:\dir_name\target_file_name копирует файл sorce_file_name из disk_name:\dir_name\ в disk_name:\dir_name\target_file_name

copy disk_name:\dir_name\sorce_dir_name disk_name:\target_dir_name копирует катлог sorce_dir_name из disk_name:\dir_name\ в disk_name:\target_dir_name

del disk_name:\dir_name\file_name удаляет файл file_name из каталога disk_name:\dir_name\, если пути нет то удаляет из текущего каталога
del disk_name:\dir_name\del_dir_name удаляет каталог del_dir_name из каталога disk_name:\dir_name\, если пути нет то удаляет из текущей каталога

info disk_name:\dir_name\file_name or dir_name выводит информцию о каталоге или файле находящемся в disk_name:\dir_name\, если пути нет то ищется в текущем каталоге

file disk_name:\dir_name\file_name просмотр содержимого файла file_name

