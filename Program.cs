using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using NLog;

namespace ConsoleApp1
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); //инициализация логгера для создания лог-файлов
            
        public int Count { get; } //функция для подсчёта количества элементов

        public static char[] Dividers = { ' ', ',', '.', '!', '?', '"', ';', ':', '[', ']', '(', ')', '\n', '\r', '\t' }; //задание массива с разделителями

        static string RemoveTags(string Text) //функция, убирающая из текста теги HTML (например, &nbsp) 
        {
            var FinalText = Regex.Replace(Text, @"<[^>]+>|&nbsp;|&mdash;", " ").Trim();

            return FinalText;
        }

        static string GetText(string route) //функция, загружающая файл из директории route и добавляющая его тексовую часть в новую переменную
        {
            string OriginalText = "";
            HtmlDocument Doc = new HtmlDocument();
            Doc.Load(route);

            IEnumerable<HtmlNode> pars = Doc.DocumentNode.Descendants("p");
            foreach (HtmlNode p in pars)
            {
                OriginalText += p.InnerText;
            }

            return OriginalText;
        }

        static string[] SplitWords(string ModifieldText) //функция, создающая новый массив, в который помещаются все слова из ранее созданного текстового файла
        {
            string[] words = ModifieldText.Split(Dividers, StringSplitOptions.RemoveEmptyEntries);
            return words;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в приложение.");

            logger.Info("Программа начала работу."); //при запуске программы создаётся запись в лог-файле

            string OriginalText = "", route, ModifieldText;
            int TakeWords;
            bool flag;

            do //проверка на правильность ввода расположения файла
            {
                Console.WriteLine("\nВведите путь к необходимому HTML-файлу: ");
                route = Console.ReadLine();
                flag = File.Exists(route);
                if (!flag)
                {
                    Console.WriteLine("Неверный путь файла. Попробуйте ещё раз.");
                    logger.Error("Введено неверное значение параметра."); //при ошибке ввода в лог-файле будет создана запись
                }

                else if (GetText(route) == "")
                {
                    Console.WriteLine("В выбранном файле нет текстовых элементов, удовлетворяющих требованиям. Выберите другой файл."); //если указан верный путь, однако в файле нет текстовых элементов, программа попросит повторный выбор файла
                    flag = false;
                }

            }
            while (!flag);

            OriginalText = GetText(route);
            ModifieldText = RemoveTags(OriginalText);
            string[] words = SplitWords(ModifieldText);

            var uniqWords = words.Select(q => q.Trim(Dividers)).Distinct();
            var result = new Dictionary<string, int>();
            
            foreach (var word in uniqWords)
            {
                result.Add(word, words.Count(q => q.Equals(word)));
            }
            
            do //проверка на правильность ввода количества выводимых значений
            {
                Console.WriteLine($"\nВведите число слов для вывода (всего в массиве {result.Count} слов(а)): ");
                flag = Int32.TryParse(Console.ReadLine(), out TakeWords);
                if (!flag)
                {
                    Console.WriteLine("Введёное значение не является целым числом. Попробуйте ещё раз.");
                    logger.Error("Введено неверное значение параметра.");
                }
            }
            while (!flag);

            if (TakeWords > result.Count) //в случае ввода слишком большого значения будет выведен весь массив слов
            {
                Console.WriteLine($"Введено слишком большое число. Будет выведен весь список слов ({result.Count} слов(а)).");
                TakeWords = result.Count;
            }
                
            Console.WriteLine($"\nБудет выведено {TakeWords} слов(а).\n");

            result = result.OrderByDescending(q => q.Value).ToList().Take(TakeWords).ToDictionary(key => key.Key, value => value.Value); //сортровка списка слов и подготовка к выводу

            foreach (var word in result) //вывод слов и количества их повторений
            {
                Console.WriteLine($"'{word.Key}' — встречается {word.Value} раз(а).");
            }

            logger.Info("Программа завершила работу."); //при завршении работы приложения создаётся запись в лог-файле
            Console.ReadKey(true);
        }
    }
}
