using System.Text.Json;

namespace Workshop.Projects;

internal static class ProjectStore
{
    internal static string DataFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Workshop");

    private static readonly string FilePath = Path.Combine(DataFolder, "projects.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    internal static List<Project> Load()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<Project>>(json) ?? [];
    }

    internal static void Save(List<Project> projects)
    {
        Directory.CreateDirectory(DataFolder);

        var json = JsonSerializer.Serialize(projects, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    internal static void MarkAsLast(List<Project> projects, Project project)
    {
        projects.Remove(project);
        projects.Insert(0, project);
        Save(projects);
    }
}
