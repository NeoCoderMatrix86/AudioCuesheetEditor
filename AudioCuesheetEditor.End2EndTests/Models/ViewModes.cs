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
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace AudioCuesheetEditor.End2EndTests.Models
{
    internal partial class ViewModes(IPage page, bool mobile)
    {
        private readonly IPage _page = page;
        private readonly bool _isMobile = mobile;

        internal async Task SwitchView(string viewMode)
        {
            if (_isMobile)
            {
                await _page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = ViewModeForward() }).Nth(4).ClickAsync();
            }
            await _page.GetByText(viewMode).ClickAsync();
        }

        [GeneratedRegex("^$")]
        internal static partial Regex ViewModeForward();
    }
}
