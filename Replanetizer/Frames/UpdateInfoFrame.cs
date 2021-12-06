// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Diagnostics;
using ImGuiNET;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Replanetizer.Frames
{
    public class UpdateInfoFrame : Frame
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private string aboutText;
        private string? link;
        protected override string frameName { get; set; } = "New version of Replanetizer is available.";

        public UpdateInfoFrame(Window wnd, string? url, DateTime? buildDate, DateTime? currVersionDate) : base(wnd)
        {
            aboutText = String.Format(@"Your Version: {0}
Current Version: {1}
Link: ", buildDate.ToString(), currVersionDate.ToString());

            link = url;
        }

        public override void Render(float deltaTime)
        {
            ImGui.Text(aboutText);
            if (link != null)
            {
                if (ImGui.Button(link))
                {
                    Process.Start("explorer", link);
                }
            }
            else
            {
                ImGui.Text("Woops. No link available.");
            }
        }

        public static async void CheckForNewVersion(Window wnd)
        {
            //Builtin only exists during building, your IDE may complain
            DateTime compileTime = new DateTime(Builtin.CompileTime, DateTimeKind.Utc);

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.UseDefaultCredentials = true;
                    handler.UseProxy = false;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/RatchetModding/Replanetizer/releases/latest");

                        //Github wants a user agent, otherwise it returns code 403 Forbidden
                        requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");

                        HttpResponseMessage response = await client.SendAsync(requestMessage);
                        response.EnsureSuccessStatusCode();
                        string content = await response.Content.ReadAsStringAsync();

                        JObject? data = (JObject?) Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                        if (data != null)
                        {
                            string? time = (string?) data["created_at"];
                            string? url = (string?) data["html_url"];

                            if (time != null)
                            {
                                CultureInfo cultureInfo = new CultureInfo("en-US");
                                DateTime currVersionDate = DateTime.Parse(time, cultureInfo);

                                if (currVersionDate.CompareTo(compileTime.AddHours(6.0)) > 0)
                                {
                                    UpdateInfoFrame frame = new UpdateInfoFrame(wnd, url, compileTime, currVersionDate);
                                    wnd.AddFrame(frame);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                LOGGER.Warn("Failed to check for new version. Error: " + e.Message);
            }
        }
    }
}
