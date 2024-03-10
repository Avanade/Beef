// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides <see cref="DirectoryInfo"/> count statistics.
    /// </summary>
    internal class DirectoryCountStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryCountStatistics"/> class.
        /// </summary>
        public DirectoryCountStatistics(DirectoryInfo directory, string[] exclude)
        {
            Directory = directory;
            if (directory.Name == "Generated")
                IsGenerated = true;

            Exclude = exclude ?? [];
        }

        /// <summary>
        /// Gets the <see cref="DirectoryInfo"/>.
        /// </summary>
        public DirectoryInfo Directory { get; }

        /// <summary>
        /// Gets the directory/path names to exclude.
        /// </summary>
        public string[] Exclude { get; private set; }

        /// <summary>
        /// Gets the file count.
        /// </summary>
        public int FileCount { get; private set; }

        /// <summary>
        /// Gets the total file count including children.
        /// </summary>
        public int TotalFileCount => FileCount + Children.Sum(x => x.TotalFileCount);

        /// <summary>
        /// Gets the generated file count.
        /// </summary>
        public int GeneratedFileCount { get; private set; }

        /// <summary>
        /// Gets the total generated file count including children.
        /// </summary>
        public int GeneratedTotalFileCount => GeneratedFileCount + Children.Sum(x => x.GeneratedTotalFileCount);

        /// <summary>
        /// Gets the line count;
        /// </summary>
        public int LineCount { get; private set; }

        /// <summary>
        /// Gets the total line count including children.
        /// </summary>
        public int TotalLineCount => LineCount + Children.Sum(x => x.TotalLineCount);

        /// <summary>
        /// Gets the generated line count.
        /// </summary>
        public int GeneratedLineCount { get; private set; }

        /// <summary>
        /// Gets the total line count including children.
        /// </summary>
        public int GeneratedTotalLineCount => GeneratedLineCount + Children.Sum(x => x.GeneratedTotalLineCount);

        /// <summary>
        /// Indicates whether the contents of the directory are generated.
        /// </summary>
        public bool IsGenerated { get; private set; }

        /// <summary>
        /// Gets the child <see cref="DirectoryCountStatistics"/> instances.
        /// </summary>
        public List<DirectoryCountStatistics> Children { get; } = [];

        /// <summary>
        /// Increments the file count.
        /// </summary>
        public void IncrementFileCount()
        {
            FileCount++;
            if (IsGenerated)
                GeneratedFileCount++;
        }

        /// <summary>
        /// Increments the line count.
        /// </summary>
        public void IncrementLineCount()
        {
            LineCount++;
            if (IsGenerated)
                GeneratedLineCount++;
        }

        /// <summary>
        /// Adds a child <see cref="DirectoryCountStatistics"/> instance.
        /// </summary>
        public DirectoryCountStatistics AddChildDirectory(DirectoryInfo di)
        {
            var dcs = new DirectoryCountStatistics(di, Exclude);
            if (IsGenerated)
                dcs.IsGenerated = true;

            Children.Add(dcs);
            return dcs;
        }

        /// <summary>
        /// Write the count statistics.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="columnLength">The maximum column length.</param>
        /// <param name="indent">The indent size to show hierarchy.</param>
        /// <param name="remove">The number of characters to remove from directory </param>
        public void Write(ILogger logger, int columnLength, int indent, int remove)
        {
            if (indent == 0)
            {
                var hdrAll = string.Format("{0, " + columnLength + "}", "All");
                var hdrGen = string.Format("{0, " + (columnLength + 5) + "}", "Generated");
                var hdrfiles = string.Format("{0, " + columnLength + "}", "Files");
                var hdrlines = string.Format("{0, " + columnLength + "}", "Lines");

                logger.LogInformation("{Content}", $"{hdrAll} | {hdrAll} | {hdrGen} | {hdrGen} | Path/");
                logger.LogInformation("{Content}", $"{hdrfiles} | {hdrlines} | {hdrfiles} Perc | {hdrlines} Perc | Directory");

                int maxLength = 0;
                DirectoryDepthAnalysis(this, 0, ref maxLength);
                logger.LogInformation("{Content}", new string('-', (columnLength * 4) + 22 + maxLength - remove));
            }

            var totfiles = string.Format("{0, " + columnLength + "}", TotalFileCount);
            var totlines = string.Format("{0, " + columnLength + "}", TotalLineCount);
            var totgenFiles = string.Format("{0, " + columnLength + "}", GeneratedTotalFileCount);
            var totgenFilesPerc = string.Format("{0, " + 3 + "}", GeneratedTotalFileCount == 0 ? 0 : Math.Round((double)GeneratedTotalFileCount / (double)TotalFileCount * 100.0, 0));
            var totgenLines = string.Format("{0, " + columnLength + "}", GeneratedTotalLineCount);
            var totgenLinesPerc = string.Format("{0, " + 3 + "}", GeneratedTotalLineCount == 0 ? 0 : Math.Round((double)GeneratedTotalLineCount / (double)TotalLineCount * 100.0, 0));

            logger.LogInformation("{Content}", $"{totfiles}   {totlines}   {totgenFiles} {totgenFilesPerc}%   {totgenLines} {totgenLinesPerc}%   {new string(' ', indent * 2)}{Directory.FullName[remove..]}");

            foreach (var dcs in Children)
            {
                if (dcs.TotalFileCount > 0)
                    dcs.Write(logger, columnLength, indent + 1, remove);
            }
        }

        /// <summary>
        /// Performs directory depth and length analysis.
        /// </summary>
        private static void DirectoryDepthAnalysis(DirectoryCountStatistics dcs, int depth, ref int maxLength)
        {
            maxLength = Math.Max(maxLength, dcs.Directory.FullName.Length + (depth * 2));

            foreach (var d in dcs.Children)
            {
                DirectoryDepthAnalysis(d, depth + 1, ref maxLength);
            }
        }

        /// <summary>
        /// Cleans (deletes) all <see cref="IsGenerated"/> directories.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/></param>
        public void Clean(ILogger logger)
        {
            // Where generated then delete.
            if (IsGenerated)
            {
                logger.LogWarning("  Deleted: {Directory} [{FileCount} files]", Directory.FullName, TotalFileCount);
                Directory.Delete(true);
                return;
            }

            // Where not generated then clean children.
            foreach (var dcs in Children)
            {
                dcs.Clean(logger);
            }
        }
    }
}
