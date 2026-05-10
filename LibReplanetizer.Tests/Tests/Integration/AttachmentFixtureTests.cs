// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Linq;
using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using static LibReplanetizer.DataFunctions;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class AttachmentFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static MobyModel? GetFirstModelWithAttachments(EngineParser parser)
        {
            return parser.GetMobyModels()
                         .Cast<MobyModel>()
                         .FirstOrDefault(m => m.attachments.Count > 0);
        }

        [SkippableFact]
        public void AtLeastOneModel_HasAttachments()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAttachments(parser);
            Skip.If(model == null, "No moby model with attachments found in this level.");
            Assert.NotEmpty(model!.attachments);
        }

        [SkippableFact]
        public void Attachment_ABones_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAttachments(parser);
            Skip.If(model == null, "No moby model with attachments found in this level.");
            foreach (var att in model!.attachments)
                Assert.NotNull(att.aBones);
        }

        [SkippableFact]
        public void Attachment_BBones_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAttachments(parser);
            Skip.If(model == null, "No moby model with attachments found in this level.");
            foreach (var att in model!.attachments)
                Assert.NotNull(att.bBones);
        }

        [SkippableFact]
        public void Attachment_Serialize_CountsPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAttachments(parser);
            Skip.If(model == null, "No moby model with attachments found in this level.");
            foreach (var att in model!.attachments)
            {
                byte[] data = att.Serialize();
                Assert.Equal((short) att.aBones.Count, ReadShort(data, 0));
                Assert.Equal((short) att.bBones.Count, ReadShort(data, 2));
            }
        }

        [SkippableFact]
        public void Attachment_Serialize_LengthMultipleOf4()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var model = GetFirstModelWithAttachments(parser);
            Skip.If(model == null, "No moby model with attachments found in this level.");
            foreach (var att in model!.attachments)
                Assert.Equal(0, att.Serialize().Length % 4);
        }
    }
}
