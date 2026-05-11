namespace OgrenciKutuphaneSistemi;

// Akademik gereklilik: manuel olarak yazilmis tek yonlu bagli liste.
// Kitaplari bellekte tutmak icin kullaniyoruz.

public class KitapDugum
{
    public Kitap Veri;
    public KitapDugum Sonraki;

    public KitapDugum(Kitap veri)
    {
        Veri = veri;
        Sonraki = null;
    }
}

public class KitapBagliListe
{
    public KitapDugum Bas;

    public KitapBagliListe()
    {
        Bas = null;
    }

    // Listenin sonuna yeni bir dugum ekler
    public void Ekle(Kitap kitap)
    {
        KitapDugum yeni = new KitapDugum(kitap);

        if (Bas == null)
        {
            Bas = yeni;
            return;
        }

        KitapDugum gecici = Bas;
        while (gecici.Sonraki != null)
        {
            gecici = gecici.Sonraki;
        }
        gecici.Sonraki = yeni;
    }

    // Listedeki dugum sayisini doner
    public int Sayim()
    {
        int sayac = 0;
        KitapDugum gecici = Bas;
        while (gecici != null)
        {
            sayac = sayac + 1;
            gecici = gecici.Sonraki;
        }
        return sayac;
    }

    // Listeyi diziye cevirir (siralama icin gerekli)
    public Kitap[] DiziyeCevir()
    {
        int n = Sayim();
        Kitap[] dizi = new Kitap[n];
        KitapDugum gecici = Bas;
        int i = 0;
        while (gecici != null)
        {
            dizi[i] = gecici.Veri;
            i = i + 1;
            gecici = gecici.Sonraki;
        }
        return dizi;
    }
}
