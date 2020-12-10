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
        private static CuesheetController cuesheetController;

        public static CuesheetController GetCuesheetController()
        {
            if (cuesheetController == null)
            {
                var options = Options.Create(new LocalizationOptions());
                var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
                var localizer = new StringLocalizer<Localization>(factory);
                cuesheetController = new CuesheetController(localizer);
            }
            return cuesheetController;
        }
    }
}
