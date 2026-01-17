using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineLibrary
{
    

    // Klasa bazowa (Abstrakcja i Dziedziczenie)
    // Używamy atrybutów JSON, aby plik wiedział, która klasa pochodna jest zapisana (Polimorfizm w JSON)
    [JsonDerivedType(typeof(Book), typeDiscriminator: "book")]
    [JsonDerivedType(typeof(Audiobook), typeDiscriminator: "audiobook")]
    public abstract class LibraryItem
    {
        // Hermetyzacja: Pola prywatne są ukryte, dostęp przez właściwości (Properties)
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Title { get; set; }
        public bool IsBorrowed { get; set; } = false;

        public LibraryItem(int id, string title)
        {
            Id = id;
            Title = title;
        }

        // Polimorfizm: Metoda abstrakcyjna, którą każda klasa pochodna musi zaimplementować po swojemu
        public abstract string GetDetails();

        // Zwykła metoda wirtualna
        public virtual void DisplayInfo()
        {
            string status = IsBorrowed ? "[Wypożyczona]" : "[Dostępna]";
            Console.WriteLine($"{Id}. {Title} {status}");
        }
    }

    // Dziedziczenie: Klasa Book dziedziczy po LibraryItem
    public class Book : LibraryItem
    {
        public string Author { get; set; }
        public int PageCount { get; set; }

        // Konstruktor dziedziczący (base)
        public Book(int id, string title, string author, int pageCount) : base(id, title)
        {
            Author = author;
            PageCount = pageCount;
        }

        // Polimorfizm: Nadpisanie metody (Override)
        public override string GetDetails()
        {
            return $"KSIĄŻKA: {Title}, Autor: {Author}, Stron: {PageCount}";
        }
    }

    //  Dziedziczenie: Klasa Audiobook dziedziczy po LibraryItem
    public class Audiobook : LibraryItem
    {
        public string Narrator { get; set; }
        public double DurationInHours { get; set; }

        public Audiobook(int id, string title, string narrator, double duration) : base(id, title)
        {
            Narrator = narrator;
            DurationInHours = duration;
        }

        //  Polimorfizm: Nadpisanie metody
        public override string GetDetails()
        {
            return $"AUDIOBOOK: {Title}, Czyta: {Narrator}, Czas: {DurationInHours}h";
        }
    }

    // Klasa zarządzająca logiką
    public class LibraryManager
    {
        //  Kolekcje Generyczne: List<T>
        private List<LibraryItem> _items = new List<LibraryItem>();
        private const string FilePath = "library_data.json";

        public void AddItem(LibraryItem item)
        {
            _items.Add(item);
            Console.WriteLine("Dodano element do biblioteki.");
        }

        public void ShowAllItems()
        {
            Console.WriteLine("\n--- Lista Pozycji ---");
            
            if (_items.Count == 0)
            {
                Console.WriteLine("Biblioteka jest pusta.");
                return;
            }

            foreach (var item in _items)
            {
                // Wywołanie polimorficzne - zadziała odpowiednia wersja metody zależnie od typu obiektu
                Console.WriteLine(item.GetDetails());
                Console.WriteLine($"   Status: {(item.IsBorrowed ? "Wypożyczony" : "Dostępny")}");
            }
        }

        public void BorrowItem(int id)
        {
            // LINQ (element kolekcji) + 1. Instrukcje warunkowe
            var item = _items.FirstOrDefault(i => i.Id == id);

            if (item == null)
            {
                Console.WriteLine("Nie znaleziono przedmiotu o takim ID.");
            }
            else if (item.IsBorrowed)
            {
                Console.WriteLine("Ten przedmiot jest już wypożyczony!");
            }
            else
            {
                item.IsBorrowed = true;
                Console.WriteLine($"Pomyślnie wypożyczono: {item.Title}");
            }
        }

        // Zapis danych do pliku JSON
        public void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_items, options);
                File.WriteAllText(FilePath, jsonString);
                Console.WriteLine("Dane zapisane do pliku JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu: {ex.Message}");
            }
        }

        // Odczyt danych z pliku JSON
        public void LoadFromFile()
        {
            if (!File.Exists(FilePath))
            {
                Console.WriteLine("Brak pliku zapisu. Tworzę nową bibliotekę.");
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(FilePath);
                _items = JsonSerializer.Deserialize<List<LibraryItem>>(jsonString) ?? new List<LibraryItem>();
                Console.WriteLine($"Wczytano {_items.Count} pozycji z pliku.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd odczytu: {ex.Message}");
            }
        }
    }

    // Główna klasa programu
    class Program
    {
        static void Main(string[] args)
        {
            LibraryManager library = new LibraryManager();
            library.LoadFromFile(); // Automatyczny odczyt na starcie

            bool exit = false;

            // Pętle: while (Główna pętla programu)
            while (!exit)
            {
                Console.WriteLine("\n=== SYSTEM ONLINE-BIBLIOTEKA ===");
                Console.WriteLine("1. Dodaj Książkę");
                Console.WriteLine("2. Dodaj Audiobooka");
                Console.WriteLine("3. Wyświetl wszystkie pozycje");
                Console.WriteLine("4. Wypożycz pozycję");
                Console.WriteLine("5. Zapisz i Wyjdź");
                Console.Write("Wybierz opcję: ");

                string input = Console.ReadLine();

                // Instrukcje warunkowe: switch
                switch (input)
                {
                    case "1":
                        AddBook(library);
                        break;
                    case "2":
                        AddAudiobook(library);
                        break;
                    case "3":
                        library.ShowAllItems();
                        break;
                    case "4":
                        Console.Write("Podaj ID do wypożyczenia: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            library.BorrowItem(id);
                        }
                        else
                        {
                            Console.WriteLine("Błędny format ID.");
                        }
                        break;
                    case "5":
                        library.SaveToFile();
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Nieznana opcja.");
                        break;
                }
            }
        }

        // Metody pomocnicze do UI
        static void AddBook(LibraryManager lib)
        {
            Console.Write("Podaj ID: ");
            int id = int.Parse(Console.ReadLine());
            Console.Write("Podaj Tytuł: ");
            string title = Console.ReadLine();
            Console.Write("Podaj Autora: ");
            string author = Console.ReadLine();
            Console.Write("Liczba stron: ");
            int pages = int.Parse(Console.ReadLine());

            lib.AddItem(new Book(id, title, author, pages));
        }

        static void AddAudiobook(LibraryManager lib)
        {
            Console.Write("Podaj ID: ");
            int id = int.Parse(Console.ReadLine());
            Console.Write("Podaj Tytuł: ");
            string title = Console.ReadLine();
            Console.Write("Podaj Lektora: ");
            string narrator = Console.ReadLine();
            Console.Write("Czas trwania (godziny): ");
            double duration = double.Parse(Console.ReadLine());

            lib.AddItem(new Audiobook(id, title, narrator, duration));
        }
    }
}