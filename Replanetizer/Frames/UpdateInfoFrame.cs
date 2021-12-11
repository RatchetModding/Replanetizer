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
using System.Collections.Generic;
using System.Reflection;

namespace Replanetizer.Frames
{
    public class UpdateInfoFrame : Frame
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        private string aboutText;
        private string? link;
        protected override string frameName { get; set; } = "New version of Replanetizer is available.";
        private List<string>? commitMessages = null;

        private async void GetCommitMessagesDiff(string newestReleaseTag)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var versionAttr = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (versionAttr == null) return;

            string gitTreeStatus = versionAttr.InformationalVersion;

            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.UseDefaultCredentials = true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/RatchetModding/Replanetizer/compare/" + gitTreeStatus + "..." + newestReleaseTag);

                        //Github wants a user agent, otherwise it returns code 403 Forbidden
                        requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");

                        HttpResponseMessage response = await client.SendAsync(requestMessage);
                        response.EnsureSuccessStatusCode();
                        string content = await response.Content.ReadAsStringAsync();

                        JObject? data = (JObject?) Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                        if (data != null)
                        {
                            JArray? commits = (JArray?) data["commits"];

                            if (commits != null)
                            {
                                commitMessages = new List<string>();

                                foreach (JObject message in commits)
                                {
                                    JObject? commit = (JObject?) message["commit"];
                                    if (commit == null) continue;

                                    string? text = (string?) commit["message"];
                                    if (text == null) continue;

                                    text = "- " + text;

                                    // Some commit messages have secondary messages, remove them.
                                    int cut = text.IndexOf('\n');

                                    if (cut != -1)
                                    {
                                        text = text.Substring(0, cut);
                                    }

                                    // Merge commits are not useful to the user, identify them by committer being web-flow
                                    JObject? committer = (JObject?) message["committer"];
                                    if (committer == null) continue;

                                    string? login = (string?) committer["login"];
                                    if (login == null || login == "web-flow") continue;

                                    commitMessages.Add(text);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LOGGER.Warn("Failed to check for newest release tag. Error: " + e.Message);
            }
        }

        public UpdateInfoFrame(Window wnd, string? url, DateTime? buildDate, DateTime? currVersionDate, string? newestReleaseTag) : base(wnd)
        {
            aboutText = String.Format(@"Your Version: {0}
Current Version: {1}

Link: ", buildDate.ToString(), currVersionDate.ToString());

            link = url;

            if (newestReleaseTag != null)
            {
                GetCommitMessagesDiff(newestReleaseTag);
            }
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

            if (commitMessages != null)
            {
                ImGui.NewLine();
                if (ImGui.TreeNodeEx("Changes (Commit Messages)", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    foreach (string str in commitMessages)
                    {
                        ImGui.Text(str);
                    }
                    ImGui.TreePop();
                }
            }
        }

        public override void RenderAsWindow(float deltaTime)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(0, 0));
            ImGui.SetNextWindowSizeConstraints(new System.Numerics.Vector2(16, 16), new System.Numerics.Vector2(1280, 720));
            if (ImGui.Begin(frameName, ref isOpen, ImGuiWindowFlags.HorizontalScrollbar))
            {
                Render(deltaTime);
                ImGui.End();
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
                            string? newestReleaseTag = (string?) data["tag_name"];

                            if (time != null)
                            {
                                CultureInfo cultureInfo = new CultureInfo("en-US");
                                DateTime currVersionDate = DateTime.Parse(time, cultureInfo);

                                if (currVersionDate.CompareTo(compileTime.AddHours(6.0)) > 0)
                                {
                                    UpdateInfoFrame frame = new UpdateInfoFrame(wnd, url, compileTime, currVersionDate, newestReleaseTag);
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
