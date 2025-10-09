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
    internal class ExportDialog(IPage page)
    {
        private readonly IPage _page = page;

        internal async Task OpenSchemeMenuAsync(string schemeHeadMenuName = "Scheme head")
        {
            await _page.Locator("div").Filter(new() { HasTextRegex = new Regex($"^{schemeHeadMenuName}$") }).GetByRole(AriaRole.Button).Nth(1).ClickAsync();
        }
    }
}
