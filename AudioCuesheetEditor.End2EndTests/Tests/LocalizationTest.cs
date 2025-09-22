using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests
{
    [TestClass]
    public class LocalizationTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task ChangeLanguageAsync()
        {
            var bar = new AppBar(TestPage);
            await bar.GotoAsync();
            await bar.ChangeLanguageAsync("German (Germany)");
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Abschnitte" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Allgemeine Informationen" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByText("Aufnahmeansicht")).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Titel" })).ToBeVisibleAsync();
            await Expect(TestPage.GetByRole(AriaRole.Heading, new() { Name = "Wiedergabe" })).ToBeVisibleAsync();
        }
    }
}
