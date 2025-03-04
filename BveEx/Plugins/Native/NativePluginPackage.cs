﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Plugins.Native
{
    internal class NativePluginPackage : IPluginPackage
    {
        public Identifier Identifier { get; }
        public string LibraryPath { get; }

        public NativePluginPackage(Identifier identifier, string libraryPath)
        {
            Identifier = identifier;
            LibraryPath = libraryPath;
        }
    }
}
