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

    static int[,] MesafeMatrisi = new int[SehirSayisi, SehirSayisi]; // Şehirler arası mesafeleri tutan matris.

    static void Main()
    {
        MesafeMatrisiOlustur(); // Şehirler arası mesafeleri rastgele oluşturur.
        List<int[]> populasyon = BaslangicPopulasyonuOlustur(); // Başlangıç popülasyonunu oluşturur.
        int enKucuk = 0, count = 0; // En küçük mesafeyi ve durma koşulu için sayacı tutar.

        for (int nesil = 0; nesil < NesilSayisi; nesil++) // Belirtilen nesil sayısı kadar döngü.
        {
            populasyon = PopulasyonuGelistir(populasyon); // Popülasyonu geliştirir (çaprazlama ve mutasyon).

            var tempEniyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First(); // En iyi rotayı bulur.
            var hesaplanmisRota = RotaMesafesiHesapla(tempEniyiRota); // En iyi rotanın mesafesini hesaplar.

            if (nesil % 50 == 0) // Her 50 nesilde bir sonuçları yazdırır.
            {
                Console.WriteLine($"{nesil} Toplam Mesafe: {hesaplanmisRota}");
            }

            if (nesil == 0) // İlk nesilde en küçük mesafeyi kaydeder.
            {
                enKucuk = hesaplanmisRota;
            }

            if (enKucuk != hesaplanmisRota) // Daha iyi bir mesafe bulunduğunda sayaç sıfırlanır.
            {
                count = 0;
                enKucuk = hesaplanmisRota;
            }
            else 
            {
                count++; // Aynı mesafe tekrarlandığında sayaç artar.
            }

            if (count == 50) // 50 nesil boyunca aynı mesafe bulunduğunda algoritma durur.
            {
                return;
            }
        }

        Console.WriteLine($"--------------------------------------");
        var enIyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First(); // En iyi rotayı bulur.
        Console.WriteLine($"En iyi rota: {string.Join(" -> ", enIyiRota)}"); // En iyi rotayı yazdırır.
        Console.WriteLine($"Toplam Mesafe: {RotaMesafesiHesapla(enIyiRota)}"); // En iyi rotanın toplam mesafesini yazdırır.
        Console.ReadKey(); // Kullanıcıdan bir tuşa basmasını bekler.
    }

    static void MesafeMatrisiOlustur()
    {
        for (int i = 0; i < SehirSayisi; i++) // Şehirler arasında mesafeleri rastgele oluşturur.
        {
            for (int j = i + 1; j < SehirSayisi; j++)
            {
                int mesafe = rastgele.Next(10, 200); // 10 ile 200 arasında rastgele mesafe.
                MesafeMatrisi[i, j] = MesafeMatrisi[j, i] = mesafe; // Mesafeyi simetrik olarak atar.
            }
        }
    }

    static List<int[]> BaslangicPopulasyonuOlustur()
    {
        return Enumerable.Range(0, PopulasyonBoyutu) // Popülasyon boyutu kadar döngü.
                         .Select(_ => Enumerable.Range(0, SehirSayisi) // Şehirleri karıştırarak rastgele rotalar oluşturur.
                                                .OrderBy(_ => rastgele.Next())
                                                .ToArray())
                         .ToList();
    }

    static List<int[]> PopulasyonuGelistir(List<int[]> populasyon)
    {
        List<int[]> yeniPopulasyon = new List<int[]>(); // Yeni popülasyon listesi.
        var siraliPopulasyon = populasyon.OrderBy(RotaMesafesiHesapla).ToList(); // Popülasyonu mesafeye göre sıralar.

        for (int i = 0; i < PopulasyonBoyutu / 2; i++) // Popülasyonun yarısı kadar döngü.
        {
            int[] ebeveyn1 = siraliPopulasyon[i]; // İlk ebeveyni seçer.
            int[] ebeveyn2 = siraliPopulasyon.Skip(i + 1).FirstOrDefault(e => !e.SequenceEqual(ebeveyn1)) ?? siraliPopulasyon[i + 1]; // İkinci ebeveyni seçer.

            var (cocuk1, cocuk2) = Caprazla(ebeveyn1, ebeveyn2); // Çaprazlama ile iki çocuk oluşturur.

            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk1); } // Çocuk 1'e mutasyon uygular.
            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk2); } // Çocuk 2'ye mutasyon uygular.

            yeniPopulasyon.Add(cocuk1); // Çocuk 1'i yeni popülasyona ekler.
            yeniPopulasyon.Add(cocuk2); // Çocuk 2'yi yeni popülasyona ekler.
        }
        return yeniPopulasyon; // Yeni popülasyonu döndürür.
    }

    static (int[], int[]) Caprazla(int[] ebeveyn1, int[] ebeveyn2)
    {
        int kesmeNoktasi = rastgele.Next(1, SehirSayisi - 1); // Çaprazlama için kesme noktası belirler.
        int[] cocuk1 = ebeveyn1.Take(kesmeNoktasi).Concat(ebeveyn2.Except(ebeveyn1.Take(kesmeNoktasi))).ToArray(); // Çocuk 1'i oluşturur.
        int[] cocuk2 = ebeveyn2.Take(kesmeNoktasi).Concat(ebeveyn1.Except(ebeveyn2.Take(kesmeNoktasi))).ToArray(); // Çocuk 2'yi oluşturur.
        return (cocuk1, cocuk2); // Çocukları döndürür.
    }

    static void MutasyonUygula(int[] rota)
    {
        int i = rastgele.Next(SehirSayisi); // Rastgele bir şehir seçer.
        int j = rastgele.Next(SehirSayisi); // Başka bir rastgele şehir seçer.
        (rota[i], rota[j]) = (rota[j], rota[i]); // İki şehri yer değiştirerek mutasyon uygular.
    }

    static int RotaMesafesiHesapla(int[] rota)
    {
        int mesafe = 0; // Toplam mesafeyi tutar.
        for (int i = 0; i < SehirSayisi - 1; i++) // Rotadaki şehirler arasındaki mesafeleri toplar.
        {
            mesafe += MesafeMatrisi[rota[i], rota[i + 1]];
        }
        mesafe += MesafeMatrisi[rota[SehirSayisi - 1], rota[0]]; // Başlangıç noktasına dönüş mesafesini ekler.
        return mesafe; // Toplam mesafeyi döndürür.
    }
}
