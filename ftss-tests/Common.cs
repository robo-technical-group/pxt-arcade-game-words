using System.Reflection;

namespace ftss_tests;

internal static class Common
{
    // https://adamprescott.net/2012/07/26/files-as-embedded-resources-in-unit-tests/
    public async static Task<string> GetResourceFileContents(string path)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        string resource = string.Format("ftss_tests.Resources.{0}", path);
        using (Stream? stream = asm.GetManifestResourceStream(resource))
        {
            if (stream != null)
            {
                StreamReader reader = new(stream);
                return await reader.ReadToEndAsync();
            }
        }
        return string.Empty;
    }

    public async static Task<IList<string>> GetResourceFileAsStringList(string path)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        string resource = string.Format("ftss-tests.Resources.{0}", path);
        using (Stream? stream = asm.GetManifestResourceStream(resource))
        {
            if (stream != null)
            {
                StreamReader reader = new(stream);
                string? line;
                List<string> list = [];
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    list.Add(line);
                }
                return list;
            }
        }
        return [];
    }
}
