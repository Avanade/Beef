// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents the statistics from a <see cref="CodeGenerator"/>.
    /// </summary>
    public class CodeGenStats
    {
        /// <summary>
        /// Gets the overall created (new) artefact count.
        /// </summary>
        public int CreatedCount { get; set; }

        /// <summary>
        /// Gets the overall updated (changed) artefact count.
        /// </summary>
        public int UpdatedCount { get; set; }

        /// <summary>
        /// Gets the overall not changed count (neither created or updated).
        /// </summary>
        public int NotChangedCount { get; set; }

        /// <summary>
        /// Gets the overall lines of code count.
        /// </summary>
        public int LinesOfCodeCount { get; set; }

        /// <summary>
        /// Gets or sets the elapsed milliseconds.
        /// </summary>
        /// <summary>A <c>null</c> indicates that the value has not been set.</summary>
        public long? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Adds other <paramref name="stats"/> to this instance.
        /// </summary>
        /// <param name="stats">The other <see cref="CodeGenStats"/>.</param>
        public void Add(CodeGenStats stats)
        {
            CreatedCount += stats.CreatedCount;
            UpdatedCount += stats.UpdatedCount;
            NotChangedCount += stats.NotChangedCount;
            LinesOfCodeCount += stats.LinesOfCodeCount;
        }

        /// <summary>
        /// Provides a formatted <see cref="string"/> representation.
        /// </summary>
        /// <returns>A formatted <see cref="string"/> representation.</returns>
        public override string ToString() => $"[Files: Unchanged = {NotChangedCount}, Updated = {UpdatedCount}, Created = {CreatedCount}]";

        /// <summary>
        /// Provides a summary formatted <see cref="string"/> representation.
        /// </summary>
        /// <returns>A summary formatted <see cref="string"/> representation.</returns>
        public string ToSummaryString() => $"[{ElapsedMilliseconds.ToString() ?? "-"}ms, Files: Unchanged = {NotChangedCount}, Updated = {UpdatedCount}, Created = {CreatedCount}, TotalLines = {LinesOfCodeCount}]";
    }
}