using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    public class Author
    {
        public string name { set; get; }
        public Author(string name)
        {
            this.name = name;
        }
    }
    public class Note
    {
        public int id { set; get; }
        public Author author { set; get; }
        public string title { set; get; }
        public string content { set; get; }
        public Note(Author author, string title, string content)
        {
            var rand = new Random();
            this.id = rand.Next(1, 1000);
            this.author = author;
            this.title = title;
            this.content = content;
        }
    }

    class ImportExportData
    {
        private static string authorsPath = @".\authors.csv";
        private static string notePath = @".\notes.csv";

        public static List<Author> ImportAuthors()
        {
            try
            {
                using StreamReader streamReader = new StreamReader(authorsPath);
                using CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                csvReader.Configuration.Delimiter = ",";
                var authors = csvReader.GetRecords<Author>().ToList();
                streamReader.Close();
                return authors;
            }
            catch (Exception ex)
            {
                FileStream fs = File.Create(authorsPath);
                fs.Close();
                return new List<Author>();
            }
        }
        public static List<Note> ImportNotes()
        {
            try
            {
                using StreamReader streamReader = new StreamReader(notePath);
                using CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                csvReader.Configuration.Delimiter = ",";
                List<Note> notes = csvReader.GetRecords<Note>().ToList();
                streamReader.Close();
                return notes;
            }
            catch (Exception ex)
            {
                FileStream fs = File.Create(notePath);
                fs.Close();
                return new List<Note>();
            }
        }
        private static void ExportAuthors(List<Author> authors)
        {
            using StreamWriter streamWriter = new StreamWriter(authorsPath);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.Configuration.Delimiter = ",";
            csvWriter.WriteRecords(authors);
            streamWriter.Close();
        }
        private static void ExportNotes(List<Note> notes)
        {
            using StreamWriter streamWriter = new StreamWriter(notePath);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.Configuration.Delimiter = ",";
            csvWriter.WriteRecords(notes);
            streamWriter.Close();
        }

        public static void ExportData(List<Author> authors, List<Note> notes)
        {
            ExportAuthors(authors);
            ExportNotes(notes);
        }

    }
    class Program
    {
        private static List<Author> authors = new List<Author>();
        private static List<Note> notes = new List<Note>();
        private static string state = "START";
        private static Author currentAuthor;
        private static List<Note> currentNotes;
        private static Dictionary<string, List<string>> messagesDict = new Dictionary<string, List<string>>() {
            { "START", new List<string>() { "Привет!" }  },
            { "CREATEUSER", new List<string>() { "Введите имя нового пользователя или <no>, если хотите отменить команду" } },
            { "LOGIN", new List<string>() { "Введите имя пользователя или <no>, если хотите отменить команду" } },
            {"SHOWNOTES", new List<string> {"Ваши заметки:" } },
            {"ADDNOTE", new List<string> {"Новая заметка"} },
            {"EDITNOTE", new List<string> {"Редактирование заметок"} },
            {"REMOVENOTE", new List<string> {"Удалить заметку"} },
            {"LOGOUT", new List<string> {"Смена пользователя"} },
            {"SAVE", new List<string> {"Сохранить заметки"} },
            {"CLEAR", new List<string> {} },
            { "HELP", new List<string>() {  "LOGIN - авторизация",
                                            "CREATEUSER - создать пользователя",
                                            "SHOWNOTES - показать заметки",
                                            "ADDNOTE - добавить новую заметку",
                                            "EDITNOTE - редактировать заметку",
                                            "REMOVENOTE - удалить заметку",
                                            "LOGOUT - сменить текущего пользователя",
                                            "SAVE - сохранить заметки",
                                            "CLEAR - очистить консоль",
                                            "EXIT - выйти из программы"
                                            } },
            {"WRONGCOMMAND", new List<string>() {"Неверная команда"} },
            {"EXIT", new List<string>() {"Выход из программы"} },
        };

        private static void writeMessage()
        {
            List<string> messages = messagesDict[state];
            foreach (string s in messages)
            {
                Console.WriteLine(s);
            }
        }

        private static bool createUser()
        {
            while (true)
            {
                string authorName = Console.ReadLine();
                if (authorName == "no")
                {
                    state = "START";
                    return true;
                }
                bool checkFlag = true;
                foreach (var s in authors)
                {
                    if (s.name == authorName)
                    {
                        Console.WriteLine("Такой пользователь уже существует");
                        checkFlag = false;
                        break;
                    }
                }
                if (checkFlag)
                {
                    Author newAuthor = new Author(authorName);
                    authors.Add(newAuthor);
                    currentAuthor = newAuthor;
                    return true;
                }
            }

        }
        private static bool loginUser()
        {
            while (true)
            {
                Console.Write("> ");
                string authorName = Console.ReadLine();
                if (authorName == "no")
                {
                    state = "START";
                    return true;
                }
                bool checkFlag = false;
                foreach (var s in authors)
                {
                    if (s.name == authorName)
                    {
                        currentAuthor = s;
                        return true;
                    }
                }
                if (!checkFlag)
                {
                    Console.WriteLine("Такого пользователя не существует");
                    continue;
                }
            }
        }
        private static bool checkValidAuthor()
        {
            if (currentAuthor == null)
            {
                Console.WriteLine("Пользователь не авторизован");
                return false;
            }
            else
            {
                return true;
            }

        }
        private static void showNotes()
        {
            if (checkValidAuthor())
            {
                List<Note> cNotes = new List<Note>();
                foreach (var note in notes)
                {
                    if (note.author.name == currentAuthor.name)
                    {
                        cNotes.Add(note);
                    }
                }
                if (cNotes.Count == 0)
                {
                    Console.WriteLine("Нет заметок");
                    currentNotes = cNotes;
                    return;
                }
                foreach (var note in cNotes)
                {
                    Console.WriteLine($"id: {note.id}");
                    Console.WriteLine($"Заголовок: {note.title}");
                    Console.WriteLine($"Содержание: {note.content}");
                }
                currentNotes = cNotes;
            }
        }

        private static void addNote()
        {
            if (checkValidAuthor())
            {
                Console.Write("Введите название заметки: ");
                string title = Console.ReadLine();
                Console.Write("Введите содержание заметки: ");
                string content = Console.ReadLine();
                notes.Add(new Note(currentAuthor, title, content));
                Console.WriteLine("Заметка добавлена");
            }
        }
        private static void editNote()
        {
            if (checkValidAuthor())
            {
                while (true)
                {
                    showNotes();
                    Console.Write("Введите номер заметки или <no> для отмены: ");
                    string answer = Console.ReadLine();
                    if (answer == "no")
                        return;
                    int id;
                    if (Int32.TryParse(answer, out id))
                    {
                        Note curNote = null;
                        foreach (var note in currentNotes)
                        {
                            if (note.id == id)
                            {
                                curNote = note;
                            }
                        }
                        if (curNote != null)
                        {
                            while (true)
                            {
                                Console.Write("1. Редактировать заголовок;\n2. Редактировать содержание.\n3. Сохранить и выйти\n<no> для отмены\n > ");
                                string answer1 = Console.ReadLine();
                                if (answer1 == "no") return;
                                else if (answer1 == "1")
                                {
                                    Console.Write("Введите новый заголовок или <no> для отмены > ");
                                    string a = Console.ReadLine();
                                    if (a == "no")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        curNote.title = a;
                                    }
                                }
                                else if (answer1 == "2")
                                {
                                    Console.Write("Введите новое содержание или <no> для отмены > ");
                                    string a = Console.ReadLine();
                                    if (a == "no")
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        curNote.content = a;
                                    }
                                }
                                else if (answer1 == "3")
                                {
                                    for (int i = 0; i < notes.Count(); i++)
                                    {
                                        if (curNote.id == notes[i].id)
                                        {
                                            notes[i] = curNote;
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Ошибка ввода");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный номер");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверный номер");
                    }
                }
            }
        }
        private static void removeNote()
        {
            if (checkValidAuthor())
            {
                while (true)
                {
                    showNotes();
                    Console.Write("Введите номер заметки или <no> для отмены: ");
                    string answer = Console.ReadLine();
                    if (answer == "no")
                        return;
                    int id;
                    if (Int32.TryParse(answer, out id))
                    {
                        for (int i = 0; i < notes.Count(); i++)
                        {
                            if (notes[i].id == id)
                            {
                                notes.Remove(notes[i]);
                                Console.WriteLine("Заметка удалена");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка ввода");
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            authors = ImportExportData.ImportAuthors();
            notes = ImportExportData.ImportNotes();
            while (true)
            {
                if (!messagesDict.ContainsKey(state))
                {
                    state = "WRONGCOMMAND";
                }

                writeMessage();
                switch (state)
                {
                    case "CREATEUSER":
                        bool createUserResult = createUser();
                        if (createUserResult && state == "START")
                            continue;
                        else
                            state = "SHOWNOTES";
                        break;
                    case "LOGIN":
                        bool loginUserResult = loginUser();
                        if (loginUserResult && state == "START")
                            continue;
                        else
                            state = "SHOWNOTES";
                        break;
                    case "SHOWNOTES":
                        showNotes();
                        break;
                    case "ADDNOTE":
                        addNote();
                        break;
                    case "EDITNOTE":
                        editNote();
                        break;
                    case "REMOVENOTE":
                        removeNote();
                        break;
                    case "LOGOUT":
                        currentAuthor = null;
                        break;
                    case "CLEAR":
                        Console.Clear();
                        break;
                    case "SAVE":
                        if (checkValidAuthor())
                        {
                            ImportExportData.ExportData(authors, notes);
                            Console.WriteLine("Заметки сохранены");
                        }
                        break;
                    case "EXIT":
                        if (checkValidAuthor())
                        {
                            ImportExportData.ExportData(authors, notes);
                        }
                        return;
                    default:
                        break;
                }
                Console.Write("> ");
                state = Console.ReadLine().ToUpper();
                //Console.Clear();
            }
        }
    }
}
