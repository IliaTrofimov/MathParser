# Math Parser
Парсер математических выражений позволяет приводить выражения к обратной польской нотации и затем вычислять их, подставляя числа в переменные.
Внутри функции Main есть поддержка некоторых команд для взаимодействия с программой:
1) -v переключает режим выполнения алгоритма по действиям
2) -c переключает чувствительность программы к регистру переменных
3) -e заставляет программу вычислять преобразованное в ОПН выражение
4) -setvalues записывает значения переменных
5) -values выводит значения переменных
6) ! выходит из программы
7) любой другой ввод будет расцениваться программой как выражение, которое нужно отпрарсить (и вычислить)
