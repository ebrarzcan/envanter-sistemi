using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OgrenciKutuphaneSistemi;

class Program
{
    // Veri dosyalari (program ile ayni klasorde olusur)
    static string kitapDosyasi = "kitaplar.txt";
    static string kullaniciDosyasi = "kullanicilar.txt";
    static string oduncDosyasi = "odunc.txt";

    static void Main()
    {
        VarsayilanYoneticiOlustur();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== OGRENCI KUTUPHANE SISTEMI =====");
            Console.WriteLine("1) Giris Yap");
            Console.WriteLine("2) Kayit Ol (Ogrenci)");
            Console.WriteLine("0) Cikis");
            Console.Write("Seciminiz: ");
            string secim = Console.ReadLine();

            if (secim == "1")
            {
                Kullanici k = GirisYap();
                if (k != null)
                {
                    AnaMenu(k);
                }
            }
            else if (secim == "2")
            {
                KendiKendineKayitOl();
            }
            else if (secim == "0")
            {
                Console.WriteLine("Gorusmek uzere!");
                break;
            }
            else
            {
                Console.WriteLine("Gecersiz secim!");
            }
        }
    }

    // Hic kullanici yoksa varsayilan yonetici hesabini olustur
    static void VarsayilanYoneticiOlustur()
    {
        if (!File.Exists(kullaniciDosyasi))
        {
            string hash = SifreHashle("1234");
            File.WriteAllText(kullaniciDosyasi, "admin|" + hash + "|Sistem Yoneticisi|Yonetici\n");
            Console.WriteLine("Bilgi: varsayilan yonetici hesabi olusturuldu -> admin / 1234");
        }
    }

    // Sifreyi SHA256 ile hashler ve hex (onaltilik) metin doner.
    // Boylece dosyada duz sifre yerine 64 karakterlik hash saklanir.
    static string SifreHashle(string sifre)
    {
        SHA256 sha = SHA256.Create();
        byte[] bayt = Encoding.UTF8.GetBytes(sifre);
        byte[] hash = sha.ComputeHash(bayt);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }

    // ========== GIRIS ==========

    static Kullanici GirisYap()
    {
        Console.Write("Kullanici Adi: ");
        string ad = Console.ReadLine();
        Console.Write("Sifre: ");
        string sifre = Console.ReadLine();
        string sifreHash = SifreHashle(sifre);

        if (!File.Exists(kullaniciDosyasi))
        {
            Console.WriteLine("Kullanici dosyasi bulunamadi!");
            return null;
        }

        string[] satirlar = File.ReadAllLines(kullaniciDosyasi);
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            Kullanici k = Kullanici.SatirdanOku(satirlar[i]);
            if (k.KullaniciAdi == ad && k.Sifre == sifreHash)
            {
                Console.WriteLine("Hosgeldiniz, " + k.AdSoyad + " (Rol: " + k.Rol + ")");
                return k;
            }
        }

        Console.WriteLine("Gecersiz kullanici adi veya sifre!");
        return null;
    }

    // ========== KAYIT OL (SELF-REGISTRATION) ==========

    // Yeni ogrenci kendi kendine hesap acabilir.
    // Guvenlik icin sadece "Ogrenci" rolu verilir; Yonetici/Personel
    // hesaplari yalnizca mevcut bir yonetici tarafindan olusturulabilir.
    static void KendiKendineKayitOl()
    {
        Console.WriteLine();
        Console.WriteLine("--- YENI OGRENCI KAYDI ---");
        Console.Write("Kullanici Adi: ");
        string ad = Console.ReadLine();
        Console.Write("Sifre: ");
        string sifre = Console.ReadLine();
        Console.Write("Ad Soyad: ");
        string adSoyad = Console.ReadLine();

        // Bos giris kontrolu
        if (ad == null || ad.Trim() == "" ||
            sifre == null || sifre == "" ||
            adSoyad == null || adSoyad.Trim() == "")
        {
            Console.WriteLine("Tum alanlari doldurmaniz gerekir!");
            return;
        }

        // Kullanici adinda dosya bicimini bozacak karakter olmasin
        if (ad.Contains("|") || adSoyad.Contains("|"))
        {
            Console.WriteLine("Kullanici adi veya ad soyad '|' karakteri iceremez.");
            return;
        }

        // Bu kullanici adi zaten var mi?
        if (File.Exists(kullaniciDosyasi))
        {
            string[] mevcut = File.ReadAllLines(kullaniciDosyasi);
            for (int i = 0; i < mevcut.Length; i++)
            {
                if (mevcut[i].Trim() == "") continue;
                Kullanici varolan = Kullanici.SatirdanOku(mevcut[i]);
                if (varolan.KullaniciAdi == ad)
                {
                    Console.WriteLine("Bu kullanici adi zaten kullaniliyor!");
                    return;
                }
            }
        }

        Kullanici k = new Kullanici();
        k.KullaniciAdi = ad;
        k.Sifre = SifreHashle(sifre);
        k.AdSoyad = adSoyad;
        k.Rol = "Ogrenci"; // self-registration her zaman Ogrenci olur

        File.AppendAllText(kullaniciDosyasi, k.DosyaSatiri() + "\n");
        Console.WriteLine("Kayit basarili! Artik 'Giris Yap' secenegi ile sisteme girebilirsiniz.");
    }

    // ========== ANA MENU ==========

    static void AnaMenu(Kullanici kullanici)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== ANA MENU (" + kullanici.KullaniciAdi + " / " + kullanici.Rol + ") =====");
            Console.WriteLine("1) Kitaplari Listele (Alfabetik)");
            Console.WriteLine("2) Kitap Ara");

            if (kullanici.Rol == "Yonetici" || kullanici.Rol == "Personel")
            {
                Console.WriteLine("3) Kitap Ekle");
                Console.WriteLine("4) Kitap Sil");
            }

            Console.WriteLine("5) Odunc Al");
            Console.WriteLine("6) Kitap Iade Et");
            Console.WriteLine("7) Odunc Aldigim Kitaplar");

            if (kullanici.Rol == "Yonetici")
            {
                Console.WriteLine("8) Yeni Kullanici Ekle");
                Console.WriteLine("9) Raporlar ve Istatistikler");
                Console.WriteLine("10) Stokta Olmayan Kitaplar");
            }

            Console.WriteLine("0) Cikis Yap");
            Console.Write("Seciminiz: ");
            string secim = Console.ReadLine();

            if (secim == "1")
            {
                KitaplariListele();
            }
            else if (secim == "2")
            {
                KitapAra();
            }
            else if (secim == "3" && (kullanici.Rol == "Yonetici" || kullanici.Rol == "Personel"))
            {
                KitapEkle();
            }
            else if (secim == "4" && (kullanici.Rol == "Yonetici" || kullanici.Rol == "Personel"))
            {
                KitapSil();
            }
            else if (secim == "5")
            {
                OduncAl(kullanici);
            }
            else if (secim == "6")
            {
                KitapIade(kullanici);
            }
            else if (secim == "7")
            {
                BenimOduncIslemlerim(kullanici);
            }
            else if (secim == "8" && kullanici.Rol == "Yonetici")
            {
                KullaniciEkle();
            }
            else if (secim == "9" && kullanici.Rol == "Yonetici")
            {
                RaporlariGoster();
            }
            else if (secim == "10" && kullanici.Rol == "Yonetici")
{
    StoktaOlmayanKitaplar();
}
            else if (secim == "0")
            {
                Console.WriteLine("Cikis yapildi.");
                break;
            }
            else
            {
                Console.WriteLine("Gecersiz secim veya bu islem icin yetkiniz yok!");
            }
        }
    }

    // ========== BAGLI LISTE & SIRALAMA ==========

    // Dosyadaki tum kitaplari bagli listeye yukler
    static KitapBagliListe KitaplariYukle()
    {
        KitapBagliListe liste = new KitapBagliListe();
        if (!File.Exists(kitapDosyasi)) return liste;

        string[] satirlar = File.ReadAllLines(kitapDosyasi);
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            liste.Ekle(Kitap.SatirdanOku(satirlar[i]));
        }
        return liste;
    }

    // Akademik gereklilik: Bubble Sort ile kitaplari basliga gore alfabetik siralar
    static void KitapBubbleSort(Kitap[] dizi)
    {
        int n = dizi.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                int kiyas = string.Compare(dizi[j].Baslik, dizi[j + 1].Baslik, StringComparison.OrdinalIgnoreCase);
                if (kiyas > 0)
                {
                    Kitap gecici = dizi[j];
                    dizi[j] = dizi[j + 1];
                    dizi[j + 1] = gecici;
                }
            }
        }
    }

    // ========== KITAP ISLEMLERI ==========

    static void KitaplariListele()
    {
        KitapBagliListe liste = KitaplariYukle();
        Kitap[] dizi = liste.DiziyeCevir();
        KitapBubbleSort(dizi);

        Console.WriteLine();
        Console.WriteLine("--- KITAP LISTESI (Alfabetik) ---");
        if (dizi.Length == 0)
        {
            Console.WriteLine("Henuz kitap eklenmemis.");
            return;
        }

        for (int i = 0; i < dizi.Length; i++)
        {
            Console.WriteLine(
                "ID:" + dizi[i].Id +
                " | " + dizi[i].Baslik +
                " / " + dizi[i].Yazar +
                " (" + dizi[i].Yil + ")" +
                " | Stok: " + dizi[i].Adet);
        }
    }

    static void KitapAra()
    {
        Console.Write("Aranacak kelime (baslik veya yazar): ");
        string kelime = Console.ReadLine();
        if (kelime == null) kelime = "";
        kelime = kelime.ToLower();

        KitapBagliListe liste = KitaplariYukle();
        Kitap[] dizi = liste.DiziyeCevir();

        Console.WriteLine();
        Console.WriteLine("--- ARAMA SONUCLARI ---");
        int bulunan = 0;
        for (int i = 0; i < dizi.Length; i++)
        {
            string baslik = dizi[i].Baslik.ToLower();
            string yazar = dizi[i].Yazar.ToLower();
            if (baslik.Contains(kelime) || yazar.Contains(kelime))
            {
                Console.WriteLine("ID:" + dizi[i].Id + " | " + dizi[i].Baslik + " / " + dizi[i].Yazar + " | Stok: " + dizi[i].Adet);
                bulunan = bulunan + 1;
            }
        }
        if (bulunan == 0) Console.WriteLine("Sonuc bulunamadi.");
    }

    static int SonrakiKitapId()
    {
        KitapBagliListe liste = KitaplariYukle();
        Kitap[] dizi = liste.DiziyeCevir();
        int enBuyuk = 0;
        for (int i = 0; i < dizi.Length; i++)
        {
            if (dizi[i].Id > enBuyuk) enBuyuk = dizi[i].Id;
        }
        return enBuyuk + 1;
    }

    static void KitapEkle()
    {
        Console.WriteLine();
        Console.WriteLine("--- YENI KITAP ---");
        Console.Write("Baslik: ");
        string baslik = Console.ReadLine();
        Console.Write("Yazar: ");
        string yazar = Console.ReadLine();
        Console.Write("Yil: ");
        int yil = int.Parse(Console.ReadLine());
        Console.Write("Adet: ");
        int adet = int.Parse(Console.ReadLine());

        Kitap k = new Kitap();
        k.Id = SonrakiKitapId();
        k.Baslik = baslik;
        k.Yazar = yazar;
        k.Yil = yil;
        k.Adet = adet;

        File.AppendAllText(kitapDosyasi, k.DosyaSatiri() + "\n");
        Console.WriteLine("Kitap eklendi. (ID: " + k.Id + ")");
    }

    static void KitapSil()
    {
        Console.Write("Silinecek kitap ID: ");
        int id = int.Parse(Console.ReadLine());

        if (!File.Exists(kitapDosyasi))
        {
            Console.WriteLine("Kitap dosyasi yok.");
            return;
        }

        string[] satirlar = File.ReadAllLines(kitapDosyasi);
        List<string> yeniSatirlar = new List<string>();
        bool silindi = false;
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            Kitap k = Kitap.SatirdanOku(satirlar[i]);
            if (k.Id == id)
            {
                silindi = true;
                continue;
            }
            yeniSatirlar.Add(satirlar[i]);
        }

        File.WriteAllLines(kitapDosyasi, yeniSatirlar.ToArray());
        if (silindi) Console.WriteLine("Kitap silindi.");
        else Console.WriteLine("Kitap bulunamadi.");
    }

    // ========== ODUNC ISLEMLERI ==========

    static int SonrakiOduncId()
    {
        if (!File.Exists(oduncDosyasi)) return 1;
        string[] satirlar = File.ReadAllLines(oduncDosyasi);
        int enBuyuk = 0;
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            OduncIslem o = OduncIslem.SatirdanOku(satirlar[i]);
            if (o.Id > enBuyuk) enBuyuk = o.Id;
        }
        return enBuyuk + 1;
    }

    static void OduncAl(Kullanici kullanici)
    {
        Console.Write("Odunc alinacak kitap ID: ");
        int id = int.Parse(Console.ReadLine());

        if (!File.Exists(kitapDosyasi))
        {
            Console.WriteLine("Sistemde hic kitap yok.");
            return;
        }

        string[] satirlar = File.ReadAllLines(kitapDosyasi);
        bool bulundu = false;
        string baslikGosterim = "";
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            Kitap kt = Kitap.SatirdanOku(satirlar[i]);
            if (kt.Id == id)
            {
                if (kt.Adet <= 0)
                {
                    Console.WriteLine("Bu kitap stokta yok!");
                    return;
                }
                kt.Adet = kt.Adet - 1;
                baslikGosterim = kt.Baslik;
                satirlar[i] = kt.DosyaSatiri();
                bulundu = true;
                break;
            }
        }

        if (!bulundu)
        {
            Console.WriteLine("Kitap bulunamadi.");
            return;
        }

        File.WriteAllLines(kitapDosyasi, satirlar);

        OduncIslem o = new OduncIslem();
        o.Id = SonrakiOduncId();
        o.KitapId = id;
        o.KullaniciAdi = kullanici.KullaniciAdi;
        o.OduncTarihi = DateTime.Now.ToString("yyyy-MM-dd");
        o.SonIadeTarihi = DateTime.Now.AddDays(15).ToString("yyyy-MM-dd"); // WP4: 15 gun sonra
        o.IadeTarihi = "";

        File.AppendAllText(oduncDosyasi, o.DosyaSatiri() + "\n");
        Console.WriteLine("'" + baslikGosterim + "' odunc alindi. (Kayit #" + o.Id + ", Son iade tarihi: " + o.SonIadeTarihi + ")");
    }

    static void KitapIade(Kullanici kullanici)
    {
        Console.Write("Iade edilecek kitap ID: ");
        int id = int.Parse(Console.ReadLine());

        if (!File.Exists(oduncDosyasi))
        {
            Console.WriteLine("Odunc kaydi bulunamadi.");
            return;
        }

        string[] oduncSatirlar = File.ReadAllLines(oduncDosyasi);
        bool guncellendi = false;
        for (int i = 0; i < oduncSatirlar.Length; i++)
        {
            if (oduncSatirlar[i].Trim() == "") continue;
            OduncIslem o = OduncIslem.SatirdanOku(oduncSatirlar[i]);
            if (o.KitapId == id && o.KullaniciAdi == kullanici.KullaniciAdi && o.IadeTarihi == "")
            {
                o.IadeTarihi = DateTime.Now.ToString("yyyy-MM-dd");
                oduncSatirlar[i] = o.DosyaSatiri();
                guncellendi = true;
                break;
            }
        }

        if (!guncellendi)
        {
            Console.WriteLine("Size ait acik bir odunc kaydi bulunamadi.");
            return;
        }

        File.WriteAllLines(oduncDosyasi, oduncSatirlar);

        // Kitabin stogunu geri arttir
        string[] kitapSatirlar = File.ReadAllLines(kitapDosyasi);
        for (int i = 0; i < kitapSatirlar.Length; i++)
        {
            if (kitapSatirlar[i].Trim() == "") continue;
            Kitap kt = Kitap.SatirdanOku(kitapSatirlar[i]);
            if (kt.Id == id)
            {
                kt.Adet = kt.Adet + 1;
                kitapSatirlar[i] = kt.DosyaSatiri();
                break;
            }
        }
        File.WriteAllLines(kitapDosyasi, kitapSatirlar);

        Console.WriteLine("Kitap basariyla iade edildi.");
    }

    static void BenimOduncIslemlerim(Kullanici kullanici)
    {
        if (!File.Exists(oduncDosyasi))
        {
            Console.WriteLine("Henuz odunc islemi yok.");
            return;
        }

        string[] satirlar = File.ReadAllLines(oduncDosyasi);
        Console.WriteLine();
        Console.WriteLine("--- ODUNC ISLEMLERINIZ ---");
        int sayac = 0;
        for (int i = 0; i < satirlar.Length; i++)
        {
            if (satirlar[i].Trim() == "") continue;
            OduncIslem o = OduncIslem.SatirdanOku(satirlar[i]);
            if (o.KullaniciAdi == kullanici.KullaniciAdi)
            {
                string durum;
                if (o.IadeTarihi == "")
                {
                    DateTime sonTarih = DateTime.Parse(o.SonIadeTarihi);
                    if (DateTime.Now > sonTarih) durum = "GECIKMIS! Son iade: " + o.SonIadeTarihi;
                    else durum = "AKTIF (Son iade: " + o.SonIadeTarihi + ")";
                }
                else
                {
                    durum = "Iade edildi: " + o.IadeTarihi;
                }
                Console.WriteLine("Kayit #" + o.Id + " | Kitap ID:" + o.KitapId + " | Alindi: " + o.OduncTarihi + " | " + durum);
                sayac = sayac + 1;
            }
        }
        if (sayac == 0) Console.WriteLine("Size ait odunc kaydi yok.");
    }

    // ========== KULLANICI EKLEME ==========

    static void KullaniciEkle()
    {
        Console.WriteLine();
        Console.WriteLine("--- YENI KULLANICI ---");
        Console.Write("Kullanici Adi: ");
        string ad = Console.ReadLine();
        Console.Write("Sifre: ");
        string sifre = Console.ReadLine();
        Console.Write("Ad Soyad: ");
        string adSoyad = Console.ReadLine();
        Console.Write("Rol (Yonetici / Personel / Ogrenci): ");
        string rol = Console.ReadLine();

        if (rol != "Yonetici" && rol != "Personel" && rol != "Ogrenci")
        {
            Console.WriteLine("Gecersiz rol! Yonetici, Personel veya Ogrenci olmalidir.");
            return;
        }

        if (File.Exists(kullaniciDosyasi))
        {
            string[] mevcut = File.ReadAllLines(kullaniciDosyasi);
            for (int i = 0; i < mevcut.Length; i++)
            {
                if (mevcut[i].Trim() == "") continue;
                Kullanici varolan = Kullanici.SatirdanOku(mevcut[i]);
                if (varolan.KullaniciAdi == ad)
                {
                    Console.WriteLine("Bu kullanici adi zaten kullaniliyor!");
                    return;
                }
            }
        }

        Kullanici k = new Kullanici();
        k.KullaniciAdi = ad;
        k.Sifre = SifreHashle(sifre);
        k.AdSoyad = adSoyad;
        k.Rol = rol;

        File.AppendAllText(kullaniciDosyasi, k.DosyaSatiri() + "\n");
        Console.WriteLine("Kullanici eklendi.");
    }

    // ========== RAPORLAR (WP6) ==========

    static void RaporlariGoster()
    {
        Console.WriteLine();
        Console.WriteLine("===== RAPORLAR VE ISTATISTIKLER =====");

        // --- Envanter Ozeti ---
        int kitapSayisi = 0;
        int toplamStok = 0;
        if (File.Exists(kitapDosyasi))
        {
            string[] satirlar = File.ReadAllLines(kitapDosyasi);
            for (int i = 0; i < satirlar.Length; i++)
            {
                if (satirlar[i].Trim() == "") continue;
                Kitap k = Kitap.SatirdanOku(satirlar[i]);
                kitapSayisi = kitapSayisi + 1;
                toplamStok = toplamStok + k.Adet;
            }
        }

        int kullaniciSayisi = 0;
        if (File.Exists(kullaniciDosyasi))
        {
            string[] satirlar = File.ReadAllLines(kullaniciDosyasi);
            for (int i = 0; i < satirlar.Length; i++)
            {
                if (satirlar[i].Trim() == "") continue;
                kullaniciSayisi = kullaniciSayisi + 1;
            }
        }

        Console.WriteLine("--- Envanter Ozeti ---");
        Console.WriteLine("Toplam Kitap (baslik) : " + kitapSayisi);
        Console.WriteLine("Toplam Stok (kopya)   : " + toplamStok);
        Console.WriteLine("Toplam Kullanici      : " + kullaniciSayisi);

        // --- Gecikmis Iadeler (15 gunu asanlar) ---
        Console.WriteLine();
        Console.WriteLine("--- Gecikmis Iadeler ---");
        int gecikmisSayisi = 0;
        if (File.Exists(oduncDosyasi))
        {
            string[] satirlar = File.ReadAllLines(oduncDosyasi);
            DateTime bugun = DateTime.Now;
            for (int i = 0; i < satirlar.Length; i++)
            {
                if (satirlar[i].Trim() == "") continue;
                OduncIslem o = OduncIslem.SatirdanOku(satirlar[i]);
                if (o.IadeTarihi != "") continue; // iade edilmis - gecikme yok
                DateTime sonTarih = DateTime.Parse(o.SonIadeTarihi);
                if (bugun > sonTarih)
                {
                    int gecenGun = (int)(bugun - sonTarih).TotalDays;
                    Console.WriteLine("Kayit #" + o.Id +
                        " | Kullanici: " + o.KullaniciAdi +
                        " | Kitap ID:" + o.KitapId +
                        " | Son iade: " + o.SonIadeTarihi +
                        " | " + gecenGun + " gun gecikti");
                    gecikmisSayisi = gecikmisSayisi + 1;
                }
            }
        }
        if (gecikmisSayisi == 0) Console.WriteLine("Gecikmis iade yok.");
        else Console.WriteLine("Toplam gecikmis: " + gecikmisSayisi);
    }
    static void StoktaOlmayanKitaplar()
{
    Console.WriteLine();
    Console.WriteLine("--- STOKTA OLMAYAN KITAPLAR ---");

    if (!File.Exists(kitapDosyasi))
    {
        Console.WriteLine("Kitap dosyasi bulunamadi.");
        return;
    }

    string[] satirlar = File.ReadAllLines(kitapDosyasi);

    int bulunan = 0;

    for (int i = 0; i < satirlar.Length; i++)
    {
        if (satirlar[i].Trim() == "") continue;

        Kitap k = Kitap.SatirdanOku(satirlar[i]);

        if (k.Adet == 0)
        {
            Console.WriteLine(
                "ID: " + k.Id +
                " | " + k.Baslik +
                " | " + k.Yazar);

            bulunan = bulunan + 1;
        }
    }

    if (bulunan == 0)
    {
        Console.WriteLine("Stokta biten kitap yok.");
    }
}
}
