using ChatApp.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ChatApp.Configuration
{
    public class AppConfiguration
    {
        //private string CONFIGFILE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "configuration.json");
        private AppConfigurations appConfigurations;

        public AppConfiguration()
        {

            
        }

        public async Task ConstructAppConfiguration()
        {
            //var jsonFileReader = File.OpenText(CONFIGFILE_PATH);
            var jsonFileReader = new StreamReader(await FileSystem.OpenAppPackageFileAsync("configuration.json"));
            appConfigurations = JsonSerializer.Deserialize<AppConfigurations>(jsonFileReader.ReadToEnd(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }

        public string GetLocalServerHost()
        {
            return appConfigurations.LocalHost;
        }

        public string GetRemoteServerHost()
        {
            return appConfigurations.RemoteHost;
        }

        public bool IsUsingLocalServerHost()
        {
            return appConfigurations.IsUsingLocalHost;
        }

    }
}
