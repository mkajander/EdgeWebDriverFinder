using System;
using System.Collections.Generic;

namespace EdgeHelpers
{
    [Serializable]
    public class EdgeWebDriverFinderException : Exception
    {
        public Dictionary<string,string> DriversAvailable { get; }
        public string EdgeVersion { get; }
        public EdgeWebDriverFinderException() { }
        public EdgeWebDriverFinderException(string message)
            : base(message) { }
        public EdgeWebDriverFinderException(string message, Exception inner)
            : base(message, inner) { }
        public EdgeWebDriverFinderException(string message, Dictionary<string, string> driversAvailable, string edgeVersion) : this(message)
        {
            DriversAvailable = driversAvailable;
            EdgeVersion = edgeVersion;
        }

    }
}