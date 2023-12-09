using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace LearnBlazer.Data
{
    public class ProjectJson
    {
        public string id { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public string repo { get; set; }
        public string thumbnail { get; set; }
        public string banner { get; set; }
        public string[] buzzwords { get; set; }
    }
    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public string repo { get; set; }
        public string thumbnail { get; set; }
        public string banner { get; set; }
        public string[] description { get; set; }
        public Buzzword[] buzzwords { get; set; }
    }

    public class ProjectProviderService
    {
		public static Project[] Projects { get; private set; }

        private async Task<string[]> LoadDescription(string id)
        {
            var file = $"wwwroot/data/descriptions/{id}.txt";
            if (!File.Exists(file)) // This could cause a race condition; but it shouldn't in 'normal' execution
                return null; 
            return await File.ReadAllLinesAsync(file);
        }

        public async Task<Project[]> GetProjects(BuzzwordProviderService buzzwordProvider)
        {
			if (Projects != null)
			{
				return Projects;
			}
			var projectsFile = $"wwwroot/data/projects.json";
            using var fileHandle = File.OpenRead(projectsFile);
            var projectsJson = await JsonSerializer.DeserializeAsync<ProjectJson[]>(fileHandle);
            Projects = new Project[projectsJson.Length];
            var buzzwordsLookup = await buzzwordProvider.GetBuzzwordLookup();
            for(var i = 0; i < projectsJson.Length; i++)
            {
                var srcProj = projectsJson[i];
                var projectBuzzwords = new Buzzword[srcProj.buzzwords.Length];
                for (var j = 0; j < projectBuzzwords.Length; j++)
                    projectBuzzwords[j] = buzzwordsLookup[srcProj.buzzwords[j]];

                Projects[i] = new Project
                {
                    id = srcProj.id,
                    name = srcProj.name,
                    summary = srcProj.summary,
                    repo = srcProj.repo,
                    thumbnail = srcProj.thumbnail,
                    banner = srcProj.banner,
                    description = await LoadDescription(srcProj.id),
                    buzzwords = projectBuzzwords
                };
                

            }
			return Projects;
        }
        public async Task<Project[]> GetProjectsWithTag(BuzzwordProviderService buzzwordProvider, Buzzword? tag = null)
        {
            var projects = await GetProjects(buzzwordProvider);
            if (tag == null)
                return projects;

            List<Project> tagged = new();
            foreach (var project in projects)
            {
                if (project.buzzwords.Contains(tag))
                    tagged.Add(project);
            }
            return tagged.ToArray();
        }
        public async Task<Project> GetProject(BuzzwordProviderService buzzwordProvider, string id)
        {
            var projects = await GetProjects(buzzwordProvider);

            foreach (var project in projects)
                if (project.id == id)
                    return project;

            return null;
        }
    }
}
