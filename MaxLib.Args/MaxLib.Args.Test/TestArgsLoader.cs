using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MaxLib.Args.Test
{
    [TestClass]
    public class TestArgsLoader
    {
        [TestMethod]
        public void BasicTest()
        {
            var expected = new Arguments
            {
                Options = { { "foo", new List<string> { "bar" } } },
                PositionalCommands = { (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar", "baz" }));
        }

        [TestMethod]
        public void IgnoreCaseTest()
        {
            var expected = new Arguments
            {
                Options = { { "foo", new List<string> { "bar" } } },
                PositionalCommands = { (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--FoO", "bar", "baz" }));
        }

        [TestMethod]
        public void RepeatedArgsTest()
        {
            var expected = new Arguments
            {
                Options = { { "foo", new List<string> { "bar1", "bar2" } } },
                PositionalCommands = { (4, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar1", "--foo", "bar2", "baz" }));
        }

        [TestMethod]
        public void BasicFlagsTest()
        {
            var expected = new Arguments
            {
                Options = { { "foo", new List<string> { null, "bar" } } },
                PositionalCommands = { (3, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "--foo", "bar", "baz" }));
        }

        [TestMethod]
        public void NoOptionsTest()
        {
            var expected = new Arguments
            {
                PositionalCommands = { (0, "--foo"), (1, "/bar"), (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = false,
                UseSlashOptions = false,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "/bar", "baz" }));
        }

        [TestMethod]
        public void SingleDashFlagsTest()
        {
            var expected = new Arguments
            {
                Options = 
                { 
                    { "foo", new List<string> { "bar" } },
                    { "a", new List<string> { null } },
                    { "b", new List<string> { null } },
                },
                PositionalCommands = { (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
                UseSingleDashOptionFlags = true
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar", "baz", "-ab" }));
        }

        [TestMethod]
        public void NegateFlagsTest()
        {
            var expected = new Arguments
            {
                Options = 
                { 
                    { "foo", new List<string> { "bar" } },
                    { "!a", new List<string> { null } },
                    { "!b", new List<string> { null } },
                },
                PositionalCommands = { (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
                UseSingleDashOptionFlags = true,
                NegateFlags = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar", "baz", "-!ab" }));
        }

        [TestMethod]
        public void KeepIndicatorTest()
        {
            var expected = new Arguments
            {
                Options = { { "--foo", new List<string> { "bar" } } },
                PositionalCommands = { (2, "baz") },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
                TrimOptionIndicator = false,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar", "baz" }));
        }

        [TestMethod]
        public void PaseThroughTest()
        {
            var expected = new Arguments
            {
                Options = { { "foo", new List<string> { "bar" } } },
                PositionalCommands = { (2, "baz") },
                PassedThrough = { "--foo", "baz" },
            };
            var loader = new ArgsLoader
            {
                UseDashOptions = true,
                EnablePaseThrough = true,
            };
            Assert.AreEqual(expected, loader.Parse(new[] { "--foo", "bar", "baz", "--", "--foo", "baz"  }));
        }
    }
}
