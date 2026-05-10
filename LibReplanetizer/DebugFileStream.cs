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
using System.Text;

namespace LibReplanetizer
{
    /// <summary>
    /// Type of a file access recorded by <see cref="DebugFileStream"/>.
    /// </summary>
    public enum AccessType
    {
        Read,
        Write
    }

    /// <summary>
    /// A single (possibly merged) entry in the <see cref="DebugFileStream"/> access log.
    /// <para>
    /// <see cref="Start"/> is inclusive, <see cref="End"/> is exclusive — i.e. the entry
    /// covers bytes [Start, End).
    /// </para>
    /// </summary>
    public sealed class AccessEntry
    {
        public long Start { get; internal set; }
        public long End { get; internal set; }
        public AccessType Type { get; }
        public string StackTrace { get; }

        public long Length => End - Start;

        internal AccessEntry(long start, long end, AccessType type, string stackTrace)
        {
            Start = start;
            End = end;
            Type = type;
            StackTrace = stackTrace;
        }

        public override string ToString()
        {
            string typeTag = Type == AccessType.Read ? "READ " : "WRITE";
            return $"[{typeTag}] 0x{Start:X8}–0x{End:X8} ({Length} bytes)\n{StackTrace}";
        }
    }

    /// <summary>
    /// A <see cref="FileStream"/> subclass that, in DEBUG builds, records every
    /// <see cref="Read"/> / <see cref="Write"/> call together with the caller's stack trace
    /// and the byte range accessed in the file.
    /// <para>
    /// Adjacent accesses of the <em>same type</em> at <em>contiguous</em> offsets that share
    /// an identical stack trace are automatically merged into a single <see cref="AccessEntry"/>,
    /// so the log stays compact even for streaming reads/writes.
    /// </para>
    /// <para>
    /// All tracking logic is compiled out in Release builds (<c>#if DEBUG</c>), so there is
    /// zero overhead outside of debug sessions.
    /// </para>
    /// </summary>
    public class DebugFileStream : FileStream
    {
        // ------------------------------------------------------------------ //
        //  Configuration                                                       //
        // ------------------------------------------------------------------ //

        private readonly int _stackDepth;

        /// <summary>
        /// Environment variable that, when set, specifies the directory to write access logs
        /// into. Each <see cref="DebugFileStream"/> writes a separate file named after the
        /// opened file. If the variable is unset, no log file is written.
        /// </summary>
        public const string LOG_DIR_ENV_VAR = "REPLANETIZER_ACCESS_LOG_DIR";

        /// <summary>
        /// Environment variable that controls how many stack frames are captured per access
        /// entry. If unset, defaults to 8.
        /// </summary>
        public const string STACK_DEPTH_ENV_VAR = "REPLANETIZER_ACCESS_LOG_STACK_DEPTH";

        private const int DEFAULT_STACK_DEPTH = 8;

        private static int ReadStackDepth()
        {
            string? raw = Environment.GetEnvironmentVariable(STACK_DEPTH_ENV_VAR);
            if (raw != null && int.TryParse(raw, out int depth) && depth > 0)
                return depth;
            return DEFAULT_STACK_DEPTH;
        }

        // ------------------------------------------------------------------ //
        //  In-memory logs (always present, empty in Release)                  //
        // ------------------------------------------------------------------ //

        private readonly List<AccessEntry> _readLog = new();
        private readonly List<AccessEntry> _writeLog = new();

        // Whether to track each access type — false when the stream was opened
        // with an access mode that excludes that operation.
        private readonly bool _trackReads;
        private readonly bool _trackWrites;

        /// <summary>
        /// All read access entries recorded so far, sorted by <see cref="AccessEntry.Start"/>.
        /// In Release builds this list is always empty.
        /// </summary>
        public IReadOnlyList<AccessEntry> ReadLog => _readLog;

        /// <summary>
        /// All write access entries recorded so far, sorted by <see cref="AccessEntry.Start"/>.
        /// In Release builds this list is always empty.
        /// </summary>
        public IReadOnlyList<AccessEntry> WriteLog => _writeLog;

        // ------------------------------------------------------------------ //
        //  Constructors                                                        //
        // ------------------------------------------------------------------ //

        /// <param name="path">Path to the file.</param>
        /// <param name="mode">File open mode.</param>
        public DebugFileStream(
            string path,
            FileMode mode)
            : base(path, mode)
        {
            _stackDepth = ReadStackDepth();
            _trackReads = true;
            _trackWrites = true;
        }

        /// <param name="path">Path to the file.</param>
        /// <param name="mode">File open mode.</param>
        /// <param name="access">Desired file access.</param>
        public DebugFileStream(
            string path,
            FileMode mode,
            FileAccess access)
            : base(path, mode, access)
        {
            _stackDepth = ReadStackDepth();
            _trackReads  = access == FileAccess.Read  || access == FileAccess.ReadWrite;
            _trackWrites = access == FileAccess.Write || access == FileAccess.ReadWrite;
        }

        // ------------------------------------------------------------------ //
        //  Core tracking                                                       //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Records that <paramref name="length"/> bytes were accessed at
        /// <paramref name="offset"/> in the file with the given <paramref name="type"/>.
        /// Adjacent entries that share both type and stack trace are merged.
        /// Compiled out entirely in Release builds.
        /// </summary>
        private void RecordAccess(long offset, int length, AccessType type)
        {
#if DEBUG
            if (length <= 0)
                return;

            // Capture a short stack trace.  Skip 2 frames: RecordAccess itself,
            // plus the Read/Write override that called us.
            var rawTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
            var frames = rawTrace.GetFrames();

            int frameCount = Math.Min(frames?.Length ?? 0, _stackDepth);
            var sb = new StringBuilder();
            for (int i = 0; i < frameCount; i++)
            {
                var frame = frames![i];
                var method = frame.GetMethod();
                if (method == null) continue;
                string methodSig = $"{method.DeclaringType?.FullName}.{method.Name}({string.Join(", ", Array.ConvertAll(method.GetParameters(), p => p.ParameterType.Name))})";
                int line = frame.GetFileLineNumber();
                if (line > 0)
                {
                    string? file = Path.GetFileName(frame.GetFileName());
                    sb.AppendLine($"   at {methodSig} in {file}:{line}");
                }
                else
                {
                    sb.AppendLine($"   at {methodSig}");
                }
            }
            string traceString = sb.ToString();

            long end = offset + length;
            List<AccessEntry> log = type == AccessType.Read ? _readLog : _writeLog;

            // Binary search for the sorted insertion point by Start address.
            int lo = 0, hi = log.Count - 1, insertAt = log.Count;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (log[mid].Start < offset)
                    lo = mid + 1;
                else
                {
                    insertAt = mid;
                    hi = mid - 1;
                }
            }

            // Try to merge with the predecessor (the entry whose range ends exactly here).
            int predIdx = insertAt - 1;
            if (predIdx >= 0)
            {
                AccessEntry pred = log[predIdx];
                if (pred.Type == type && pred.End == offset && pred.StackTrace == traceString)
                {
                    pred.End = end;
                    insertAt = predIdx; // merged entry now lives at predIdx
                    goto tryMergeSuccessor;
                }
            }

            log.Insert(insertAt, new AccessEntry(offset, end, type, traceString));

            tryMergeSuccessor:
            // Try to merge the entry at insertAt with its successor.
            int succIdx = insertAt + 1;
            if (succIdx < log.Count)
            {
                AccessEntry curr = log[insertAt];
                AccessEntry succ = log[succIdx];
                if (curr.Type == succ.Type && curr.End == succ.Start && curr.StackTrace == succ.StackTrace)
                {
                    curr.End = succ.End;
                    log.RemoveAt(succIdx);
                }
            }
#endif
        }

        // ------------------------------------------------------------------ //
        //  FileStream overrides                                                //
        // ------------------------------------------------------------------ //

        public override int Read(byte[] buffer, int offset, int count)
        {
            long pos = Position;
            int bytesRead = base.Read(buffer, offset, count);
            RecordAccess(pos, bytesRead, AccessType.Read);
            return bytesRead;
        }

        public override int ReadByte()
        {
            long pos = Position;
            int result = base.ReadByte();
            if (result != -1)
                RecordAccess(pos, 1, AccessType.Read);
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long pos = Position;
            base.Write(buffer, offset, count);
            RecordAccess(pos, count, AccessType.Write);
        }

        public override void WriteByte(byte value)
        {
            long pos = Position;
            base.WriteByte(value);
            RecordAccess(pos, 1, AccessType.Write);
        }

        // ------------------------------------------------------------------ //
        //  Log output                                                          //
        // ------------------------------------------------------------------ //

        private const long GAP_THRESHOLD = 16;

        /// <summary>
        /// Writes one access log (reads or writes) to <paramref name="path"/> in a
        /// human-readable format, interleaving "NOT ACCESSED" entries for any gap in the
        /// file larger than <see cref="GAP_THRESHOLD"/> bytes.
        /// Only meaningful in DEBUG builds.
        /// </summary>
        public void WriteLogToFile(string path, AccessType type, long fileLength)
        {
            List<AccessEntry> log = type == AccessType.Read ? _readLog : _writeLog;
            string typeLabel = type == AccessType.Read ? "READ" : "WRITE";
            var sb = new StringBuilder();
            sb.AppendLine($"DebugFileStream {typeLabel} access log — {log.Count} entries");
            sb.AppendLine($"File: {Name}");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();

            long cursor = 0;
            foreach (AccessEntry entry in log)
            {
                long gapSize = entry.Start - cursor;
                if (gapSize > GAP_THRESHOLD)
                {
                    sb.AppendLine($"[-----] 0x{cursor:X8}–0x{entry.Start:X8} ({gapSize} bytes) NOT ACCESSED");
                    sb.AppendLine("");
                    sb.AppendLine("---");
                }

                cursor = entry.End;

                sb.AppendLine(entry.ToString());
                sb.AppendLine("---");
            }

            // Trailing gap after the last entry.
            if (fileLength - cursor > GAP_THRESHOLD)
            {
                sb.AppendLine($"[-----] 0x{cursor:X8}–0x{fileLength:X8} ({fileLength - cursor} bytes) NOT ACCESSED");
                sb.AppendLine("");
                sb.AppendLine("---");
            }

            File.WriteAllText(path, sb.ToString());
        }

        // ------------------------------------------------------------------ //
        //  Dispose                                                             //
        // ------------------------------------------------------------------ //

        protected override void Dispose(bool disposing)
        {
#if DEBUG
            if (disposing)
            {
                string? logDir = Environment.GetEnvironmentVariable(LOG_DIR_ENV_VAR);
                if (logDir != null)
                {
                    string baseName = Path.GetFileName(Name);
                    long fileLength = Length;
                    try
                    {
                        if (_trackReads)
                            WriteLogToFile(Path.Combine(logDir, baseName + ".reads.accesslog.txt"), AccessType.Read, fileLength);
                        if (_trackWrites)
                            WriteLogToFile(Path.Combine(logDir, baseName + ".writes.accesslog.txt"), AccessType.Write, fileLength);
                    }
                    catch (Exception)
                    {
                        // Best-effort — do not let log writing prevent the stream from closing.
                    }
                }
            }
#endif
            base.Dispose(disposing);
        }
    }
}
