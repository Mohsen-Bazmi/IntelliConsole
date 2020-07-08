using System;
using System.Linq;
using FsCheck;

namespace IntelliConsole.Tests
{
    static class GenerateNonNullOrEnterStrings
    {
        public static Arbitrary<string> Generate()
        => Arb.From(Arb.Default.Char().Filter(c => c != (char)13)
                       .Generator.ArrayOf().Select(x => new string(x)));

    }
    // static class GenerateAlphabeticalConsoleKeyInfo
    // {
    //     public static Arbitrary<(ConsoleKeyInfo,string)> Generate()
    //     => Arb.From(Arb.Default.Char().Filter(c => c != (char)13)
    //                    .Generator.ArrayOf().Select(x => (ConsoleKe new string(x)));

    // }
}