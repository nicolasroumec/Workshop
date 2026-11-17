using System.Text.Json;

namespace Workshop.Projects;

internal class ProjectStore(string dataFolder)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath = Path.Combine(dataFolder, "projects.json");

    public string DataFolder { get; } = dataFolder;

    public List<Project> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Project>>(json, JsonOptions) ?? [];
    }

    public void Save(List<Project> projects)
    {
        Directory.CreateDirectory(DataFolder);

        var json = JsonSerializer.Serialize(projects, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public void MarkAsLast(List<Project> projects, Project project)
    {
        projects.Remove(project);
        projects.Insert(0, project);
        Save(projects);
    }
}
