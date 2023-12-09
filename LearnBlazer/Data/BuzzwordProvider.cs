using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
namespace LearnBlazer.Data
{
    public class BuzzwordJson
    {
        public string[] alias { get; set; }
        public string? logo { get; set; }
        public string? safe_name { get; set; }
    }
    public class Buzzword
    {
        public string Name { get; set; }
        public string SafeName { get; set; }
        public string[] Alias { get; set; }
        public string? Logo { get; set; }
    }
    public class BuzzwordProviderService
    {
        public static Dictionary<string,Buzzword> Buzzwords { get; private set; }

        public async Task<IReadOnlyDictionary<string,Buzzword>> GetBuzzwordLookup()
        {
            if (Buzzwords != null)
            {
                return Buzzwords;
            }
            var buzzwordsFile = $"wwwroot/data/buzzwords.json";
            using var fileHandle = File.OpenRead(buzzwordsFile);
            var buzzwordsJson = await JsonSerializer.DeserializeAsync<Dictionary<string,BuzzwordJson>>(fileHandle);
            Buzzwords = new Dictionary<string, Buzzword>();

            foreach (var kvp in buzzwordsJson)
            {
                var name = kvp.Key;
                var buzzword = kvp.Value;
                Buzzwords[name] = new Buzzword()
                {
                    Name = name,
                    SafeName = buzzword.safe_name != null ? buzzword.safe_name : name,
                    Logo = buzzword.logo,
                    Alias = buzzword.alias,
                };
            }
            return Buzzwords;
        }
        public async Task<Buzzword> TryGetBuzzword(string name)
        {
            var lookup = await GetBuzzwordLookup();
            if (lookup.TryGetValue(name, out var bw)) // Name matches
                return bw;

            name = name.ToLower();
            foreach( var kvp in lookup)
            {
                bw = kvp.Value;
                if (kvp.Key.ToLower() == name)
                    return bw;
                if (bw.SafeName.ToLower() == name)
                    return bw;
                if(bw.Alias != null)
                    foreach (var bwa in bw.Alias)
                        if (bwa.ToLower() == name)
                            return bw;
            }
            return null;

        }
    }

}
