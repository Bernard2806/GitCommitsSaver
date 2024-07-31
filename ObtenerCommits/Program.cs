using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ObtenerCommits;


class Program
{
    private static async Task Main(string[] args)
    {
        Console.Write("Ingrese su nombre de usuario de GitHub: ");
        var username = Console.ReadLine();
        Console.Clear();

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("El nombre de usuario no puede estar vacío.");
            return;
        }

        try
        {
            var repos = await GetUserRepositoriesAsync(username);
            if (repos.Count == 0)
            {
                Console.WriteLine("No se encontraron repositorios para este usuario.");
                return;
            }

            string[] repoNames = repos.ConvertAll(repo => repo.Name).ToArray(); // Convierte los nombres de repositorios a una lista de strings
            int selectedIndex = MenuEasy.MostrarMenu(repoNames, "Seleccione el repositorio del cual desea obtener los commits:");

            if (selectedIndex >= 0 && selectedIndex < repos.Count)
            {
                var selectedRepo = repos[selectedIndex];
                var commits = await GetAllCommitsAsync(selectedRepo.Owner, selectedRepo.Name);
                SaveCommitsToFile(commits, selectedRepo.Name);
                Console.WriteLine($"Commits del repositorio '{selectedRepo.Name}' guardados exitosamente.");
                Console.WriteLine($"Número total de commits: {commits.Count}");
            }
            else
            {
                Console.WriteLine("Selección de repositorio inválida.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        Console.ReadKey();
    }

    private static async Task<List<Repository>> GetUserRepositoriesAsync(string username)
    {
        var repositories = new List<Repository>();
        var url = $"https://api.github.com/users/{username}/repos";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await client.GetStringAsync(url);
            var json = JArray.Parse(response);

            foreach (var repo in json)
            {
                repositories.Add(new Repository
                {
                    Name = repo["name"].ToString(),
                    Owner = username
                });
            }
        }

        return repositories;
    }

    private static async Task<List<CommitInfo>> GetAllCommitsAsync(string owner, string repoName)
    {
        var commits = new List<CommitInfo>();
        var url = $"https://api.github.com/repos/{owner}/{repoName}/commits";
        int page = 1;
        const int perPage = 100;

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            while (true)
            {
                var response = await client.GetStringAsync($"{url}?page={page}&per_page={perPage}");
                var json = JArray.Parse(response);

                if (json.Count == 0)
                    break;

                foreach (var commit in json)
                {
                    var message = commit["commit"]["message"].ToString();
                    var lines = message.Split(new[] { '\n' }, 2); // Divide en dos partes usando el primer salto de línea

                    var commitInfo = new CommitInfo
                    {
                        Title = lines[0].Trim(),
                        Body = lines.Length > 1 ? lines[1].Trim() : string.Empty,
                        Date = DateTimeOffset.Parse(commit["commit"]["committer"]["date"].ToString())
                    };
                    commits.Add(commitInfo);
                }

                page++;
                await Task.Delay(500);
                Console.Clear();
            }
        }

        return commits;
    }

    private static void SaveCommitsToFile(List<CommitInfo> commits, string repoName)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, $"{repoName}_commits.md");

        // Invertir la lista de commits para guardar los más antiguos primero
        commits.Reverse();

        using (var writer = new StreamWriter(filePath))
        {
            int commitNumber = 1; // Contador para el número del commit

            foreach (var commit in commits)
            {
                writer.WriteLine($"## Commit #{commitNumber}: {commit.Title}");
                writer.WriteLine();

                if (!string.IsNullOrEmpty(commit.Body))
                {
                    writer.WriteLine($"**Body:**");
                    writer.WriteLine($"{commit.Body}");
                    writer.WriteLine();
                }

                writer.WriteLine($"**Date:** {commit.Date}");
                writer.WriteLine();
                writer.WriteLine(new string('-', 50));

                commitNumber++;
            }
        }
    }
}

public class Repository
{
    public string Name { get; set; }
    public string Owner { get; set; }
}

public class CommitInfo
{
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
}
