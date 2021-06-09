# FileManger
Console File Manager
Список команд:
quit - выйти из программы,  сохраняется текущий каталог, количество строк страницы вывода команды dir

dir disk_name:\dir_name			выводит содержимое указанного каталога, если путь не указан, то выводит список содержимого текущего каталога
dir 					выводит список содержимого текущего каталога
dir -p					выводит количество строк страницы вывода команды dir
dir -p(int)				меняет	количество строк страницы вывода команды dir на значение (int)

mkdir disk_name:\dir_name\new_dir_name 	создает каталог с именем new_dir_name в disk_name:\dir_name\
mkdir new_dir_name 			создает каталог с именем new_dir_name в текущем каталоге

copy disk_name:\dir_name\sorce_file_name disk_name:\dir_name\target_file_name 	копирует файл sorce_file_name из disk_name:\dir_name\ в disk_name:\dir_name\target_file_name
copy disk_name:\dir_name\sorce_dir_name disk_name:\target_dir_name 		копирует каталог sorce_dir_name из disk_name:\dir_name\ в disk_name:\target_dir_name
copy sorce_file_name target_file_name						копирует файл в текущем каталоге
copy sorce_dir_name target_dir_name						копирует каталог в текущем каталоге

del disk_name:\dir_name\file_name 		удаляет файл file_name из каталога disk_name:\dir_name\
del file_name 					удаляет file_name из текущего каталога
del disk_name:\dir_name\del_dir_name 		удаляет каталог del_dir_name из каталога disk_name:\dir_name\
del del_dir_name				удаляет каталог del_dir_name из текущего каталога

info disk_name:\dir_name\file_name		выводит информцию о файле file_name находящемся в disk_name:\dir_name\
info file_name					выводит информцию о файле file_name в текущем каталоге
info disk_name:\dir_name\dir_name		выводит информцию о каталоге находящемся в disk_name:\dir_name\ в текущем каталоге
info dir_name					выводит информцию о каталоге dir_name в текущем каталоге

file disk_name:\dir_name\file_name 		просмотр содержимого файла file_name
file file_name

cd disk_name:\dir_name				смена каталога
cd dir_name					смена каталога на каталог в текущем каталоге



