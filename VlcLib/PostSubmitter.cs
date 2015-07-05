using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VlcLib
{
    // <summary>
    /// Submits post data to a url.
    /// </summary>
    public class PostSubmitter
    {
        public NetworkCredential Credentials { get; set; }
        /// <summary>
        /// determines what type of post to perform.
        /// </summary>
        public enum PostTypeEnum
        {
            /// <summary>
            /// Does a get against the source.
            /// </summary>
            Get,
            /// <summary>
            /// Does a post against the source.
            /// </summary>
            Post
        }

        public CookieContainer cookieJar { get; set; }
        private string m_url = string.Empty;
        private NameValueCollection m_values = new NameValueCollection();
        private PostTypeEnum m_type = PostTypeEnum.Get;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PostSubmitter()
        {
            this.Referer = "";
            this.cookieJar = new CookieContainer();
        }

        /// <summary>
        /// Constructor that accepts a url as a parameter
        /// </summary>
        /// <param name="url">The url where the post will be submitted to.</param>
        public PostSubmitter(string url)
            : this()
        {
            m_url = url;
        }

        public PostSubmitter(string url, CookieContainer cookies)
            : this()
        {
            m_url = url;
            this.cookieJar = cookies;
        }
        /// <summary>
        /// Constructor allowing the setting of the url and items to post.
        /// </summary>
        /// <param name="url">the url for the post.</param>
        /// <param name="values">The values for the post.</param>
        public PostSubmitter(string url, NameValueCollection values)
            : this(url)
        {
            m_values = values;
        }

        /// <summary>
        /// Gets or sets the url to submit the post to.
        /// </summary>
        public string Url
        {
            get
            {
                return m_url;
            }
            set
            {
                m_url = value;
            }
        }
        /// <summary>
        /// Gets or sets the name value collection of items to post.
        /// </summary>
        public NameValueCollection PostItems
        {
            get
            {
                return m_values;
            }
            set
            {
                m_values = value;
            }
        }
        /// <summary>
        /// Gets or sets the type of action to perform against the url.
        /// </summary>
        public PostTypeEnum Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <returns>a string containing the result of the post.</returns>
        public async Task<string> PostAsync()
        {
            return await PostDataAsync(m_url);
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <param name="url">The url to post to.</param>
        /// <returns>a string containing the result of the post.</returns>
        public async Task<string> PostAsync(string url)
        {
            m_url = url;
            return await this.PostAsync();
        }
        /// <summary>
        /// Posts the supplied data to specified url.
        /// </summary>
        /// <param name="url">The url to post to.</param>
        /// <param name="values">The values to post.</param>
        /// <returns>a string containing the result of the post.</returns>
        public async Task<string> PostAsync(string url, NameValueCollection values)
        {
            m_values = values;
            return await this.PostAsync(url);
        }

        /// <summary>
        /// Posts data to a specified url. Note that this assumes that you have already url encoded the post data.
        /// </summary>
        /// <param name="url">the url to post to.</param>
        /// <returns>Returns the result of the post.</returns>
        /// 
        private async Task<string> PostDataAsync(string url)
        {
            var res = "";
            Uri baseAddress = null;
            baseAddress = new Uri(url);
            using (var handler = new HttpClientHandler() { CookieContainer = this.cookieJar })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                if (Credentials != null)
                {
                    string authInfo = this.Credentials.UserName + ":" + this.Credentials.Password;
                    authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(authInfo));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic", authInfo);
                }
                HttpResponseMessage result = null;
                if (this.Type == PostTypeEnum.Get)
                {
                    var query = "";
                    if (this.m_values.Count > 0)
                    {
                        for (int i = 0; i < this.m_values.Count; i++)
                        {
                            if (query.Length != 0)
                            {
                                query += "&";
                            }
                            else
                            {
                                query += "?";
                            }
                            query += this.m_values.Values[i].Key + "=" + WebUtility.UrlEncode(this.m_values.Values[i].Value);
                        }
                    }
                    result = await client.GetAsync(query);
                }
                else
                {
                    var content = new FormUrlEncodedContent(
                        this.m_values.Values.Select(x => new System.Collections.Generic.KeyValuePair<string, string>(x.Key, x.Value)).ToArray()
                    );
                    result = await client.PostAsync("", content);
                }
                result.EnsureSuccessStatusCode();
                res = await result.Content.ReadAsStringAsync();
            }
            return res;
        }

        public string Referer { get; set; }

    }
}
