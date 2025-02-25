using System;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Projeversion1
{
    public class NeuronCell
    {
        int[,] inputs;
        double[] weights = new double[25];  // Ağırlıkları tutan dizidir.

        static Random random = new Random();
        public NeuronCell()     // Boş NeuronCell yapıcı metot oluşturulur.
        {
            this.inputs = null;
        }

        public void setInput(int[,] input)  // Input değerine ulaşmayı sağlayan metottur.
        {
            this.inputs = input;
        }
        public void RandomWeights(double[] weight)  // Rastgele 1-0 arasında değerleri oluşturan metottur.
        {
            for (int i = 0; i < weight.Length; i++)
            {
                weights[i] = random.NextDouble();
            }
        }
        public double[] GetRandomWeightS()      //  weights arrayine erişilmesini sağlayan metottur.
        {
            return weights;
        }
        public double CalculateOutput(int[,] input)     // Neuron çıktısını hesaplayan metottur.
        {
            double sum1 = 0;
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    sum1 += weights[(5 * i) + j] * input[i, j];     // yeni bir satıra geçtimesi için 5 değeri ile çarpılmıştır i değişkeni.
                }
            }
            return sum1;

        }
        public void RiseWeights(float delta, int[,] input)      // Ağırlıkların değerinin arttıran metottur.
        {
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    weights[5 * i + j] += (delta * (int)input[i, j]);// Ağırlığın yeni bir satıra geçtimesi için 5 değeri ile çarpılmıştır i değişkeni.
                }
            }
        }
        public void LowerWeights(float delta, int[,] input)     // Ağırlıkların değerinin azaltan metottur.
        {
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    weights[5 * i + j] -= delta * (int)input[i, j];// Ağırlığın yeni bir satıra geçtimesi için 5 değeri ile çarpılmıştır i değişkeni.
                }
            }
        }
    }
    public class NeuralNetwork
    {

        public NeuronCell cell1;
        public NeuronCell cell2;
        public float delta;
        public int[] image;
        public NeuralNetwork(NeuronCell celll1, NeuronCell celll2, float delta, int[] image)    // Parametreli NeuralNetwork yapıcı metod oluşturulur.
        {
            this.cell1 = celll1;
            this.cell2 = celll2;
            this.delta = delta;
            this.image = null;
        }
        public void RandomGenerator(double[] weight1, double[] weight2)     // Her bir hücre için kendilerine özel ağırlık oluşturulur.
        {
            this.cell1.RandomWeights(weight1);
            this.cell2.RandomWeights(weight2);
        }
        public void trainImage(float delta, int[,] input, int imageindex, float deviation) // Eğitim verisi ile nöronların eğitilmesi sağlanır
        {
            double result1 = cell1.CalculateOutput(input);
            double result2 = cell2.CalculateOutput(input);
            if (imageindex == 1)    // Resim 1 ise nöron1'i 1 değerine nöron2'i 0 değerine yaklaştırır.
            {                       //Herzaman Bu işlemin uygulanmaması için bir sapma miktarı vardır.
                if (result1 < 1 - deviation)
                {
                    cell1.RiseWeights(delta, input);
                }
                else if (result1 > 1 + deviation)
                {
                    cell1.LowerWeights(delta, input);
                }
                if (result2 > 0 + deviation)
                {
                    cell2.LowerWeights(delta, input);
                }
                else if (result2 < 0 + deviation)
                {
                    cell2.RiseWeights(delta, input);
                }
            }
            else     // Resim 2 ise nöron2'i 1 değerine nöron1'i 0 değerine yaklaştırır. 
            {       //Herzaman Bu işlemin uygulanmaması için bir sapma miktarı vardır.
                if (result2 < 1 - deviation)
                {
                    cell2.RiseWeights(delta, input);
                }
                else if (result2 > 1 + deviation)
                {
                    cell2.LowerWeights(delta, input);
                }
                if (result1 > 0 + deviation)
                {
                    cell1.LowerWeights(delta, input);
                }
                else if (result1 < 0 + deviation)
                {
                    cell1.RiseWeights(delta, input);
                }
            }
        }
        public int TestImage(int[,] input, int imageindex, float deviation) // Test verileri ile nöronların testi yapılır.
        {
            double result1 = 0;
            double result2 = 0;
            result1 = cell1.CalculateOutput(input);
            result2 = cell2.CalculateOutput(input);
            if (imageindex == 1)
            {
                if (result1 - result2 > 0)  // Nöron1 daha büyük çıkarsa resimde 1 şekli vardır.
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (result1 - result2 < 0)  // Nöron2 daha büyük çıkarsa resimde 1 şekli vardır.
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }

        }
        public int DetectImage(int[,] input, float deviation)  // Hiç girilmemiş veri setinin tahmini yapılır.
        {
            double result1 = 0;
            double result2 = 0;
            result1 = cell1.CalculateOutput(input);
            result2 = cell2.CalculateOutput(input);
            if (result1 - result2 > 0)      // Nöron1 daha büyük çıkarsa resimde 1 şekli vardır.
            {
                return 1;
            }
            else if (result1 - result2 < 0) // Nöron2 daha büyük çıkarsa resimde 1 şekli vardır.
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

    }
    internal class Program
    {
        static Random random = new Random();
        static void Main(string[] args)
        {
            Object[] data = new Object[20];
            ArrayList uniquePositions = new ArrayList();

            CreateImage(data, uniquePositions);     // Resim veri seti oluşturulur.

            int[] image = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };   // Veri setimizdeki verilerin etiketidir.

            int sayac = 0;  // image dizisinde gezilmeyi sağlar.
            float learningCoefficient = 0.003f;   //Öğrenme katsayısıdır
            float deviation = 0.1f; // Sapma miktarıdır.
            int epoch = 40;     // Tur sayısıdır.
            double[] weightt1 = new double[25];
            double[] weightt2 = new double[25];
            NeuralNetwork neuralNetwork = new NeuralNetwork(new NeuronCell(), new NeuronCell(), learningCoefficient, image);

            neuralNetwork.RandomGenerator(weightt1, weightt2);  // Nöronların rastgele ağırlıkları oluşturulur 

            for (int i = 0; i < epoch; i++)     // Nöral ağımızın eğitimi yapılır.
            {
                foreach (int[,] datum in data)
                {
                    neuralNetwork.cell1.setInput(datum);
                    neuralNetwork.cell2.setInput(datum);
                    neuralNetwork.trainImage(learningCoefficient, datum, image[sayac], deviation);
                    sayac++;
                }
                sayac = 0;

            }
            int sayac2 = 0;
            int correct_prediction = 0;
            Console.WriteLine("Data NO: \tTarget Value:\tEstimated value:");
            foreach (int[,] datum in data)  // Eğitim Sonucu test aşamasındaki başarısı çıktı olarak verilir.
            {
                neuralNetwork.cell1.setInput(datum);
                neuralNetwork.cell2.setInput(datum);
                int predictionValue = neuralNetwork.TestImage(datum, image[sayac2], deviation);
                if (image[sayac2] == predictionValue)
                {
                    correct_prediction += 1;
                }
                Console.WriteLine($"{sayac2 + 1}\t\t{image[sayac2]}\t\t{predictionValue}");
                sayac2++;

            }
            Console.WriteLine("Predic Rate: %" + (correct_prediction * 100 / 20));

            int[,] newdata1 = { { 1, 0, 0, 1, 1 }, { 1, 0, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 1, 1, 0, 1, 1 } };    // Yeni veri setidir.
            int[,] newdata2 = { { 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 0 }, { 1, 1, 1, 0, 1 }, { 1, 1, 0, 1, 1 }, { 1, 0, 0, 0, 0 } };    // Yeni veri setidir.
            int[,] newdata3 = { { 1, 1, 0, 1, 1 }, { 1, 0, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 0, 0, 0, 0, 0 } };    // Yeni veri setidir.
            Console.WriteLine("New Data:");
            Console.WriteLine($"Data 1: Expected: 1 Estimated {neuralNetwork.DetectImage(newdata1, deviation)}");   //Yeni veri setinin tespitidir.
            Console.WriteLine($"Data 2: Expected: 2 Estimated {neuralNetwork.DetectImage(newdata2, deviation)}");   // Yeni veri setinin tespitidir.
            Console.WriteLine($"Data 3: Expected: 1 Estimated {neuralNetwork.DetectImage(newdata3, deviation)}");   // Yeni veri setinin tespitidir.
            Console.ReadKey();
        }
        private static void CreateImage(object[] data1, ArrayList uniquePositions)
        {

            for (int j = 0; j < 10; j++)    // Onar tane verinin taslak hali oluşturulur.
            {
                data1[j] = new int[,] { { 1, 1, 0, 1, 1 }, { 1, 0, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 1, 1, 0, 1, 1 }, { 1, 1, 1, 1, 1 } };
                data1[j + 10] = new int[,] { { 1, 0, 0, 0, 1 }, { 0, 1, 1, 1, 0 }, { 1, 1, 1, 0, 1 }, { 1, 1, 0, 1, 1 }, { 0, 0, 0, 0, 0 } };
            }
            for (int i = 0; i < 5; i++)     // 1 şeklinin en altına sırasıyla 1 tane doldurma işlemi yapar.
            {
                int[,] fillArray = (int[,])data1[i];
                fillArray[4, i] = 0;
            }
            for (int k = 0; k < 4; k++)     // 1 şeklinin en altına sırasıyla 2 tane doldurma işlemi yapar.
            {
                int[,] fillArray = (int[,])data1[k + 5];
                for (int i = k; i < k + 2; i++)
                {
                    fillArray[4, i] = 0;
                }
            }

            int randomNumber;
            for (int i = 10; i < 20; i++)       // 2 şeklinde işaretlenmemiş bir bölgeyi rastgele şekilde işaretler.
            {
                randomNumber = random.Next(1, 25);
                int[,] fillArray = (int[,])data1[i];
                do
                {       // Aynı index değerinin gelmemesi için bir while döngüsüne girer.
                    randomNumber = random.Next(1, 25);
                }
                while (fillArray[randomNumber / 5, randomNumber % 5] == 0 || uniquePositions.Contains(randomNumber));
                uniquePositions.Add(randomNumber);
                fillArray[randomNumber / 5, randomNumber % 5] = 0;
            }
            foreach (int[,] data in data1)  // Veri setinin ekrana çıktısını döndürür. 
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        Console.Write(data[i, j] + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("----------------------");

            }
        }
    }
}


