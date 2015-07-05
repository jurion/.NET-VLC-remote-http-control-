using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VlcLib.Helpers;
using VlcLib.Media;
using VlcLib.Status;

namespace VlcLib
{
    public enum SortOrder
    {
        NameAscending,
        NameDescending,
        LastDateAscending,
        LastDateDescending
    }

    public class VlcWebControler
    {

        private NetworkCredential creds;
        private string baseUrl = "";
        private string[] typeNameLoc = new string[] { "Type", "Type ", "Тип" };
        private string[] typeNameVidLoc = new string[] { "Video", "Vidéo", "Видео" };
        private string[] typeNameSubsLoc = new string[] { "Subtitle", "Sous-titres", "Субтитры" };
        private string[] typeNameAudioLoc = new string[] { "Audio", "Audio", "Аудио" };

        private PostSubmitter GetSubmiter(string url)
        {
            PostSubmitter ps = new PostSubmitter(url);
            ps.Type = PostSubmitter.PostTypeEnum.Get;
            ps.Credentials = creds;
            return ps;
        }

        private PostSubmitter GetSubmiterForCommand(string command)
        {
            PostSubmitter ps = GetSubmiter(baseUrl + "requests/status.xml");
            ps.PostItems.AddItem("command", command);
            return ps;
        }

        public VlcWebControler(string url, string password)
        {
            creds = new NetworkCredential("", password);
            baseUrl = url;
        }

        public async Task<bool> TestConnexion()
        {
            try
            {
                PostSubmitter ps = GetSubmiter(baseUrl);
                var res = await ps.PostAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<VlcMediaItem>> GetMediaList(string distantPath, SortOrder order)
        {
            var res = await GetMediaList(distantPath);
            switch (order)
            {
                case SortOrder.NameAscending:
                    res = (from r in res orderby r.Name select r).ToList();
                    break;
                case SortOrder.NameDescending:
                    res = (from r in res orderby r.Name descending select r).ToList();
                    break;
                case SortOrder.LastDateAscending:
                    res = (from r in res orderby r.LastAccess select r).ToList();
                    break;
                case SortOrder.LastDateDescending:
                    res = (from r in res orderby r.LastAccess descending select r).ToList();
                    break;
                default:
                    break;
            }
            return res;
        }

        public async Task<VlcStatus> GetStatus()
        {
            PostSubmitter ps = GetSubmiter(baseUrl + "requests/status.xml");
            var res = await ps.PostAsync();
            XDocument doc = XDocument.Parse(res);
            //doc.LoadXml(res);
            VlcStatus result = new VlcStatus();
            var fs = XmlHelper.SelectElement(doc.Root, "fullscreen");
            if (fs != null)
            {
                if (fs.Value == "0") 
                {
                    result.IsFullScreen = false;
                }
                else
                {
                    result.IsFullScreen = Convert.ToBoolean(fs.Value);
                }
            }
            result.AudioDelay = Convert.ToDouble(XmlHelper.SelectElement(doc.Root, "audiodelay").Value);
            result.SubsDelay = Convert.ToDouble(XmlHelper.SelectElement(doc.Root, "subtitledelay").Value);
            var state = XmlHelper.SelectElement(doc.Root, "state").Value.Trim();
            switch (state)
            {
                case "paused":
                    result.State = VlcState.Paused;
                    break;
                case "playing":
                    result.State = VlcState.Playing;
                    break;
                default:
                    result.State = VlcState.Stopped;
                    break;
            }
            result.VlcVersion = XmlHelper.SelectElement(doc.Root, "version").Value.Trim();
            result.Duration = TimeSpan.FromSeconds(Convert.ToInt32(XmlHelper.SelectElement(doc.Root, "length").Value));
            result.CurrentPossiton = TimeSpan.FromSeconds(Convert.ToInt32(XmlHelper.SelectElement(doc.Root, "time").Value));
            var infos = XmlHelper.SelectElements(doc.Root, "information/category");
            foreach (var item in infos)
            {
                var name = item.Attribute("name").Value;
                if (name == "meta")
                {
                    var serie = XmlHelper.GetNodeByNameAttribute(item, "info", "name", "showName");
                    if (serie != null)
                    {
                        var titre = XmlHelper.GetNodeByNameAttribute(item, "info", "name", "title");
                        if (titre != null)
                        {
                            result.CurrentllyPlaying = titre.Value.Trim();
                        }
                        else
                        {
                            result.CurrentllyPlaying = serie.Value.Trim();
                        }
                    }
                    else
                    {
                        var fname = XmlHelper.GetNodeByNameAttribute(item, "info", "name", "filename");
                        if (fname != null)
                        {
                            result.CurrentllyPlaying = fname.Value.Trim();
                        }
                        else
                        {
                            result.CurrentllyPlaying = "";
                        }
                    }
                }
                if (name.IndexOf(' ') > 0)
                {
                    //FLUUUUUUXUX
                    VlcFlux flux = new VlcFlux();
                    flux.Id = int.Parse(name.Split(' ')[1]);
                    for (int i = 0; i < typeNameLoc.Length; i++)
                    {
                        var typeNode = XmlHelper.GetNodeByNameAttribute(item, "info", "name", typeNameLoc[i]);
                        if (typeNode != null)
                        {
                            var typeText = typeNode.Value.Trim();
                            if (typeNameAudioLoc.Contains(typeText))
                            {
                                flux.FluxType = FluxType.Audio;
                                break;
                            }
                            else if (typeNameSubsLoc.Contains(typeText))
                            {
                                flux.FluxType = FluxType.SubTitles;
                                break;
                            }
                            else if (typeNameVidLoc.Contains(typeText))
                            {
                                flux.FluxType = FluxType.Video;
                                break;
                            }
                        }
                    }
                    result.Fluxs.Add(flux);
                    flux.Name = "Stream " + (from r in result.Fluxs where r.FluxType == flux.FluxType select r).Count();

                }
            }

            return result;

        }

        private async Task<List<VlcMediaItem>> GetMediaList(string distantPath)
        {
            List<VlcMediaItem> result = new List<VlcMediaItem>();
            if (distantPath.EndsWith(".."))
            {
                return result;
            }
            string uri = !distantPath.StartsWith("file") ? new Uri(distantPath).ToString() : distantPath;

            PostSubmitter ps = GetSubmiter(baseUrl + "requests/browse.xml");
            ps.PostItems.AddItem("uri", uri.ToString());
            var res = await ps.PostAsync();
            XDocument doc = XDocument.Parse(res);
            var nodes = XmlHelper.GetNodeArrayByNameAttribute(doc.Root, "element", "type", "dir");
            foreach (var currentNode in nodes)
            {
                List<VlcMediaItem> newFiles = await GetMediaList(Uri.UnescapeDataString(currentNode.Attribute("uri").Value));
                result.AddRange(newFiles);
            }
            var files = (from n in XmlHelper.GetNodeArrayByNameAttribute(doc.Root, "element", "type", "file")
                         let media = new VlcMediaItem(n)
                         where media.MediaType != MediaType.None
                         select media).ToList();
            result.AddRange(files);
            return result;
        }

        public async Task<bool> PlayFile(VlcMediaItem file)
        {
            var ps = GetSubmiterForCommand("in_play");
            ps.PostItems.AddItem("input", Uri.EscapeUriString(new Uri(file.FileName).ToString()));
            var res = await ps.PostAsync();

            return true;
        }

        public async void Pause()
        {
            var ps = GetSubmiterForCommand("pl_forcepause");
            await ps.PostAsync();
        }

        public async void PlatLast()
        {
            var ps = GetSubmiterForCommand("pl_play");
            await ps.PostAsync();
        }

        public async void PlayResume()
        {
            var ps = GetSubmiterForCommand("pl_forceresume");
            await ps.PostAsync();
        }

        public async void Stop()
        {
            var ps = GetSubmiterForCommand("pl_stop");
            await ps.PostAsync();
        }

        public async void ToggleFullScreen()
        {
            var ps = GetSubmiterForCommand("fullscreen");
            await ps.PostAsync();
        }

        public async void AdvanceSeconds(int seconds)
        {
            var ps = GetSubmiterForCommand("seek");
            ps.PostItems.AddItem("val", seconds > 0 ? "+" + seconds.ToString() : seconds.ToString());
            await ps.PostAsync();
        }

        public async void SetSubsDelay(decimal seconds)
        {
            var ps = GetSubmiterForCommand("subdelay");
            var v = seconds.ToString();
            ps.PostItems.AddItem("val", v);
            await ps.PostAsync();
        }
    }
}
