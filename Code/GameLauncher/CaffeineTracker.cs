using System.Text.Json;

namespace GameLauncher;

public class CaffeineTracker
{
    private readonly string _dataPath;
    
    public CaffeineTracker()
    {
        _dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CoffeePause",
            "caffeine_data.json"
        );
    }
    
    public List<CaffeineEntry> LoadEntries()
    {
        if (!File.Exists(_dataPath))
            return new List<CaffeineEntry>();
            
        try
        {
            var json = File.ReadAllText(_dataPath);
            return JsonSerializer.Deserialize<List<CaffeineEntry>>(json) ?? new List<CaffeineEntry>();
        }
        catch
        {
            return new List<CaffeineEntry>();
        }
    }
    
    public void SaveEntry(CaffeineEntry entry)
    {
        var entries = LoadEntries();
        entries.Add(entry);
        
        // Keep only entries from the last 7 days
        var sevenDaysAgo = DateTime.Now.AddDays(-7);
        entries = entries.Where(e => e.ConsumedAt >= sevenDaysAgo).ToList();
        
        try
        {
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataPath, json);
        }
        catch
        {
            // Silently fail
        }
    }
    
    public double GetCurrentCaffeineLevel(DateTime sleepTime)
    {
        var entries = LoadEntries();
        double totalCaffeine = 0;
        var now = DateTime.Now;
        
        // Caffeine has a half-life of about 5 hours
        const double halfLife = 5.0;
        
        foreach (var entry in entries)
        {
            // Only count caffeine consumed before sleep time today
            if (entry.ConsumedAt > sleepTime && entry.ConsumedAt.Date == sleepTime.Date)
                continue;
                
            var hoursSince = (now - entry.ConsumedAt).TotalHours;
            if (hoursSince < 0) continue;
            
            // Calculate remaining caffeine using exponential decay
            var remaining = entry.CaffeineAmount * Math.Pow(0.5, hoursSince / halfLife);
            totalCaffeine += remaining;
        }
        
        return totalCaffeine;
    }
}

public class CaffeineEntry
{
    public string DrinkType { get; set; } = "";
    public int SizeMl { get; set; }
    public int Quantity { get; set; }
    public double CaffeineAmount { get; set; }
    public DateTime ConsumedAt { get; set; }
}

public static class CaffeineData
{
    // Caffeine content in mg per 100ml
    public static Dictionary<string, double> CaffeineContent = new Dictionary<string, double>
    {
        { "Espresso", 212.0 },
        { "Coffee (Brewed)", 40.0 },
        { "Coffee (Instant)", 30.0 },
        { "Black Tea", 20.0 },
        { "Green Tea", 15.0 },
        { "Energy Drink", 32.0 },
        { "Cola", 10.0 },
        { "Dark Chocolate (per 100g)", 43.0 }
    };
    
    public static double CalculateCaffeine(string drinkType, int sizeMl, int quantity)
    {
        if (CaffeineContent.TryGetValue(drinkType, out double contentPer100ml))
        {
            return (contentPer100ml * sizeMl / 100.0) * quantity;
        }
        return 0;
    }
}
