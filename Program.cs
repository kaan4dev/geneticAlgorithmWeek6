using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static Random rastgele = new Random();
    const int SehirSayisi = 1000;
    const int PopulasyonBoyutu = 500;
    const int NesilSayisi = 10000;
    const double MutasyonOrani = 0.5;

    //MesafeMatrisiOlustur metod ile birlikte kullanılan dinamik mesafe oluşturma
    static int[,] MesafeMatrisi = new int[SehirSayisi, SehirSayisi];

    //Manuel oluşturulmuş mesafeler
    //static readonly int[,] MesafeMatrisi = {
    //    { 0, 29, 20, 21, 16, 31, 100, 12, 4, 31 },
    //    { 29, 0, 15, 29, 28, 40, 72, 21, 29, 41 },
    //    { 20, 15, 0, 15, 14, 25, 81, 9, 23, 27 },
    //    { 21, 29, 15, 0, 4, 12, 92, 12, 25, 13 },
    //    { 16, 28, 14, 4, 0, 16, 94, 9, 20, 16 },
    //    { 31, 40, 25, 12, 16, 0, 95, 24, 36, 3 },
    //    { 100, 72, 81, 92, 94, 95, 0, 90, 101, 99 },
    //    { 12, 21, 9, 12, 9, 24, 90, 0, 15, 25 },
    //    { 4, 29, 23, 25, 20, 36, 101, 15, 0, 35 },
    //    { 31, 41, 27, 13, 16, 3, 99, 25, 35, 0 }
    //};
    static void Main()
    {
        MesafeMatrisiOlustur();
        List<int[]> populasyon = BaslangicPopulasyonuOlustur();
        int enKucuk = 0, count = 0;
        for (int nesil = 0; nesil < NesilSayisi; nesil++)
        {
            populasyon = PopulasyonuGelistir(populasyon);

            var tempEniyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First();
            var hesaplanmisRota = RotaMesafesiHesapla(tempEniyiRota);

            if (nesil % 50 == 0)
            {
                Console.WriteLine($"{nesil} Toplam Mesafe: {hesaplanmisRota}");
            }
            


            if (nesil == 0)
            {
                enKucuk = hesaplanmisRota;
            }

            if (enKucuk != hesaplanmisRota)
            {
                count = 0;
                enKucuk = hesaplanmisRota;
            }
            else 
            {
                count++;
            }

            if (count == 50)
            {
                return;
            }

        }

        Console.WriteLine($"--------------------------------------");
        var enIyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First();
        Console.WriteLine($"En iyi rota: {string.Join(" -> ", enIyiRota)}");
        Console.WriteLine($"Toplam Mesafe: {RotaMesafesiHesapla(enIyiRota)}");
        Console.ReadKey();  
    }

    static void MesafeMatrisiOlustur()
    {
        for (int i = 0; i < SehirSayisi; i++)
        {
            for (int j = i + 1; j < SehirSayisi; j++)
            {
                int mesafe = rastgele.Next(10, 200);
                MesafeMatrisi[i, j] = MesafeMatrisi[j, i] = mesafe;
            }
        }
    }

    static List<int[]> BaslangicPopulasyonuOlustur()
    {
        return Enumerable.Range(0, PopulasyonBoyutu)
                         .Select(_ => Enumerable.Range(0, SehirSayisi)
                                                .OrderBy(_ => rastgele.Next())
                                                .ToArray())
                         .ToList();
       
    }

    static List<int[]> PopulasyonuGelistir(List<int[]> populasyon)
    {
        List<int[]> yeniPopulasyon = new List<int[]>();
        var siraliPopulasyon = populasyon.OrderBy(RotaMesafesiHesapla).ToList();

        for (int i = 0; i < PopulasyonBoyutu / 2; i++)
        {
            int[] ebeveyn1 = siraliPopulasyon[i];
            int[] ebeveyn2 = siraliPopulasyon.Skip(i + 1).FirstOrDefault(e => !e.SequenceEqual(ebeveyn1)) ?? siraliPopulasyon[i + 1];

            var (cocuk1, cocuk2) = Caprazla(ebeveyn1, ebeveyn2);

            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk1); } 
            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk2); } 

            yeniPopulasyon.Add(cocuk1);
            yeniPopulasyon.Add(cocuk2);
        }
        return yeniPopulasyon;
    }

    static (int[], int[]) Caprazla(int[] ebeveyn1, int[] ebeveyn2)
    {
        /*
         * ebeveyn1.Take(kesmeNoktasi).Concat(ebeveyn2.Except(ebeveyn1.Take(kesmeNoktasi))).ToArray(); AÇIKLAMASI:
ebeveyn1.Take(kesmeNoktasi) // ebeveyn1 dizisinin ilk 'kesmeNoktasi' kadar öğesini al
.Concat( // Bu alınan kısmı, ebeveyn2 dizisinin belirli öğeleriyle birleştir
ebeveyn2.Except(ebeveyn1.Take(kesmeNoktasi)) // ebeveyn2 içinden, ebeveyn1’in ilk 'kesmeNoktasi' kadar olan elemanlarını hariç tutarak geri kalanları ekle
).ToArray(); // Sonucu bir diziye çevir

         * */
        int kesmeNoktasi = rastgele.Next(1, SehirSayisi - 1);
        int[] cocuk1 = ebeveyn1.Take(kesmeNoktasi).Concat(ebeveyn2.Except(ebeveyn1.Take(kesmeNoktasi))).ToArray();
        int[] cocuk2 = ebeveyn2.Take(kesmeNoktasi).Concat(ebeveyn1.Except(ebeveyn2.Take(kesmeNoktasi))).ToArray();
        return (cocuk1, cocuk2);
    }
    /*
     2,4,5,7,9
    except
   1,2,5,6,8

        sonuc:
        4,7,9

        sonuÇ:
        1,6,8
    */
    static void MutasyonUygula(int[] rota)
    {
        int i = rastgele.Next(SehirSayisi);
        int j = rastgele.Next(SehirSayisi);
        (rota[i], rota[j]) = (rota[j], rota[i]);
    }

    static int RotaMesafesiHesapla(int[] rota)
    {
        int mesafe = 0;
        for (int i = 0; i < SehirSayisi - 1; i++)
        {
            mesafe += MesafeMatrisi[rota[i], rota[i + 1]];
        }
        mesafe += MesafeMatrisi[rota[SehirSayisi - 1], rota[0]]; // Başlangıç noktasına dönüş mesafesi
        return mesafe;
    }
}
