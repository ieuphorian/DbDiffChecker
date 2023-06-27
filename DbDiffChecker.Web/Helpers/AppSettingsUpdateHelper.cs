using System.Configuration;

namespace DbDiffChecker.Web.Helpers
{
    public class AppSettingsUpdateHelper
    {
        public static void AddOrUpdateAppSetting<T>(string key, T value/*, string environment*/)
        {
            try
            {
                string fileName = "appsettings.json";
                /*if (!string.IsNullOrEmpty(environment))
                {
                    fileName = $"appsettings.{environment}.json";
                }*/
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                var sectionPath = key.Split(":")[0];

                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(":")[1];
                    jsonObj[sectionPath][keyPath] = value;
                }
                else
                {
                    jsonObj[sectionPath] = value; // if no sectionpath just set the value
                }

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}
