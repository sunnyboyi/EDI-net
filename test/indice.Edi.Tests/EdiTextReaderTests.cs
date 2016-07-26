﻿using indice.Edi.Tests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace indice.Edi.Tests
{
    public class EdiTextReaderTests
    {
        private static readonly Assembly _assembly = typeof(EdiTextReaderTests).GetTypeInfo().Assembly;
        private static Stream GetResourceStream(string fileName) {
            var qualifiedResources = _assembly.GetManifestResourceNames().OrderBy(x => x).ToArray();
            Stream stream = _assembly.GetManifestResourceStream("indice.Edi.Tests.Samples." + fileName);
            return stream;
        }

        private static MemoryStream StreamFromString(string value) {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        [Fact]
        public void ReaderTest() {
            var msgCount = 0;
            var grammar = EdiGrammar.NewTradacoms();

            using (var ediReader = new EdiTextReader(new StreamReader(GetResourceStream("tradacoms.order9.edi")), grammar)) {
                while (ediReader.Read()) {
                    if (ediReader.IsStartMessage) {
                        msgCount++;
                    }
                }
            }
            Assert.Equal(4, msgCount);
        }

        [Fact]
        public void DeserializeTest() {
            var grammar = EdiGrammar.NewTradacoms();
            var interchange = default(Interchange);
            using (var stream = GetResourceStream("tradacoms.utilitybill.edi")) {
                interchange = new EdiSerializer().Deserialize<Interchange>(new StreamReader(stream), grammar);
            }
            Assert.Equal(1, interchange.Invoices.Count);
        }

        [Fact]
        public void EscapeCharactersTest() {
            var grammar = EdiGrammar.NewTradacoms();
            var interchange = default(Interchange);
            using (var stream = GetResourceStream("tradacoms.utilitybill.escape.edi")) {
                interchange = new EdiSerializer().Deserialize<Interchange>(new StreamReader(stream), grammar);
            }
            Assert.Equal("GEORGE'S FRIED CHIKEN + SONS. Could be the best chicken yet?", interchange.Head.ClientName);
        }

        [Fact]
        public void EdiFactTest() {
            var grammar = EdiGrammar.NewEdiFact();
            var messages = default(Test);
            using (var stream = GetResourceStream("edifact.edi")) {
                messages = new EdiSerializer().Deserialize<Test>(new StreamReader(stream), grammar);
            }
            Assert.NotNull(messages);
        }

        [Serialization.EdiSegment, Serialization.EdiPath("DTM")]
        public class MessageCreationInfo
        {
            [Serialization.EdiValue("X(3)", Path = "DTM/0")]
            public string Code { get; set; }
            [Serialization.EdiValue("X(12)", Path = "DTM/0/1")]
            public string Date { get; set; }
        }

        public class Test
        {
            public List<MessageTest> Messages { get; set; }
        }

        [Serialization.EdiMessage]
        public class MessageTest
        {

            public List<MessageCreationInfo> CreationInfos { get; set; }
        }
    }
}
