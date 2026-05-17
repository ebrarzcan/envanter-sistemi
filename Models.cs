namespace OgrenciKutuphaneSistemi;

// Kitap modeli
public class Kitap
{
    public int Id;
    public string Baslik;
    public string Yazar;
    public int Yil;
    public int Adet;

    public string DosyaSatiri()
    {
        return Id + "|" + Baslik + "|" + Yazar + "|" + Yil + "|" + Adet;
    }

    public static Kitap SatirdanOku(string satir)
    {
        string[] parcalar = satir.Split('|');
        Kitap k = new Kitap();
        k.Id = int.Parse(parcalar[0]);
        k.Baslik = parcalar[1];
        k.Yazar = parcalar[2];
        k.Yil = int.Parse(parcalar[3]);
        k.Adet = int.Parse(parcalar[4]);
        return k;
    }
}

// Kullanici modeli
public class Kullanici
{
    public string KullaniciAdi;
    public string Sifre;
    public string AdSoyad;
    public string Rol; // Yonetici, Personel, Ogrenci

    public string DosyaSatiri()
    {
        return KullaniciAdi + "|" + Sifre + "|" + AdSoyad + "|" + Rol;
    }

    public static Kullanici SatirdanOku(string satir)
    {
        string[] parcalar = satir.Split('|');
        Kullanici k = new Kullanici();
        k.KullaniciAdi = parcalar[0];
        k.Sifre = parcalar[1];
        k.AdSoyad = parcalar[2];
        k.Rol = parcalar[3];
        return k;
    }
}

// Odunc alma kaydi
public class OduncIslem
{
    public int Id;
    public int KitapId;
    public string KullaniciAdi;
    public string OduncTarihi;
    public string SonIadeTarihi; // 15 gun sonrasi - kitabin geri verilmesi gereken tarih (WP4)
    public string IadeTarihi;    // bos ise henuz iade edilmemis demektir

    public string DosyaSatiri()
    {
        return Id + "|" + KitapId + "|" + KullaniciAdi + "|" + OduncTarihi + "|" + SonIadeTarihi + "|" + IadeTarihi;
    }

    public static OduncIslem SatirdanOku(string satir)
    {
        string[] parcalar = satir.Split('|');
        OduncIslem o = new OduncIslem();
        o.Id = int.Parse(parcalar[0]);
        o.KitapId = int.Parse(parcalar[1]);
        o.KullaniciAdi = parcalar[2];
        o.OduncTarihi = parcalar[3];
        o.SonIadeTarihi = parcalar[4];
        o.IadeTarihi = parcalar[5];
        return o;
    }
}
