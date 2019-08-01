using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade.Model.UpgradeSetting
{
    public class Replace
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FilesUpgrade.Enum.Type Type { get; set; }
    }
}
