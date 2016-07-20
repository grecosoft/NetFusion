﻿using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Logging.Serilog
{
    public class SerilogManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "578e90d997ff280dace9cc83";
        public string Name => "Serilog Plug-in";

        public string Description =>
            "Registers a logger implementing IContainerLogger that delegates to Serilog.";
    }
}
