namespace FourthLabThirdTask;

public class SuperCity : IComparer<SuperCity>, IComparable
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long Population { get; set; }

    public int Compare(SuperCity? x, SuperCity? y)
    {
        return x.Population.CompareTo(y.Population);
    }

    public int CompareTo(object? obj)
    {
        var type = typeof(SuperCity);
        if (type.IsInstanceOfType(obj))
        {
            var city = (SuperCity)obj;
            return this.Compare(this, city);
        }

        return 0;
    }
}