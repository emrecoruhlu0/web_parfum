using WebParfum.Models;

namespace WebParfum.ViewModels;

public class DailyLogIndexViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>Bu ayın gün sayısı.</summary>
    public int DaysInMonth { get; set; }

    /// <summary>Ayın 1'i hangi haftanın gününe denk geliyor (1=Pzt … 7=Paz).</summary>
    public int FirstDayOfWeek { get; set; }

    /// <summary>Anahtar: gün (1..31), değer: o güne ait log listesi.</summary>
    public Dictionary<int, List<DailyLog>> LogsByDay { get; set; } = [];

    public int TotalLogsThisMonth { get; set; }

    /// <summary>Kullanıcının "owned" statüsündeki koleksiyonu — hızlı log için.</summary>
    public List<Collection> OwnedCollection { get; set; } = [];

    // Navigation
    public (int Year, int Month) PrevMonth => Month == 1 ? (Year - 1, 12) : (Year, Month - 1);
    public (int Year, int Month) NextMonth => Month == 12 ? (Year + 1, 1) : (Year, Month + 1);
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy",
        new System.Globalization.CultureInfo("tr-TR"));
}
