using System.ComponentModel;

namespace NobelLaureatesApp.Model;

public class NobelData
{
    public List<Prize> prizes { get; set; }
}

public class Prize
{
    public Prize()
    {
        laureates = new List<Laureate>();
    }

    public int year { get; set; }
    public string category { get; set; }
    public string? overallMotivation { get; set; }
    public List<Laureate>? laureates { get; set; }
}

public class Laureate
{
    public string id { get; set; }
    public string firstname { get; set; }
    public string surname { get; set; }
    public string motivation { get; set; }
    public string share { get; set; }
}