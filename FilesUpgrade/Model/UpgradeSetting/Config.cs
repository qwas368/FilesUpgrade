﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade.Model.UpgradeSetting
{
    public class Config
    {
        public List<dynamic> ReplaceList { get; set; } = new List<dynamic>();

        public List<string> IgnoreList { get; set; } = new List<string>();
    }
}
