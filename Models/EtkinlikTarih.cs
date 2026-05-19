using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class EtkinlikTarih
    {
        [Key]
        public int EtkinlikTarihID { get; set; }

        [Required]
        public int EtkinlikID { get; set; }
        [ForeignKey("EtkinlikID")]
        public Etkinlik? Etkinlik { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        public TimeSpan Saat { get; set; } = new TimeSpan(10, 0, 0);

        public int Kapasite { get; set; } = 20;

        public int RezervasyonSayisi { get; set; } = 0;

        public bool Aktif { get; set; } = true;

        [NotMapped]
        public int KalanKapasite => Kapasite - RezervasyonSayisi;

        [NotMapped]
        public double DolulukOrani => Kapasite > 0 ? (double)RezervasyonSayisi / Kapasite * 100 : 0;
    }
}
