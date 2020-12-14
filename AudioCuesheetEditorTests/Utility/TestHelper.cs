using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Shared.ResourceFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioCuesheetEditorTests.Utility
{
    internal class TestHelper
    {
        public TestHelper()
        {
            var options = Options.Create(new LocalizationOptions());
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            var localizer = new StringLocalizer<Localization>(factory);
            CuesheetController = new CuesheetController(localizer);
        }

        public CuesheetController CuesheetController { get; private set; }
    }
}
