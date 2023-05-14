using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FourthLabThirdTask;

class Program
{
    static async Task<SortedSet<SuperCity>> ReadChunksAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        var header = await reader.ReadLineAsync();
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 1048576,
            useAsync: true);
        var totalBytesRead = 0L;
        var buffer = new byte[1048576];
        var wordCounts = new SortedSet<SuperCity>();

        async Task Loop()
        {
            var bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                totalBytesRead += bytesRead;
                Console.WriteLine($"{totalBytesRead / 1024 / 1024} MB");
                var content = Encoding.Default.GetString(buffer, 0, bytesRead);
                var records = content.Split("\n");
                foreach (var record in records)
                {
                    var splitted = record.Split(", ");
                    if (splitted.Length == 3)
                    {
                        var id = splitted[0];
                        var name = splitted[1];

                        if (long.TryParse(splitted[2], out var population))
                        {
                            var city = new SuperCity()
                            {
                                Id = id,
                                Name = name,
                                Population = population
                            };
                            lock (wordCounts)
                            {
                                if (wordCounts.Count >= 100)
                                {
                                    wordCounts.Add(city);
                                    wordCounts.Remove(wordCounts.Last());
                                    break;
                                }
                                else
                                {
                                    wordCounts.Add(city);
                                }
                            }
                        }
                    }
                }
                await Loop();
            }
        }

        var tasks = Enumerable.Range(1, Environment.ProcessorCount)
            .Select(_ => Loop())
            .ToArray();

        await Task.WhenAll(tasks);
        Console.WriteLine($"Total bytes read: {totalBytesRead}");
        return wordCounts;
    }

    static void WriteToFile(string path, SortedSet<SuperCity> cities, long time)
    {
        using var writer = new StreamWriter(path);
        foreach (var city in cities.OrderBy(x => x.Population))
        {
            writer.Write($"{city.Id}-{city.Name}-{city.Population}\n");
        }
        writer.WriteLine($"Spend={time}ms");
    }

    static void Main()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "cities.csv");
        var result = ReadChunksAsync(filePath).GetAwaiter().GetResult();
        timer.Stop();
        Console.WriteLine($"Result time: {timer.ElapsedMilliseconds} ms");
        WriteToFile(Path.Combine(Directory.GetCurrentDirectory(), "resultCities.txt"), result, timer.ElapsedMilliseconds);
    }
}