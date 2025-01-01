// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Replanetizer.Utils
{
    public static class CrossFileDialog
    {
        public class NoImplementationException : Exception { }

        /// <summary>
        /// Shows a Open File dialog to pick a filename.
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="filter">File extension filter, eg: .bmp;.jpg|.dds</param>
        public static string OpenFile(string title = "Choose a file", string filter = "")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (PrefersKdialog())
                    return KDialog.OpenFile(title, filter);

                return Zenity.OpenFile(title, filter);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Win32Dialog.OpenFile(title, filter);

            throw new NoImplementationException();
        }

        public static List<string> OpenMultipleFiles(string title = "Choose one or more files")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (PrefersKdialog())
                    return KDialog.OpenMultipleFiles(title);

                return Zenity.OpenMultipleFiles(title);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Win32Dialog.OpenMultipleFiles(title);

            throw new NoImplementationException();
        }

        public static string SaveFile(string title = "Enter the name of the file to save to", string filter = "")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (PrefersKdialog())
                    return KDialog.SaveFile(title);

                return Zenity.SaveFile(title);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Win32Dialog.SaveFile(title, filter);

            throw new NoImplementationException();
        }

        public static string OpenFolder(string title = "Choose a folder")
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (PrefersKdialog())
                    return KDialog.OpenFolder(title);

                return Zenity.OpenFolder(title);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Win32Dialog.OpenFolder(title);

            throw new NoImplementationException();
        }

        private static bool ExecutableInPath(string fileName)
        {
            if (File.Exists(fileName))
                return true;

            String? values = Environment.GetEnvironmentVariable("PATH");

            if (values != null)
            {
                foreach (var path in values.Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(path, fileName);
                    if (File.Exists(fullPath))
                        return true;
                }
            }

            return false;
        }

        private static bool PrefersKdialog()
        {
            string? desktopSession = Environment.GetEnvironmentVariable("DESKTOP_SESSION");
            string? xdgCurrentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");

            bool probablyKde = (desktopSession != null && desktopSession.ToLower().Equals("kde")) ||
                               (xdgCurrentDesktop != null && xdgCurrentDesktop.ToLower().Equals("kde"));

            bool hasKdialogBinary = ExecutableInPath("kdialog");
            bool hasZenityBinary = ExecutableInPath("zenity");

            if (hasKdialogBinary && (probablyKde || !hasZenityBinary))
                return true;

            if (hasZenityBinary)
                return false;

            throw new NoImplementationException();
        }

        private static string RunProcess(string filename, List<string> args)
        {
            Process process = new Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            foreach (string arg in args)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();

            return output.Replace("\n", "").Replace("\r", "");
        }

        private static class KDialog
        {
            private static string RunKDialog(List<string>? args = null, Dictionary<string, string>? kwargs = null)
            {
                List<string> zArgs = new List<string>();
                if (args != null)
                    foreach (string arg in args)
                        zArgs.Add("--" + arg);

                if (kwargs != null)
                    foreach (var entry in kwargs)
                    {
                        zArgs.Add("--" + entry.Key);
                        zArgs.Add(entry.Value);
                    }

                return RunProcess("kdialog", zArgs);
            }

            public static string OpenFile(string title, string filter = "")
            {
                return RunKDialog(new List<string>() { "getopenfilename" },
                    new Dictionary<string, string>() { { "title", title } });
            }

            public static List<string> OpenMultipleFiles(string title)
            {
                var result = RunKDialog(new List<string>() { "getopenfilename", "multiple" },
                    new Dictionary<string, string>() { { "title", title } });

                return result.Split(' ').ToList();
            }

            public static string SaveFile(string title)
            {
                return RunKDialog(new List<string>() { "getsavefilename" },
                    new Dictionary<string, string>() { { "title", title } });
            }

            public static string OpenFolder(string title)
            {
                return RunKDialog(new List<string>() { "getexistingdirectory" },
                    new Dictionary<string, string>() { { "title", title } });
            }
        }

        private static class Zenity
        {
            private static string RunZenity(List<string>? args = null, Dictionary<string, string>? kwargs = null)
            {
                List<string> zArgs = new List<string>();
                if (args != null)
                    foreach (string arg in args)
                        zArgs.Add("--" + arg);

                if (kwargs != null)
                    foreach (var entry in kwargs)
                        zArgs.Add("--" + entry.Key + "=" + entry.Value);

                return RunProcess("zenity", zArgs);
            }

            public static string OpenFile(string title, string filter = "")
            {
                return RunZenity(new List<string>() { "file-selection" },
                    new Dictionary<string, string>() { { "title", title } });
            }

            public static List<string> OpenMultipleFiles(string title)
            {
                var result = RunZenity(new List<string>() { "file-selection", "multiple" },
                    new Dictionary<string, string>() { { "title", title } });

                return result.Split("|").ToList();
            }

            public static string SaveFile(string title)
            {
                return RunZenity(new List<string>() { "file-selection", "save", "confirm-overwrite" },
                    new Dictionary<string, string>() { { "title", title } });
            }

            public static string OpenFolder(string title)
            {
                return RunZenity(new List<string>() { "file-selection", "directory" },
                    new Dictionary<string, string>() { { "title", title } });
            }
        }

        private static class Win32Dialog
        {
            private static string BuildFilterList(string filter)
            {
                var wildcardFound = false;
                var filters = new List<string>();
                foreach (string group in filter.Split('|'))
                {
                    var extensions = new List<string>();
                    foreach (string f in group.Split(';'))
                    {
                        if (f == ".*") wildcardFound = true;
                        extensions.Add($"*{f}");
                    }
                    var extString = String.Join(';', extensions);
                    filters.Add($"Supported files ({extString})|{extString}");
                }

                if (!wildcardFound)
                {
                    filters.Add("All files (*.*)|*.*");
                }
                return String.Join('|', filters);
            }

            public static string OpenFile(string title, string filter = "")
            {
#if _WINDOWS
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Title = title;
                dialog.Filter = BuildFilterList(filter);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName;
                }
                else
                {
                    return string.Empty;
                }
#else
                throw new NoImplementationException();
#endif
            }

            public static List<string> OpenMultipleFiles(string title)
            {
#if _WINDOWS
                throw new NoImplementationException();
#else
                throw new NoImplementationException();
#endif
            }

            public static string SaveFile(string title, string filter = "")
            {
#if _WINDOWS
                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Title = title;
                dialog.Filter = BuildFilterList(filter);
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName;
                }
                else
                {
                    return string.Empty;
                }
#else
                throw new NoImplementationException();
#endif
            }

            public static string OpenFolder(string title)
            {
#if _WINDOWS
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = title;
                dialog.UseDescriptionForTitle = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
                else
                {
                    return string.Empty;
                }
#else
                throw new NoImplementationException();
#endif
            }
        }
    }
}
