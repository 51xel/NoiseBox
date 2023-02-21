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
    }
    public class Playlist {
        public string Name { get; set; }
        public List<string> SongIds { get; set; }
        public Playlist() {
            SongIds = new List<string>();
        }
    }
    public class MusicLibrary {
        private const string _jsonFilePath = "music_library.json";
        private List<Song> _songs = new List<Song>();
        private List<Playlist> _playlists = new List<Playlist>();
        private ILog _log;

        public MusicLibrary(ILog log) {
            if (log == null) {
                throw new ArgumentNullException("Log can`t be null");
            }

            _log = log;

            LoadFromJson();
        }

        private void LoadFromJson() {
            if (File.Exists(_jsonFilePath)) {
                string json = File.ReadAllText(_jsonFilePath);
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);

                _songs = ((JArray)data["songs"]).ToObject<List<Song>>();
                _playlists = ((JArray)data["playlists"]).ToObject<List<Playlist>>();
            }
            else {
                File.Create(_jsonFilePath).Close();
            }
        }
        public void SaveToJson() {
            var data = new Dictionary<string, object>
            {
                { "songs", JArray.FromObject(_songs) },
                { "playlists", JArray.FromObject(_playlists) }
            };

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            File.WriteAllText(_jsonFilePath, json);
        }
        public void AddSong(Song song) {
            if (File.Exists(song.Path) && Path.GetExtension(song.Path).Equals(".mp3", StringComparison.OrdinalIgnoreCase)) {
                // Generate a unique ID for the song
                song.Id = Guid.NewGuid().ToString();

                var tagFile = TagLib.File.Create(song.Path);
                song.Name = tagFile.Tag.Title;
                song.Duration = tagFile.Properties.Duration;

                _songs.Add(song);
                SaveToJson();
            }
        }

        public void RemoveSong(string songId) {
            _songs.RemoveAll(s => s.Id == songId);

            foreach (var playlist in _playlists) {
                playlist.SongIds.Remove(songId);
            }

            SaveToJson();
        }

        public void AddPlaylist(Playlist playlist) {
            if (_playlists.Find(p => p.Name == playlist.Name) == null) {
                _playlists.Add(playlist);
                SaveToJson();
            } 
        }

        public void RemovePlaylist(string playlistName) {
            _playlists.RemoveAll(p => p.Name == playlistName);

            SaveToJson();
        }

        public void AddSongToPlaylist(string songId, string playlistName) {
            var playlist = _playlists.Find(p => p.Name == playlistName);

            if (playlist != null) {
                // Check if the song is already in the playlist
                if (!playlist.SongIds.Contains(songId)) {
                    playlist.SongIds.Add(songId);
                    SaveToJson();
                }
            }
        }

        public void RemoveSongFromPlaylist(string songId, string playlistName) {
            var playlist = _playlists.Find(p => p.Name == playlistName);

            if (playlist != null) {
                playlist.SongIds.Remove(songId);
                SaveToJson();
            }
        }

        public void MoveSongToPlaylist(string songId, string fromPlaylist, string toPlaylist) {
            var from = _playlists.Find(p => p.Name == fromPlaylist);
            var to = _playlists.Find(p => p.Name == toPlaylist);

            if (from != null && to != null) {
                from.SongIds.Remove(songId);
                to.SongIds.Add(songId);
                SaveToJson();
            }
        }

        public IEnumerable<Song> GetSongs() {
            return _songs;
        }
        public IEnumerable<Playlist> GetPlaylists() {
            return _playlists;
        }
        public List<Song> GetSongsFromPlaylist(string playlistName) {
            var playlist = _playlists.Find(p => p.Name == playlistName);
            var songsFromPlaylist = new List<Song>();

            if (playlist != null) {
                foreach (var songId in playlist.SongIds) {
                    songsFromPlaylist.Add(_songs.Find(p => p.Id == songId));
                }
            }

            return songsFromPlaylist;
        }
    }
}
