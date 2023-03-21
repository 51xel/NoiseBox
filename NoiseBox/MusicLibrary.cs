using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoiseBox.Log;

namespace NoiseBox {
    public class Song {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public TimeSpan Duration { get; set; }

        public Song Clone() {
            return new Song {
                Id = this.Id,
                Name = this.Name,
                Path = this.Path,
                Duration = this.Duration
            };
        }
    }
    public class Playlist {
        public string Name { get; set; }
        public List<string> SongIds { get; set; }
        public Playlist() {
            SongIds = new List<string>();
        }

        public Playlist Clone() {
            return new Playlist {
                Name = this.Name,
                SongIds = this.SongIds.ToList()
            };
        }
    }
    public class MusicLibrary {
        private const string _jsonFilePath = "music_library.json";
        private static List<Song> _songs = new List<Song>();
        private static List<Playlist> _playlists = new List<Playlist>();
        private static ILog _log = LogSettings.SelectedLog;

        static MusicLibrary() {
            LoadFromJson();
        }

        private static void LoadFromJson() {
            if (File.Exists(_jsonFilePath)) {
                string json = File.ReadAllText(_jsonFilePath);
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);

                _songs = ((JArray)data["songs"]).ToObject<List<Song>>();
                _playlists = ((JArray)data["playlists"]).ToObject<List<Playlist>>();

                _log.Print("Data loaded from json", LogInfoType.INFO);
            }
            else {
                File.Create(_jsonFilePath).Close();

                _log.Print("Empty json file created", LogInfoType.INFO);
            }
        }
        private static void SaveToJson() {
            var data = new Dictionary<string, object>
            {
                { "songs", JArray.FromObject(_songs) },
                { "playlists", JArray.FromObject(_playlists) }
            };

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            File.WriteAllText(_jsonFilePath, json);

            _log.Print("Data saved to json", LogInfoType.INFO);
        }
        public static bool AddSong(Song song) {
            if (File.Exists(song.Path) && Path.GetExtension(song.Path).Equals(".mp3", StringComparison.OrdinalIgnoreCase)) {
                // Generate a unique ID for the song
                song.Id = Guid.NewGuid().ToString();

                var tagFile = TagLib.File.Create(song.Path);

                if (tagFile.Tag.Title == null) {
                    song.Name = Path.GetFileNameWithoutExtension(song.Path);
                }
                else {
                    song.Name = tagFile.Tag.Title;
                }
                
                song.Duration = tagFile.Properties.Duration;

                _songs.Add(song.Clone());

                _log.Print($"New song with id {song.Id} added", LogInfoType.INFO);

                SaveToJson();

                return true;
            }

            return false;
        }

        public static async Task ConvertToMp3(string path) {
            var directory = System.AppContext.BaseDirectory.Split(Path.DirectorySeparatorChar);
            var slice = new ArraySegment<string>(directory, 0, directory.Length - 4);
            var BinariesDirPath = Path.Combine(Path.Combine(slice.ToArray()), "Binaries");
            var ffmpegLocation = Path.Combine(BinariesDirPath, @"ffmpeg\bin");

            var psi = new ProcessStartInfo(Path.Combine(ffmpegLocation, "ffmpeg.exe")) {
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $" -i \"{path}\" -vn -ac 2 \"{Path.ChangeExtension(path, ".mp3")}\""
            };

            var process = new Process { StartInfo = psi };

            process.Start();

            await process.WaitForExitAsync();

            process.Dispose();
        }

        public static void RemoveSong(string songId) {
            _songs.RemoveAll(s => s.Id == songId);

            foreach (var playlist in _playlists) {
                playlist.SongIds.Remove(songId);
            }

            _log.Print($"Song with id {songId} removed", LogInfoType.INFO);

            SaveToJson();
        }

        public static bool AddPlaylist(Playlist playlist) {
            if (_playlists.Find(p => p.Name == playlist.Name) == null) {
                _playlists.Add(playlist.Clone());

                _log.Print($"New playlist \'{playlist.Name}\' added", LogInfoType.INFO);

                SaveToJson();

                return true;
            }

            return false;
        }

        public static void RemovePlaylist(string playlistName) {
            _playlists.RemoveAll(p => p.Name == playlistName);

            _log.Print($"Playlist \'{playlistName}\' removed", LogInfoType.INFO);

            SaveToJson();
        }

        public static void AddSongToPlaylist(string songId, string playlistName, int position=-1) {
            var playlist = _playlists.Find(p => p.Name == playlistName);

            if (playlist != null) {
                if (!playlist.SongIds.Contains(songId)) {
                    if (position == -1) {
                        playlist.SongIds.Add(songId);
                    }
                    else {
                        if (position >= 0 && position <= playlist.SongIds.Count) {
                            playlist.SongIds.Insert(position, songId);
                        }
                    }

                    _log.Print($"Song with id {songId} added to playlist \'{playlistName}\'", LogInfoType.INFO);

                    SaveToJson();
                }
            }
        }

        public static void RemoveSongFromPlaylist(string songId, string playlistName) {
            var playlist = _playlists.Find(p => p.Name == playlistName);

            if (playlist != null) {
                playlist.SongIds.Remove(songId);

                _log.Print($"Song with id {songId} removed from playlist \'{playlistName}\'", LogInfoType.INFO);

                SaveToJson();
            }
        }

        public static void MoveSongToPlaylist(string songId, string fromPlaylist, string toPlaylist) {
            var from = _playlists.Find(p => p.Name == fromPlaylist);
            var to = _playlists.Find(p => p.Name == toPlaylist);

            if (from != null && to != null) {
                from.SongIds.Remove(songId);
                to.SongIds.Add(songId);

                _log.Print($"Song with id {songId} moved from \'{fromPlaylist}\' to \'{toPlaylist}\'", LogInfoType.INFO);

                SaveToJson();
            }
        }

        public static bool RenameSong(string songId, string newName) {
            var song = _songs.Find(s => s.Id == songId);

            if (song != null && !string.IsNullOrEmpty(newName)) {
                _log.Print($"Song with id {songId} has been renamed", LogInfoType.INFO);

                song.Name = newName;
                SaveToJson();

                return true;
            }

            return false;
        }

        public static bool RenamePlaylist(string oldName, string newName) {
            var playlist = _playlists.Find(s => s.Name == oldName);

            if (playlist != null && !string.IsNullOrEmpty(newName) && _playlists.Find(s => s.Name == newName) == null) {
                _log.Print($"Playlist with name {oldName} has been renamed to {newName}", LogInfoType.INFO);

                playlist.Name = newName;
                SaveToJson();

                return true;
            }

            return false;
        }

        public static IEnumerable<Song> GetSongs() {
            var songs = new List<Song>();

            foreach (var song in _songs) {
                songs.Add(song.Clone());
            }

            return songs;
        }

        public static IEnumerable<Playlist> GetPlaylists() {
            var playlists = new List<Playlist>();

            foreach (var playlist in _playlists) {
                playlists.Add(playlist.Clone());
            }

            return playlists;
        }

        public static IEnumerable<Song> GetSongsFromPlaylist(string playlistName) {
            var playlist = _playlists.Find(p => p.Name == playlistName);
            var songsFromPlaylist = new List<Song>();

            if (playlist != null) {
                foreach (var songId in playlist.SongIds) {
                    songsFromPlaylist.Add(_songs.Find(p => p.Id == songId).Clone());
                }
            }

            return songsFromPlaylist;
        }
    }
}
