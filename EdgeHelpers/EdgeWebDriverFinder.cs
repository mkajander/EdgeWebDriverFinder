using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace EdgeHelpers
{
    public interface IConfigure
    {
        IFindAvailableDrivers FindAvailableDrivers();
    }

    public interface IFindAvailableDrivers
    {
        IFindEdgeVersion FindEdgeVersion();
    }

    public interface IFindEdgeVersion
    {
        string ReturnMatchedDriverPath();
    }


    public class EdgeWebDriverFinder : IConfigure, IFindAvailableDrivers, IFindEdgeVersion
    {
        //Adding others would obviously need some simple refactoring
        public enum DriverTypeEnums
        {
            Edge = 0
        }

        private readonly ILogger _logger;

        public EdgeWebDriverFinder(ILogger<EdgeWebDriverFinder> logger)
        {
            _logger = logger;
        }

        public DriverTypeEnums DriverType { get; private set; }
        public Dictionary<string, string> AvailableDrivers { get; private set; }
        public string EdgeVersion { get; private set; }

        public string EdgePath { get; private set; } =
            "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe";

        public string DriveFolder { get; private set; } =
            "S:\\Sovellukset\\Tax_technology\\Development\\DA tiimi\\ServerBots\\EdgeDrivers";

        public IFindAvailableDrivers FindAvailableDrivers()
        {
            var drivers = new Dictionary<string, string>();
            var partialName = "msedgedriver";
            var hdDirectoryInWhichToSearch = new DirectoryInfo(DriveFolder);
            var filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + partialName + "*").ToArray();

            foreach (var foundFile in filesInDir)
            {
                var fullName = foundFile.FullName;
                var hasversionInName = foundFile.Name.Contains('-');
                var version = GetFileVersionInfo(fullName).FileVersion;
                if (hasversionInName)
                {
                    drivers.Add(version, fullName);
                    continue;
                }

                var newName = Path.Combine(foundFile.Directory.FullName,
                    $"msedgedriver-{version}{foundFile.Extension}");
                if (File.Exists(newName))
                {
                    File.Delete(newName); //if this file exists then delete it
                    drivers.Remove(version);
                }

                File.Move(foundFile.FullName, newName);
                drivers.Add(version, newName);
            }

            _logger.LogInformation("Found drivers", drivers);

            AvailableDrivers = drivers;
            return this;
        }

        public IFindEdgeVersion FindEdgeVersion()
        {
            try
            {
                EdgeVersion = GetFileVersionInfo(EdgePath).FileVersion;
                _logger.LogInformation($"Edge Version {EdgeVersion}");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError("Edge Version not Found.", ex);
            }

            return this;
        }

        public string ReturnMatchedDriverPath()
        {
            foreach (var availableDriver in AvailableDrivers)
                if (availableDriver.Key.Substring(0, 2) == EdgeVersion.Substring(0, 2))
                {
                    _logger.LogInformation("Returning driver", availableDriver);
                    return availableDriver.Value;
                }

            throw new EdgeWebDriverFinderException("Driver not found or version not available", AvailableDrivers,
                EdgeVersion);
        }

        public IConfigure Configure(string edgePath = null, string driveFolder = null,
            DriverTypeEnums driverType = DriverTypeEnums.Edge)
        {
            EdgePath = edgePath ?? EdgePath;
            DriveFolder = driveFolder ?? DriveFolder;
            DriverType = driverType;
            return this;
        }

        public static FileVersionInfo GetFileVersionInfo(string filePath)
        {
            return FileVersionInfo.GetVersionInfo(filePath);
        }
    }
}