using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace HashcodeApp
{
    class Library
    {
        public List<Book> libraryBookList = new List<Book>();
        public List<Book> librarySentBookList = new List<Book>();

        public int bookCount { get; set; }
        public int singUpTime { get; set; }
        public int booksPerDay { get; set; }
        public int libraryIndex { get; set; }

        public bool isLibrarySignedUp = false;
        public bool isLibrarySorted = false;

        public Library(int bCount, int time, int bookShipmentCapability, int id)
        {
            bookCount = bCount;
            singUpTime = time;
            booksPerDay = bookShipmentCapability;
            libraryIndex = id; 
        }

        public void AddBook(Book bookToAdd)
        {
            libraryBookList.Add(bookToAdd);
        }
    }

    class Book
    {
        public int BookValue;
        public int BookIndex;

        public Book(int id, int bookValue)
        {
            BookIndex = id;
            BookValue = bookValue;
        }
    }


    class Program
    {
        public static Encoding StandardEncoding = Encoding.ASCII;

        public static int currentDay;

        public static int deadline;
        public static int totalBookNumber;
        public static int libraryCount;

        public static List<Book> bookList = new List<Book>();
        public static List<Library> libraryList = new List<Library>();

        public static List<Book> booksSent = new List<Book>();

        //Scorekeeping
        public static List<Library> signedUpLibraries = new List<Library>();

        static void Main(string[] args)
        {
            FileScanner();

            Sorter();

            //NumberOfSimilarBooksFinder();

            //[0] sign up in process, [2] previous library done signing up
            bool[] flagArray = new bool[2] { false, false };
            bool[,] dailyArray = new bool[deadline, 2];

            int currentlyManagedLibrary = 0;

            int testDay;

            for(int i = 1; i < deadline; i++)
            {
                if(currentlyManagedLibrary >= libraryList.Count())
                {
                    break;
                }

                //Keeping track of concurrent sign ups
                if (dailyArray[i,0] == false)
                {
                    if (i + libraryList[currentlyManagedLibrary].singUpTime <= deadline)
                    {
                        for (int j = i; j < i + libraryList[currentlyManagedLibrary].singUpTime; j++)
                        {
                            dailyArray[j, 0] = true;
                        }

                        dailyArray[i + libraryList[currentlyManagedLibrary].singUpTime, 1] = true;
                    }
                }

                if(dailyArray[i, 1] == true)
                {
                    libraryList[currentlyManagedLibrary].isLibrarySignedUp = true;
                    signedUpLibraries.Add(libraryList[currentlyManagedLibrary]);
                    currentlyManagedLibrary++;
                }
                testDay = i;

                Console.WriteLine("Day: " + testDay.ToString());

                foreach (Library library in libraryList)
                {
                    if (library.isLibrarySignedUp && !library.isLibrarySorted)
                    {
                        int booksSentInADay = 0;
                        int currentlySentBookIndex = 0;

                        while (booksSentInADay < library.booksPerDay)
                        {
                            if(currentlySentBookIndex == library.libraryBookList.Count())
                            {
                                library.isLibrarySorted = true;
                                break;
                            }

                            if (!HasBookBeenSent(library.libraryBookList[currentlySentBookIndex]))
                            {
                                booksSent.Add(library.libraryBookList[currentlySentBookIndex]);
                                library.librarySentBookList.Add(library.libraryBookList[currentlySentBookIndex]);
                                booksSentInADay++;
                            }

                            currentlySentBookIndex++;
                        }
                    }
                }
            }

            CalculateBookValues();

            OutputResult();
        }

        static void FileScanner()
        {
            FileStream fileStream = new FileStream("D:\\problem.txt", FileMode.OpenOrCreate);
            byte[] byteBuffer = new byte[fileStream.Length];

            fileStream.Read(byteBuffer, 0, byteBuffer.Length);

            string input = StandardEncoding.GetString(byteBuffer);

            //[0] library data, [1] book score, [2 ..] individual library data
            string[] fileLines = input.Split(new char[] { '\n' });

            //[0] number of books, [1] number of libraries, [2] number of days
            string[] mainDataSplits = fileLines[0].Split(new char[] { ' ' });

            totalBookNumber = Convert.ToInt32(mainDataSplits[0]);
            libraryCount = Convert.ToInt32(mainDataSplits[1]);
            deadline = Convert.ToInt32(mainDataSplits[2]);

            //Line contatins each book data
            string[] bookValueSplits = fileLines[1].Split(new char[] { ' ' });

            for(int i = 0; i < totalBookNumber; i++)
            {
                bookList.Add(new Book(i, Convert.ToInt32(bookValueSplits[0])));
            }

            int libraryIndex = 0; ;
            for (int i = 2; i <= libraryCount + 2; i++)
            {
                //Per library, [0] bookCount, [1] bookSignUpTime [2] bookShipmentCapability
                string[] libraryConditionsSplits = fileLines[i].Split(new char[] { ' ' });
                libraryList.Add(new Library(Convert.ToInt32(libraryConditionsSplits[0]), Convert.ToInt32(libraryConditionsSplits[1]),
                    Convert.ToInt32(libraryConditionsSplits[2]), libraryIndex));

                string[] bookIndexList = fileLines[i + 1].Split(new char[] { ' ' });

                foreach(string bookIndex in bookIndexList)
                {
                    libraryList[libraryIndex].AddBook(bookList[Convert.ToInt32(bookIndex)]);             
                }

                libraryIndex++;
                i++;
            }

        }

        static void Sorter()
        {
            libraryList.Sort(CompareByBookCount);
        }

        public static int CompareByBookCount(Library library1, Library library2)
        {
            return ((library2.bookCount * library2.booksPerDay) /
                library2.singUpTime).CompareTo((library1.bookCount * library1.booksPerDay) / library1.singUpTime);
        }

        static bool HasBookBeenSent(Book book)
        {
            foreach(Book sentBook in booksSent)
            {
                if(sentBook == book)
                {
                    return true;
                }
            }

            return false;
        }

        static void CalculateBookValues()
        {
            int totalValue = 0;

            foreach(Book book in booksSent)
            {
                totalValue += book.BookValue;
            }

            int test = totalValue;
        }

        static void OutputResult()
        {
            FileStream fileStream = new FileStream("D:\\problemSolution.txt", FileMode.OpenOrCreate);

            byte[] byteBuffer;

            string output = signedUpLibraries.Count.ToString() + Environment.NewLine;

            for(int i = 0; i < signedUpLibraries.Count; i++)
            {
                output += signedUpLibraries[i].libraryIndex.ToString() + " " + signedUpLibraries[i].librarySentBookList.Count.ToString() + Environment.NewLine;
                
                for(int j = 0; j < signedUpLibraries[i].librarySentBookList.Count; j++)
                {
                    output += signedUpLibraries[i].librarySentBookList[j].BookIndex.ToString() + " ";
                }

                output += Environment.NewLine;
            }

            byteBuffer = StandardEncoding.GetBytes(output);

            fileStream.Write(byteBuffer, 0, byteBuffer.Length);
        }
    }
}
