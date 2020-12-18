//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
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
            Localizer = new StringLocalizer<Localization>(factory);
            CuesheetController = new CuesheetController(Localizer);
        }

        public StringLocalizer<Localization> Localizer { get; private set; }
        public CuesheetController CuesheetController { get; private set; }
    }
}
