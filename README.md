# FileManger
Console File Manager
Список команд:

dir disk_name:\dir_name	выводит содержимое указанной директории, если путь не указа, то выводит список содержимого текущей директории
подкоманды - 	next - следующая страница
		prev - предыдущая страница
		stop - остановить вывод

mkdir disk_name:\dir_name\new_dir_name	создает директорию с именем new_dir_name в disk_name:\dir_name\

copy disk_name:\dir_name\sorce_file_name disk_name:\dir_name\target_file_name	копирует файл sorce_file_name из disk_name:\dir_name\ в disk_name:\dir_name\target_file_name

copy disk_name:\dir_name\sorce_dir_name disk_name:\target_dir_name копирует директорию sorce_dir_name из disk_name:\dir_name\ в disk_name:\target_dir_name

del disk_name:\dir_name\file_name удаляет файл file_name из директории disk_name:\dir_name\, если пути нет то удаляет из текущей директории
del disk_name:\dir_name\del_dir_name удаляет директорию del_dir_name из директории disk_name:\dir_name\, если пути нет то удаляет из текущей директории

info disk_name:\dir_name\file_name or dir_name выводит информцию о директории или файле находящемся в disk_name:\dir_name\, если пути нет то ищется в текущем директории

file disk_name:\dir_name\file_name просмотр содержимого файла file_name

