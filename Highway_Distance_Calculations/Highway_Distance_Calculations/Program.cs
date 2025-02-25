using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Numerics;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Dijkstra
{
    internal class Program
    {
        // Static olan veri yapılarımız birden çok yerde kullaıldıklarından ve bellekte kayıtlı kalmasını istediğimizden static yapılmıştır.
        static int[][] jaggedMatrix;        // İller Arası Mesafe Cetcelindeki verilen kaydedildiği jagged türünde bir iki boyutlu dizidir.
        static string[] cities;             // Şehirlerin adını tutan dizidir.
        static string[] districts;          // İlçelerin adını tutan dizidir.
        static int[,] districtMatrix;       //  İlçelerin kendi aralarında mesafelerinin tutulduğu iki boyutlu dizidir.
        static int[][] cloneJaggedMatrix;   // Jagged Matrisindeki verilenin kopyasını tutan jagged türünde bir iki boyutlu dizidir.
        static void Main(string[] args)
        {
            cloneJaggedMatrix = new int[81][];
            int[][] neighborCityCode = new int[81][];   // Şehirlerin komşularının plakalarını tutan dizidir.
            int[,] districtMatrix = new int[30, 30];    //Ilçe matrisidir.
            int[,] infinityTableCities = new int[81, 81];   // Bu tabloyu dijkstra fonksiyonunda kullanmak için oluşturulur.
            int[][] neighbourDistrictNumbers = new int[30][];    //Ilçelerin komşularının plakalarını tutan dizidir.
            int[,] infinityTableDistricts = new int[30, 30];    // Komşu ilçeler dışı sonsuz olan matristir.
            
            ArrayList maxDistance = new ArrayList();    // En fazla farka sahip il ve ilçelerin yazıldığı arraylisttir.
            ArrayList minDistance = new ArrayList();    //En düşük farka sahip il ve ilçelerin yazıldığı arraylisttir.
            ArrayList plateArrayList = new ArrayList();  // gezilen il/ilçe plaka numaralarını tutar.
            try
            {
                // Excel dosyasının yolu
                string filePath = @"C:\Users\hp\Desktop\ilmesafe (1).xlsx";

                // Satır sayısı 81 olan ve sütun 81 e kadar artarak giden bir jagged matrix tanımlama
                jaggedMatrix = GetJaggedMatrix(filePath);

                // Şehir isimleri listesi
                cities = GetCities(filePath);

                // Rastgele şehir çifti ve mesafeler için değişkenler
                var random = new Random();
                int[][] cityPairs = new int[10][];
                int[] distances = new int[10];

                // 10 rastgele şehir çifti oluştur
                for (int i = 0; i < 10; i++)
                {
                    int plate1 = random.Next(1, 82);
                    int plate2;

                    // İki şehir aynı değilse seç
                    do
                    {
                        plate2 = random.Next(1, 82);
                    }
                    while (plate1 == plate2);

                    // Plate1 > Plate2 olacak şekilde sırala
                    if (plate1 < plate2)
                    {
                        int temp = plate1;
                        plate1 = plate2;
                        plate2 = temp;
                    }

                    // Şehirler arası mesafeyi jaggedMatrix'ten al
                    distances[i] = jaggedMatrix[plate1 - 1][plate2 - 1];
                    cityPairs[i] = new int[] { plate1, plate2 };
                }

                // Şehir çiftlerini ve mesafelerini ekrana yazdır
                Console.WriteLine("City Pair\tPlate 1\t        City 1\t\t\tPlate 2\t        City 2\t\t\tDistance (km)");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------");

                for (int i = 0; i < 10; i++)
                {
                    int plate1 = cityPairs[i][0];
                    int plate2 = cityPairs[i][1];
                    int distance = distances[i];

                    // Şehir çifti ve mesafeyi yazdır
                    Console.WriteLine($"{i + 1,-10}\t{plate1,-10}\t{cities[plate1],-20}\t{plate2,-10}\t{cities[plate2],-20}\t{distance,-10} km");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            NeighboringCitiesList(neighborCityCode, infinityTableCities, cloneJaggedMatrix);  // Jagged matrisi değiştiri ve dijkstrafunction metodunda gerekli matrisi oluşturur. 
            int[,] Dijkstra = (int[,])infinityTableCities.Clone();  // Komşuların mesafelerini içeren matrisin birden fazla kullanılmasından dolayı kopyasını oluşturur. 

            StreamWriter sw = new StreamWriter("C:\\Users\\hp\\Desktop\\NeighbourList.txt");    //Dosya konumu ayarlanmalıdır.
            Console.WriteLine("\nJagged Matrix:");// Jagged array'i ekrana yazdır
            for (int i = 0; i < jaggedMatrix.Length; i++)
            {
                for (int j = 0; j < jaggedMatrix[i].Length; j++)
                {
                    sw.Write($"{jaggedMatrix[i][j],4} "); // İller arası mesafe en fazla 4 basamaklı olduğu için 4 karakter genişliğinde yazdırır.(Matrisin daha düzenli gözükmesi için)
                }
                sw.WriteLine(); // Her satırın sonunda yeni satıra geçer.
            }
            sw.Close();
            DijkstraFunction(Dijkstra, neighborCityCode, plateArrayList, infinityTableCities);  // Şehirler için dijkstra fonksiyonunu uygular.
            cityDistances(cities, Dijkstra, cloneJaggedMatrix, minDistance, maxDistance);   // Şehirler için gerekli çıktı işlemlerininin bulunduğu metottur

            ReadDistrictFolder(districtMatrix); // İlçeler için okuma yapar
            districts = DistrictArray();    // İlçe isimlerini okunan dosyadan dizimize ekler 
            StreamWriter swDistrict = new StreamWriter("C:\\Users\\hp\\Desktop\\DistrictMatrix.txt");   // çıktı çok büyük olduğundan txt dosyasına kaydeder.
            for (int i = 0; i < districtMatrix.GetLength(0); i++)                                       //Dosya konumu ayarlanmalıdır.
            {
                for (int j = 0; j < districtMatrix.GetLength(0); j++)
                {
                    swDistrict.Write($"{districtMatrix[i,j],4} "); // İller arası mesafe en fazla 4 basamaklı olduğu için 4 karakter genişliğinde yazdırır.(Matrisin daha düzenli gözükmesi için)
                }
                swDistrict.WriteLine(); // Her satırın sonunda yeni satıra geçer.
            }
            swDistrict.Close();
            NeighboringDistrictsList(neighbourDistrictNumbers, infinityTableDistricts, districtMatrix);     // dijkstra fonksiyonumuzda kullanılacak komşuların mesafesini içeren diziyi oluşturan metottur.
            int[,] DijkstraDistricts = (int[,])infinityTableDistricts.Clone();  // Komşuların mesafelerini içeren matrisin birden fazla kullanılmasından dolayı kopyasını oluşturur. 
            DijkstraFunction(DijkstraDistricts, neighbourDistrictNumbers, plateArrayList, infinityTableDistricts); // İlçeler için dijkstra fonksiyonunu uygular.
            districtDistance(districts, DijkstraDistricts, districtMatrix, minDistance, maxDistance);   // İlçeler için gerekli çıktı işlemlerininin bulunduğu metottur

            Console.ReadKey();
        }
        private static int[][] GetJaggedMatrix(string filePath)
        {
            int[][] jaggedMatrix = new int[81][];// iller için 81 boyutunda bir jagged matrix oluşturma

            // Her satırın sütun sayısını belirleyip dizileri başlat
            for (int i = 0; i < jaggedMatrix.Length; i++)
            {
                jaggedMatrix[i] = new int[i + 1]; // 1'den başlayarak artan sütun sayısı
            }

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var range = worksheet.RangeUsed();

                for (int row = 3; row <= 83; row++)//ilmesafe.xlsx  excel dosyasında uzaklık değerleri 3'e 3'ten başladığı için oradan değerleri okutmaya başlıyoruz.
                {
                    for (int col = 3; col <= row; col++)
                    {
                        if (row == col)// Satır ve sütun numarası eşit mi kontrol eder.
                        {
                            continue; // Eşit olan hücreyi atlar
                        }

                        // Tek tek hücreleri okuma işlemi
                        var cell = worksheet.Cell(row, col);

                        // Hücre içindeki değer boş değilse ve tam sayı içeriyorsa ekleme işlemi
                        if (int.TryParse(cell.GetString(), out int cellValue))
                        {
                            jaggedMatrix[row - 3][col - 3] = cellValue;//Dosya okuma işlemi 3. satır 3. sütundan başladığı için -3 ile işlem  yapıyoruz.
                        }
                    }
                }
            }

            return jaggedMatrix;
        }

        private static string[] GetCities(string filePath)// Dosya yolu verilen excel dosyasını açar . İşlemek için bir XLWorkbook nesnesi oluşturur.
        {
            var columnValues = new List<string>();
            columnValues.Add("");// Başına boşluk eklememizin sebebi oluşturdugumuz arrayde index 0 dan başlıyor fakat Türkiye'deki plakalar 1 den başlıyor. İllerin listedeki indexlerinin plakalarla eşit olması için yapıyoruz. 

            using (var workbook = new XLWorkbook(filePath))// Dosya yolundaki excel dosyasını açar.
            {
                var worksheet = workbook.Worksheet(1);//1. çalışma sayfasını alır.
                var range = worksheet.RangeUsed();// Çalışma sayfasında veri içeren hücre aralığını alır.

                foreach (var row in range.RowsUsed())
                {
                    if (row.RowNumber() >= 3)
                    {
                        var cellValue = row.Cell(2).GetValue<string>();//3.satır 2.sütuna denk gelen Adana'dan aşağıya doğru Düzceye kadar iller okunup cities adlı diziye ekleniyor.
                        columnValues.Add(cellValue);
                    }
                }
            }
            return columnValues.ToArray();
        }
        private static void CloneJagged(int[][] jaggedMatrix, int[][] cloneJaggedMatrix)    // Bu metot ile oluşturduğumuz jugged matrix i klonluyoruz.
        {
            for (int i = 0; i < jaggedMatrix.GetLength(0); i++)
            {
                cloneJaggedMatrix[i] = (int[])jaggedMatrix[i].Clone();
            }
        }
        private static void NeighboringCitiesList(int[][] neighborCityCode, int[,] infinityTableCities, int[][] cloneJaggedMatrix)
        {
            CloneJagged(jaggedMatrix, cloneJaggedMatrix);

            for (int i = 0; i < 81; i++)
            {
                for (int j = 0; j < 81; j++)
                {
                    infinityTableCities[i, j] = int.MaxValue;  // Tüm tabloyu sonsuz yapıyoruz.
                }
            }
            for (int i = 0; i < jaggedMatrix.GetLength(0); i++) {
                for (int j = 0; j < i + 1; j++)
                {
                    jaggedMatrix[i][j] = int.MaxValue;  // Tüm tabloyu sonsuz yapıyoruz.
                }
            }

            try
            {
                string file = @"C:\Users\hp\Desktop\illerVePlakalari.txt";  // Komşu illerin plakalarının olduğu txt dosyasını okutuyoruz.
                string[] lines = File.ReadAllLines(file);                   //Dosya konumu ayarlanmalıdır.


                for (int i = 0; i < lines.Length; i++)
                {

                    int[] neighbourPlates;  // Komşu illerin plakalarının olduğu txt dosyasından her satırı alıp onu dizi yapıyoruz.
                    string[] numbers = lines[i].Split(',');
                    neighbourPlates = Array.ConvertAll(numbers, int.Parse);
                    neighborCityCode[i] = new int[neighbourPlates.Length];  // Komşu sayısının index olduğu bir jugged array oluşturuyoruz.
                    for (int j = 0; j < neighbourPlates.Length; j++)
                    {

                        int a;
                        a = neighbourPlates[j]; // Komşu illerin  tutuyoruz.
                        neighborCityCode[i][j] = a;
                        if (a - 1 < i)
                        {
                            infinityTableCities[i, a - 1] = cloneJaggedMatrix[i][a - 1];  // Matrixteki alt üçgende komşu illerin değerlerini sonsuzla değiştirir.
                            jaggedMatrix[i][a - 1] = cloneJaggedMatrix[i][a - 1];   // Jugged array da sadece alt üçgeni kullanmamız istendi. Burda da daha önce sıfırladığımız arrayde komşu olanların değerlerini klonladığımız arrayden alıyoruz.
                        }
                        else if (a - 1 > i)
                        {
                            infinityTableCities[i, a - 1] = cloneJaggedMatrix[a-1][i];  // Matrixteki üst üçgende komşu illerin değerlerini sonsuzla değiştiriyoruz.
                        }

                    }

                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found.");
            }
        }

        // Dijkstra algoritmasının iişlemlerini içeren metottur.
        private static void DijkstraFunction(int[,] Dijkstra, int[][] neighborCityCode, ArrayList plateArrayList ,int[,] infinityTableCities)  
        {
            for (int k = 0; k < Dijkstra.GetLength(0); k++)     // Metot Dijkstra dizsinin her bir satırnını doldurmak için tur atar.
            {
                int startnode = k;      // Diğer konumlara olan en kısa yolu bulmak başlangıç konumunu tutar.
                int node = 0;           // Başlangıç konumu dışında diğer konumların plakalarının tek tek tutulur.
                int indexsaver = 0;     // myPlateListte gezmemiz için index numarasını sayar.
                Dijkstra[k, k] = 0;     // Konumun kendisiyle olan uzaklığını sıfır yapar. 
                for (int i = 0; i < neighborCityCode[k].Length; i++)  // Başlangıç düğümümüzün komşularını plate arraye ekler ve dijkstra arrayinde uzaklıklarınız yazar.
                {

                    plateArrayList.Add(neighborCityCode[k][i]);
                    Dijkstra[k, neighborCityCode[k][i] - 1] = infinityTableCities[k, neighborCityCode[k][i] - 1];
                }
                plateArrayList.Add(startnode + 1); // Başlangıç konumun gezilen şehir/ilçe listesine ekler.
                while (indexsaver < plateArrayList.Count)   // Gezilen il/ilçe kalmayıncaya kadar işlem sürer.
                {
                    node = (int)plateArrayList[indexsaver]; // Gezilen il/ilçe listesindeki ilk konumun plakasını tutar.

                    foreach (int plate in neighborCityCode[node - 1])   // Node daki konumun komşularınında gezmemizi sağlar.
                    {
                        int isthere = plateArrayList.IndexOf(plate);    // node daki konumun komşularının plaka numarası Gezilen il/ilçe listesinde varmı diye bakar.


                        if (plate == (startnode + 1) && -1 == isthere)  // Node daki konumun komşusu başlangıç konumu mu diye bakar . Gezilmemişse listeye ekler.
                        {
                            plateArrayList.Add((int)plate);
                            continue;
                        }
                        else if (-1 == isthere && (infinityTableCities[node - 1, plate - 1] + Dijkstra[startnode, node - 1]) < Dijkstra[startnode, plate - 1])  // Node daki konumun komşusu gezilmemişse ve dijkstra arrayinden
                        {                                                                                                                                       // küçükse mesafe bilgisi işler ve gezilmiş olarak listeye ekler.
                            Dijkstra[startnode, plate - 1] = infinityTableCities[node - 1, plate - 1] + Dijkstra[startnode, node - 1];
                            plateArrayList.Add((int)plate);
                        }
                        else if (-1 == isthere && (infinityTableCities[node - 1, plate - 1] + Dijkstra[startnode, node - 1]) > Dijkstra[startnode, plate])  // Node daki komunum komşusu gezilmemişse ve dijkstra arrayinden
                        {                                                                                                                                   // büyükse mesafe bilgisi değişmez ve gezilmiş olarak listeye ekler.
                            plateArrayList.Add((int)plate);
                        }
                        else if (-1 != isthere && (Dijkstra[startnode, node - 1] + infinityTableCities[node - 1, plate - 1]) > Dijkstra[startnode, plate - 1]) //Node daki komunum komşusu gezilmişse ve dijkstra arrayinden
                        {                                                                                                                                                            //büyükse mesafe bilgisi değişmez ve gezilmiş olarak listeye eklenmez.

                            continue;
                        }
                        else if (-1 != isthere && (Dijkstra[startnode, node - 1] + infinityTableCities[node - 1, plate - 1]) < Dijkstra[startnode, plate - 1])//Node daki komunum komşusu gezilmişse ve dijkstra arrayinden
                        {                                                                                                                                     //küçükse mesafe bilgisi değişir ve gezilmiş olarak listeye eklenmez.
                            Dijkstra[startnode, plate - 1] = (Dijkstra[startnode, node - 1] + infinityTableCities[node - 1, plate - 1]);    // Önceden Gezilmiş konumun değeri değiştiğinden dijkstra arrayinde güncellemek için                                                                                                                                                                            
                            indexsaver = -1;                                                                                                // en kısa yol bulma işlemine baştan başlar.
                            break;
                        }
                    }
                    indexsaver++;
                }

                plateArrayList.Clear(); // Her yeni başlangıç konumu için gezilen listenin içini temizler.

            }
        }

        private static void cityDistances(string[] cities, int[,] Dijkstra, int[][] cloneJaggedMatrix, ArrayList minValue, ArrayList maxValue)
        {
            StreamWriter sWCity = new StreamWriter("C:\\Users\\hp\\Desktop\\CityOutput.txt");// Çıktılar TXT dosyasına kaydedilir. Dosya konumu ayarlanmalıdır.
            Console.WriteLine("Starting city:\tDestination city:\tDistance from highways:\tShortest way:\t(Shortest way - Distance from highways:)");
            sWCity.WriteLine("Starting city:\tDestination city:\tDistance from highways:\tShortest way:\t(Shortest way - Distance from highways:)");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            sWCity.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            minValue.Clear();
            maxValue.Clear(); 
            for (int i = 0; i < cloneJaggedMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (i == j) { continue; }
                    else
                    {

                        int difference = (Dijkstra[i, j] - cloneJaggedMatrix[i][j]);// Farklarını alır.
                        if (minValue.Count == 0 && maxValue.Count == 0)
                        {                      
                            //minValue.count ve maxValue.count 0 iken (yani Başlangıçta) ilk uzaklık her iki değere de atanır.
                            minValue.Add(difference);
                            minValue.Add(cities[i + 1]);
                            minValue.Add(cities[j + 1]);
                            maxValue.Add(difference);
                            maxValue.Add(cities[i + 1]);
                            maxValue.Add(cities[j + 1]);
                        }
                        else if (difference < (int)minValue[0])//Eğer minValue değeri o anki uzaklıktan büyükse, min value değeri silinir ve yeni değer atanır.
                        {
                            minValue.Clear();
                            minValue.Add(difference);
                            minValue.Add(cities[i + 1]);
                            minValue.Add(cities[j + 1]);
                        }
                        else if (difference == (int)minValue[0])// Yeni fark ile minValue eşitse, birden fazla minValue olusabileceği için karşılaşılan değer rrayliste eklenir. 
                        {
                            minValue.Add(difference);
                            minValue.Add(cities[i + 1]);
                            minValue.Add(cities[j + 1]);
                        }
                        if (difference > (int)maxValue[0])//Eğer maxValue değeri o anki uzaklıktan küçükse, maxValue değeri silinir ve yeni değer atanır.
                        {
                            maxValue.Clear();
                            maxValue.Add(difference);
                            maxValue.Add(cities[i + 1]);
                            maxValue.Add(cities[j + 1]);
                        }

                        else if (difference == (int)maxValue[0])    // Yeni fark ile maxValue eşitse, birden fazla maxValue olusabileceği için  karşılaşılan değer arrayliste eklenir.
                        {
                            maxValue.Add(difference);
                            maxValue.Add(cities[i + 1]);
                            maxValue.Add(cities[j + 1]);
                        }

                        Console.Write($"{cities[i + 1],-25}\t{cities[j + 1],-25}\t{cloneJaggedMatrix[i][j],-10}\t{Dijkstra[i, j],-10}\t{difference,-10}");
                        sWCity.Write($"{cities[i + 1],-25}\t{cities[j + 1],-25}\t{cloneJaggedMatrix[i][j],-10}\t{Dijkstra[i, j],-10}\t{difference,-10}");
                        Console.WriteLine();
                        sWCity.WriteLine();
                    }
                }
            }
            sWCity.Close();
            Console.WriteLine("The city(s) with the lowest difference:");
            int flagValue = 0;
            while (flagValue < minValue.Count)
            {
                Console.WriteLine($"Difference:{minValue[flagValue]}\t{minValue[flagValue + 1],-15}\t{minValue[flagValue + 2]}");
                flagValue += 3;// Yukarıda yaptığımız 3 tane ekleme işleminden sonra eklenmesi için flagValue değerini +=3 ten başlattık.
            }
            Console.WriteLine();
            flagValue = 0;
            Console.WriteLine("The city(s) with the highest difference:");
            while (flagValue < maxValue.Count)
            {
                Console.WriteLine($"Difference:{maxValue[flagValue]}\t{maxValue[flagValue + 1],-15}\t{maxValue[flagValue + 2]}");
                flagValue += 3;// Yukarıda yaptığımız 3 tane ekleme işleminden sonra eklenmesi için flagValue değerini +=3 ten başlattık.
            }
            Console.WriteLine();
        }
        private static void ReadDistrictFolder(int[,] districtMatrix)
        {

            try
            {
                string filePath = "C:\\Users\\hp\\Desktop\\izmir.xlsx";     //Dosya konumu ayarlanmalıdır.

                var columnValues = new List<string>();
                columnValues.Add("");

                using (var workbook = new XLWorkbook(filePath))// Dosya yolundaki excel dosyasını açar.
                {
                    var worksheet = workbook.Worksheet(1);//1. çalışma sayfasını alır.
                    var range = worksheet.RangeUsed();// Çalışma sayfasında veri içeren hücre aralığını alır.

                    for (int i = 0; i < 30; i++)
                    {
                        for (int j = 0; j < 30; j++)
                        {
                            var cellValue = worksheet.Cell(i + 2, j + 2).GetValue<string>();
                            if (int.TryParse(cellValue, out int value))
                            {
                                districtMatrix[i, j] = value;// Okunan değerle district matrixi dolduruyoruz.
                            }
                        }
                    }
                }
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found.");
            }
        }
        private static string[] DistrictArray()
        {
            string filePath = @"C:\Users\hp\Desktop\izmir.xlsx";    //Dosya konumu ayarlanmalıdır.
            var columnValues = new List<string>();
            columnValues.Add("");// Başına boşluk eklememizin sebebi oluşturdugumuz arrayde index'in 1'den itibaren yazdırmasını istememiz 

            using (var workbook = new XLWorkbook(filePath))// Dosya yolundaki excel dosyasını açar.
            {
                var worksheet = workbook.Worksheet(1);//1. çalışma sayfasını alır.
                var range = worksheet.RangeUsed();// Çalışma sayfasında veri içeren hücre aralığını alır.

                foreach (var row in range.RowsUsed())
                {
                    if (row.RowNumber() >= 2)
                    {
                        var cellValue = row.Cell(1).GetValue<string>();//2.satır 1.sütuna denk gelen Aliağa'dan aşağıya doğru Urlaya kadar ilçeler okunup districts adlı diziye ekleniyor.
                        columnValues.Add(cellValue);
                    }
                }
            }
            return columnValues.ToArray();
        }
        private static void NeighboringDistrictsList(int[][] neighbourDistrictNumbers, int[,] infinityTableDistricts, int[,] districtMatrix)
        {

            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    infinityTableDistricts[i, j] = int.MaxValue;  // Tüm tabloyu sonsuz yapıyoruz.
                }
            }

            try
            {
                string file = @"C:\Users\hp\Desktop\ilceKomsulari.txt";     // İlçeye komşu olan ilçelerin numaralarının olduğu txt dosyasını okutuyoruz.
                string[] lines = File.ReadAllLines(file);                   //Dosya konumu ayarlanmalıdır.


                for (int i = 0; i < lines.Length; i++)
                {

                    int[] neighbourNumbers; // İlçelerin komşu numaralarının olduğu txt dosyasından her satırı alıp onu dizi yapıyoruz.
                    string[] numbers = lines[i].Split(',');
                    neighbourNumbers = Array.ConvertAll(numbers, int.Parse);
                    neighbourDistrictNumbers[i] = new int[neighbourNumbers.Length];     // Komşu sayısının index olduğu bir jugged array oluşturuyoruz.
                    for (int j = 0; j < neighbourNumbers.Length; j++)
                    {

                        int a;
                        a = neighbourNumbers[j];
                        neighbourDistrictNumbers[i][j] = a;
                        if (a - 1 < i)
                        {
                            infinityTableDistricts[i, a - 1] = districtMatrix[a - 1, i];  // Matrixte alt üçgendeki komşu ilçelerin değerlerini sonsuzla değişiyoruz.
                        }
                        else if (a - 1 > i)
                        {
                            infinityTableDistricts[i, a - 1] = districtMatrix[a - 1, i];  // Matrixte üst üçgendeki komşu ilçelerin değerlerini sonsuzla değişiyoruz.
                        }

                    }

                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File don't Found");
            }
        }

        private static void districtDistance(string[] districts, int[,] DijkstraDistricts, int[,] districtMatrix, ArrayList minValue, ArrayList maxValue)
        {
            StreamWriter sWDistrict = new StreamWriter("C:\\Users\\hp\\Desktop\\DistrictOutput.txt"); // Çıktılar TXT dosyasına kaydedilir. Dosya konumu ayarlanmalıdır.
            Console.WriteLine("Starting district:\tDestination district:\tDistance from highways:\tShortest way:\t(Shortest way - Distance from highways:)");
            sWDistrict.WriteLine("Starting district:\tDestination district:\tDistance from highways:\tShortest way:\t(Shortest way - Distance from highways:)"); 
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            sWDistrict.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            minValue.Clear();
            maxValue.Clear();
            for (int i = 0; i < districtMatrix.GetLength(0); i++)
            {

                for (int j = 0; j < i; j++)
                {
                    if (i == j) { continue; }
                    else
                    {
                        int difference = (DijkstraDistricts[i, j] - districtMatrix[i, j]);// Farklarını alır.
                        if (minValue.Count == 0 && maxValue.Count == 0)
                        {
                            //minValue.count ve maxValue.count 0 iken (yani Başlangıçta) ilk uzaklık her iki değere de atanır.
                            minValue.Add(difference);
                            minValue.Add(districts[i + 1]);
                            minValue.Add(districts[j + 1]);
                            maxValue.Add(difference);
                            maxValue.Add(districts[i + 1]);
                            maxValue.Add(districts[j + 1]);
                        }
                        else if (difference < (int)minValue[0])//Eğer minValue değeri o anki uzaklıktan büyükse, min value değeri silinir ve yeni değer atanır.
                        {
                            minValue.Clear();
                            minValue.Add(difference);
                            minValue.Add(districts[i + 1]);
                            minValue.Add(districts[j + 1]);
                        }
                        else if (difference == (int)minValue[0])// Yeni fark ile minValue eşitse, birden fazla minValue olusabileceği için karşılaşılan değer arrayliste eklenir. 
                        {
                            minValue.Add(difference);
                            minValue.Add(districts[i + 1]);
                            minValue.Add(districts[j + 1]);
                        }
                        if (difference > (int)maxValue[0])//Eğer maxValue değeri o anki uzaklıktan küçükse, maxValue değeri silinir ve yeni değer atanır.
                        {
                            maxValue.Clear();
                            maxValue.Add(difference);
                            maxValue.Add(districts[i + 1]);
                            maxValue.Add(districts[j + 1]);
                        }

                        else if (difference == (int)maxValue[0])// Yeni fark ile maxValue eşitse, birden fazla maxValue olusabileceği için  karşılaşılan değer arrayliste eklenir.
                        {
                            maxValue.Add(difference);
                            maxValue.Add(districts[i + 1]);
                            maxValue.Add(districts[j + 1]);
                        }
                        sWDistrict.Write($"{districts[i + 1],-20}\t{districts[j + 1],-20}\t{districtMatrix[i, j],-20}\t{DijkstraDistricts[i, j],-20}\t{difference,-20}");
                        Console.Write($"{districts[i + 1],-20}\t{districts[j + 1],-20}\t{districtMatrix[i, j],-20}\t{DijkstraDistricts[i, j],-20}\t{difference,-20}");
                        Console.WriteLine();
                        sWDistrict.WriteLine();
                    }

                }


            }
            sWDistrict.Close();
            Console.WriteLine();
            Console.WriteLine("The district(s) with the lowest difference:");
            int flagValue = 0;
            while (flagValue < minValue.Count)
            {
                Console.WriteLine($"Difference:{minValue[flagValue]}\t{minValue[flagValue + 1],-15}\t{minValue[flagValue + 2]}");
                flagValue += 3;// Yukarıda yaptığımız 3 tane ekleme işleminden sonra eklenmesi için flagValue değerini +=3 ten başlattık.
            }
            Console.WriteLine();
            flagValue = 0;
            Console.WriteLine("The district(s) with the highest difference:");
            while (flagValue < maxValue.Count)
            {
                Console.WriteLine($"Difference:{maxValue[flagValue]}\t{maxValue[flagValue + 1],-15}\t{maxValue[flagValue + 2]}");
                flagValue += 3;// Yukarıda yaptığımız 3 tane ekleme işleminden sonra eklenmesi için flagValue değerini +=3 ten başlattık.
            }
        }

    }
}
