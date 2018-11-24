using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineCalendarGenerator
{
    public class Options
    {
        [CommandLine.Option('o', "output", Required = true, HelpText = "出力するファイル名")]
        public string OutputFile { get; set; }

        [CommandLine.Option('y', "year", Required = true, HelpText = "年")]
        public int Year { get; set; }

        [CommandLine.Option('m', "month", Required = true, HelpText = "月")]
        public int Month { get; set; }

        [CommandLine.Option('f', "font", HelpText = "フォント", Default = "Myrica M")]
        public string Font { get; set; }

        [CommandLine.Option("vertical", HelpText = "縦出力", Default = false)]
        public bool Vertical { get; set; }

        [CommandLine.Option("interval", HelpText = "文字間隔", Default = 40)]
        public int Interval { get; set; }
    }
}
