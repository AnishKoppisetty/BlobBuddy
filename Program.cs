using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

class Program
{
    // use your container. change if needed.
    private const string ContainerName = "filesharecus";

    static async Task Main()
    {
        var conn = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(conn))
        {
            Console.WriteLine("AZURE_STORAGE_CONNECTION_STRING not set. export it and run again.");
            return;
        }

        var container = new BlobContainerClient(conn, ContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);

        while (true)
        {
            Console.WriteLine("\n=== blob buddy ===");
            Console.WriteLine("1) upload");
            Console.WriteLine("2) list");
            Console.WriteLine("3) download");
            Console.WriteLine("4) delete");
            Console.WriteLine("5) share link (SAS)");
            Console.WriteLine("0) exit");
            Console.Write("choose: ");
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await UploadAsync(container);
                        break;
                    case "2":
                        await ListAsync(container);
                        break;
                    case "3":
                        await DownloadAsync(container);
                        break;
                    case "4":
                        await DeleteAsync(container);
                        break;
                    case "5":
                        await ShareAsync(container);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("invalid option.");
                        break;
                }
            }
            catch (RequestFailedException rfe)
            {
                Console.WriteLine($"azure error: {rfe.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }
    }

    static async Task UploadAsync(BlobContainerClient container)
    {
        Console.Write("local file path to upload: ");
        var path = (Console.ReadLine() ?? "").Trim('"');

        if (!File.Exists(path))
        {
            Console.WriteLine("file not found.");
            return;
        }

        var name = Path.GetFileName(path);
        var blob = container.GetBlobClient(name);

        var headers = new BlobHttpHeaders { ContentType = GuessContentType(name) };

        await using var fs = File.OpenRead(path);
        // BlobUploadOptions does not have an Overwrite property. Use the overload
        // that accepts the `overwrite` parameter, then set headers afterwards.
        await blob.UploadAsync(fs, overwrite: true);
        await blob.SetHttpHeadersAsync(headers);

        Console.WriteLine($"uploaded as {name}.");
    }

    static async Task ListAsync(BlobContainerClient container)
    {
        Console.WriteLine("blobs:");
        await foreach (var item in container.GetBlobsAsync())
        {
            var size = item.Properties.ContentLength ?? 0;
            var mod = item.Properties.LastModified?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
            Console.WriteLine($"- {item.Name}  {size} bytes  modified {mod} UTC");
        }
    }

    static async Task DownloadAsync(BlobContainerClient container)
    {
        Console.Write("blob name to download: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) return;

        Console.Write("destination folder, default ./downloads: ");
        var folder = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(folder)) folder = "./downloads";

        Directory.CreateDirectory(folder);
        var dest = Path.Combine(folder, name);

        var blob = container.GetBlobClient(name);
        await blob.DownloadToAsync(dest);
        Console.WriteLine($"downloaded to {Path.GetFullPath(dest)}.");
    }

    static async Task DeleteAsync(BlobContainerClient container)
    {
        Console.Write("blob name to delete: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) return;

        var blob = container.GetBlobClient(name);
        var result = await blob.DeleteIfExistsAsync();
        Console.WriteLine(result.Value ? "deleted." : "not found.");
    }

    static async Task ShareAsync(BlobContainerClient container)
    {
        Console.Write("blob name to share: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) return;

        Console.Write("expires in hours, default 24: ");
        var hoursStr = Console.ReadLine();
        if (!int.TryParse(hoursStr, out var hours) || hours <= 0) hours = 24;

        var blob = container.GetBlobClient(name);

        if (!blob.CanGenerateSasUri)
        {
            Console.WriteLine("client cannot generate sas. needs key based auth via connection string.");
            return;
        }

        var sas = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            BlobName = name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(hours)
        };
        sas.SetPermissions(BlobSasPermissions.Read);

        var uri = blob.GenerateSasUri(sas);
        Console.WriteLine("\nsas url:");
        Console.WriteLine(uri.ToString());
        Console.WriteLine();
        await Task.CompletedTask;
    }

    static string GuessContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".html" => "text/html",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
