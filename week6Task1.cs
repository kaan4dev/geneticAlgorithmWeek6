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

    static int[,] MesafeMatrisi = new int[SehirSayisi, SehirSayisi]; // şehirler arası mesafeleri tutan matris

    static void Main()
    {
        MesafeMatrisiOlustur(); // şehirler arası mesafeleri rastgele oluştur
        List<int[]> populasyon = BaslangicPopulasyonuOlustur(); // başlangıç popülasyonunu oluştur
        int enKucuk = int.MaxValue; // en küçük mesafeyi başta büyük bir değere atıyoruz
        int enIyiNesil = 0; // en iyi çözümü hangi nesilde bulduğumuzu tutuyoruz
        int count = 0; // durma koşulu için sayaç
        int[] enIyiRota = null; // en iyi rotayı tutmak için değişken
        
        for (int nesil = 0; nesil < NesilSayisi; nesil++) // belirtilen nesil sayısı kadar döngü
        {
            populasyon = PopulasyonuGelistir(populasyon); // popülasyonu geliştir (çaprazlama ve mutasyon)

            var tempEniyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First(); // en iyi rotayı buluyoruz
            var hesaplanmisRota = RotaMesafesiHesapla(tempEniyiRota); // en iyi rotanın mesafesini hesaplıyoruz

            if (hesaplanmisRota < enKucuk) // daha iyi bir mesafe bulduğunda
            {
                enKucuk = hesaplanmisRota; // yeni en kısa mesafeyi kaydediyoruz
                enIyiNesil = nesil; // en iyi nesili güncelliyoruz
                enIyiRota = tempEniyiRota; // en iyi rotayı güncelliyoruz
            }

            if (nesil % 50 == 0) // her 50 nesilde bir sonucu yazdırıyoruz
            {
                Console.WriteLine($"{nesil} toplam mesafe: {hesaplanmisRota}");
            }

            if (enKucuk != hesaplanmisRota) // daha iyi bir mesafe bulunduğunda sayaç sıfırlanıyor
            {
                count = 0;
            }
            else
            {
                count++; // aynı mesafe tekrarlandığında sayaç artıyor
            }

            if (count == 50) // 50 nesil boyunca aynı mesafe bulunduğunda algoritma duruyor
            {
                break; // algoritmayı sonlandırıyoruz
            }
        }

        // Son iterasyondaki en iyi bireyi al
        var sonNesilEnIyiRota = populasyon.OrderBy(RotaMesafesiHesapla).First();
        var sonNesilEnIyiMesafe = RotaMesafesiHesapla(sonNesilEnIyiRota);

        // Eğer son iterasyondaki en iyi birey, genel en iyi bireyden daha iyiyse, onu kullan
        if (sonNesilEnIyiMesafe < enKucuk)
        {
            enKucuk = sonNesilEnIyiMesafe;
            enIyiRota = sonNesilEnIyiRota;
        }

        // en iyi çözümü bulduktan sonra:
        Console.WriteLine($"--------------------------------------");
        Console.WriteLine($"en iyi rota: {string.Join(" -> ", enIyiRota)}"); // en iyi rotayı yazdırıyoruz
        Console.WriteLine($"toplam mesafe: {enKucuk}"); // en iyi rotanın toplam mesafesini yazdırıyoruz
        Console.WriteLine($"en iyi çözüm {enIyiNesil}. nesilde bulundu"); // en iyi çözümün bulunduğu nesili yazdırıyoruz
        Console.ReadKey(); // kullanıcıdan bir tuşa basmasını bekliyoruz
    }

    static void MesafeMatrisiOlustur()
    {
        for (int i = 0; i < SehirSayisi; i++) // şehirler arasında mesafeleri rastgele oluşturuyoruz
        {
            for (int j = i + 1; j < SehirSayisi; j++)
            {
                int mesafe = rastgele.Next(10, 200); // 10 ile 200 arasında rastgele mesafe
                MesafeMatrisi[i, j] = MesafeMatrisi[j, i] = mesafe; // mesafeyi simetrik olarak atıyoruz
            }
        }
    }

    static List<int[]> BaslangicPopulasyonuOlustur()
    {
        return Enumerable.Range(0, PopulasyonBoyutu) // popülasyon boyutu kadar döngü
                         .Select(_ => Enumerable.Range(0, SehirSayisi) // şehirleri karıştırarak rastgele rotalar oluşturuyoruz
                                .OrderBy(_ => rastgele.Next())
                                .ToArray())
                         .ToList();
    }

    static List<int[]> PopulasyonuGelistir(List<int[]> populasyon)
    {
        List<int[]> yeniPopulasyon = new List<int[]>(); // yeni popülasyon listesi
        var siraliPopulasyon = populasyon.OrderBy(RotaMesafesiHesapla).ToList(); // popülasyonu mesafeye göre sıralıyoruz

        for (int i = 0; i < PopulasyonBoyutu / 2; i++) // popülasyonun yarısı kadar döngü
        {
            int[] ebeveyn1 = siraliPopulasyon[i]; // ilk ebeveyni seçiyoruz
            int[] ebeveyn2 = siraliPopulasyon.Skip(i + 1).FirstOrDefault(e => !e.SequenceEqual(ebeveyn1)) ?? siraliPopulasyon[i + 1]; // ikinci ebeveyni seçiyoruz

            var (cocuk1, cocuk2) = Caprazla(ebeveyn1, ebeveyn2); // çaprazlama ile iki çocuk oluşturuyoruz

            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk1); } // çocuk 1'e mutasyon uyguluyoruz
            if (rastgele.NextDouble() < MutasyonOrani) { MutasyonUygula(cocuk2); } // çocuk 2'ye mutasyon uyguluyoruz

            yeniPopulasyon.Add(cocuk1); // çocuk 1'i yeni popülasyona ekliyoruz
            yeniPopulasyon.Add(cocuk2); // çocuk 2'yi yeni popülasyona ekliyoruz
        }
        return yeniPopulasyon; // yeni popülasyonu döndürüyoruz
    }

    static (int[], int[]) Caprazla(int[] ebeveyn1, int[] ebeveyn2)
    {
        int kesmeNoktasi = rastgele.Next(1, SehirSayisi - 1); // çaprazlama için kesme noktası belirliyoruz
        int[] cocuk1 = ebeveyn1.Take(kesmeNoktasi).Concat(ebeveyn2.Except(ebeveyn1.Take(kesmeNoktasi))).ToArray(); // çocuk 1'i oluşturuyoruz
        int[] cocuk2 = ebeveyn2.Take(kesmeNoktasi).Concat(ebeveyn1.Except(ebeveyn2.Take(kesmeNoktasi))).ToArray(); // çocuk 2'yi oluşturuyoruz
        return (cocuk1, cocuk2); // çocukları döndürüyoruz
    }

    static void MutasyonUygula(int[] rota)
    {
        int i = rastgele.Next(SehirSayisi); // rastgele bir şehir seçiyoruz
        int j = rastgele.Next(SehirSayisi); // başka bir rastgele şehir seçiyoruz
        (rota[i], rota[j]) = (rota[j], rota[i]); // iki şehri yer değiştirerek mutasyon uyguluyoruz
    }

    static int RotaMesafesiHesapla(int[] rota)
    {
        int mesafe = 0; // toplam mesafeyi tutuyoruz
        for (int i = 0; i < SehirSayisi - 1; i++) // rotadaki şehirler arasındaki mesafeleri topluyoruz
        {
            mesafe += MesafeMatrisi[rota[i], rota[i + 1]];
        }
        mesafe += MesafeMatrisi[rota[SehirSayisi - 1], rota[0]]; // başlangıç noktasına dönüş mesafesini ekliyoruz
        return mesafe; // toplam mesafeyi döndürüyoruz
    }
}