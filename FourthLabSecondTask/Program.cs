using System.Collections.Concurrent;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "document.txt");
        var result = ReadChunksAsync(filePath);
        result.Wait();
        timer.Stop();
        System.Console.WriteLine($"result time: {timer.ElapsedMilliseconds} ms");

        WriteToFile("result.txt", result.Result);
    }

    static async Task<ConcurrentDictionary<string, int>> ReadChunksAsync(string filePath)
    {
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 65536, useAsync: true))
        {
            long totalBytesRead = 0;
            var buffer = new byte[65536]; // 64 KB buffer
            var wordCounts = new ConcurrentDictionary<string, int>();
            async Task Loop()
            {
                var bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    totalBytesRead += bytesRead;
                    // Console.WriteLine($"{totalBytesRead / 1024 / 1024} MB read");
                    var content = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    var noPunctuation = Regex.Replace(content, @"[.,\-]", "").ToLower();
                    var words = noPunctuation.Split(' ', '\t', '\n', '\r').Where(s => s != "").ToArray();
                    foreach (var word in words)
                    {
                        wordCounts.AddOrUpdate(word, 1, (_, count) => count + 1);
                    }
                    await Loop();
                }
            }

            var tasks = Enumerable.Range(1, Environment.ProcessorCount).Select(_ => Loop()).ToArray();
            await Task.WhenAll(tasks);

            System.Console.WriteLine($"Total bytes read: {totalBytesRead}");
            return wordCounts;
        }
    }

    static void WriteToFile(string path, ConcurrentDictionary<string, int> dict)
    {
        using (var writer = new StreamWriter(path))
        {
            foreach (var word in dict)
            {
                writer.WriteLine($"{word.Key}-{word.Value}");
            }
        }
    }



}