using AudioCuesheetEditor.End2EndTests.Models;
using Microsoft.Playwright;

namespace AudioCuesheetEditor.End2EndTests.Tests
{
    [TestClass]
    public class TracingTest : PlaywrightTestBase
    {
        [TestMethod]
        public async Task UndoRedoTrackTableTestAsync()
        {
            var bar = new AppBar(TestPage);
            await bar.GotoAsync();
            var detailView = new DetailView(TestPage);
            await detailView.AddTrackAsync("Test Artist 1");
            await Expect(bar.UndoButton).ToBeEnabledAsync();
            await Expect(bar.RedoButton).ToBeDisabledAsync();
            await detailView.EditTrackAsync(title: "Test Title 1");
            await Expect(bar.UndoButton).ToBeEnabledAsync();
            await Expect(bar.RedoButton).ToBeDisabledAsync();
            await bar.UndoAsync();
            await bar.UndoAsync();
            await Expect(TestPage.GetByRole(AriaRole.Table)).ToMatchAriaSnapshotAsync(@"- table:
  - rowgroup:
    - row ""# Sort Column options Artist Sort Column options Title Sort Column options Begin Sort Column options End Sort Column options Length Sort Column options Status"":
      - columnheader:
        - checkbox
      - columnheader ""# Sort Column options"":
        - text: ""#""
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Artist Sort Column options"":
        - text: Artist
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Title Sort Column options"":
        - text: Title
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Begin Sort Column options"":
        - text: Begin
        - button ""Sort""
        - button ""Column options""
      - columnheader ""End Sort Column options"":
        - text: End
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Length Sort Column options"":
        - text: Length
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Status""
  - rowgroup:
    - row ""Increment Decrement 00:00:00"":
      - cell:
        - checkbox
      - cell ""Increment Decrement"":
        - spinbutton: ""1""
        - button ""Increment""
        - button ""Decrement""
      - cell:
        - textbox
        - button
      - cell:
        - textbox
        - button
      - cell ""00:00:00"":
        - textbox: 00:00:00
      - cell:
        - textbox
      - cell:
        - textbox
      - cell
  - rowgroup:
    - row");
            await Expect(bar.RedoButton).ToBeEnabledAsync();
            await bar.RedoAsync();
            await bar.RedoAsync();
            await Expect(TestPage.GetByRole(AriaRole.Table)).ToMatchAriaSnapshotAsync("- cell \"Test Title 1 Clear\":\n  - textbox: Test Title 1\n  - button \"Clear\"\n  - button");
            await detailView.EditTrackAsync("Mozart", "Eine kleine Nachtmusik");
            await Expect(TestPage.GetByRole(AriaRole.Table)).ToMatchAriaSnapshotAsync(@"- table:
  - rowgroup:
    - row ""# Sort Column options Artist Sort Column options Title Sort Column options Begin Sort Column options End Sort Column options Length Sort Column options Status"":
      - columnheader:
        - checkbox
      - columnheader ""# Sort Column options"":
        - text: ""#""
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Artist Sort Column options"":
        - text: Artist
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Title Sort Column options"":
        - text: Title
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Begin Sort Column options"":
        - text: Begin
        - button ""Sort""
        - button ""Column options""
      - columnheader ""End Sort Column options"":
        - text: End
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Length Sort Column options"":
        - text: Length
        - button ""Sort""
        - button ""Column options""
      - columnheader ""Status""
  - rowgroup:
    - row ""Increment Decrement Mozart Clear Eine kleine Nachtmusik Clear 00:00:00"":
      - cell:
        - checkbox
      - cell ""Increment Decrement"":
        - spinbutton: ""1""
        - button ""Increment""
        - button ""Decrement""
      - cell ""Mozart Clear"":
        - textbox: Mozart
        - button ""Clear""
        - button
      - cell ""Eine kleine Nachtmusik Clear"":
        - textbox: Eine kleine Nachtmusik
        - button ""Clear""
        - button
      - cell ""00:00:00"":
        - textbox: 00:00:00
      - cell:
        - textbox
      - cell:
        - textbox
      - cell
  - rowgroup:
    - row");
            await bar.UndoAsync();
            await Expect(TestPage.GetByRole(AriaRole.Table)).ToMatchAriaSnapshotAsync("- cell \"Test Title 1 Clear\":\n  - textbox: Test Title 1\n  - button \"Clear\"\n  - button");
        }
    }
}
