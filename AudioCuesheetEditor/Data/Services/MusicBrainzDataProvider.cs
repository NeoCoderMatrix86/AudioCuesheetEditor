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
using MetaBrainz.MusicBrainz;
using System.Reflection;

namespace AudioCuesheetEditor.Data.Services
{
    public class MusicBrainzDataProvider
    {
        public const String Application = "AudioCuesheetEditor";
        public const String ProjectUrl = "https://github.com/NeoCoderMatrix86/AudioCuesheetEditor";

        public async Task<Dictionary<Guid, String?>> SearchArtistAsync(String searchString)
        {
            var artistSearchResult = await Query.FindArtistsAsync(searchString, simple: true);
            return artistSearchResult.Results.ToDictionary(x => x.Item.Id,x => x.Item.Name);
        }

        //TODO: SearchTitle

        private Query Query
        {
            get
            {
                string? version = null;
                var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null)
                {
                    version = versionAttribute.InformationalVersion;
                }
                return new Query(Application, version, ProjectUrl);
            }
        }
    }
}
