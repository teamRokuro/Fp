using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class Processor
    {
        #region External tool / library utilities

        /// <summary>
        /// Execute external program
        /// </summary>
        /// <param name="shellExecute">See <see cref="ProcessStartInfo.UseShellExecute"/></param>
        /// <param name="program">Program to run</param>
        /// <param name="args">Arguments</param>
        /// <returns>Exit code</returns>
        public int Execute(bool shellExecute, string program, string args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(program, args)
                {
                    RedirectStandardOutput = !shellExecute,
                    UseShellExecute = shellExecute,
                    RedirectStandardError = !shellExecute
                }
            };
            LogInfo($"Starting process {program} {args}");
            process.Start();
            if (!shellExecute)
            {
                LogInfo("Stdout>");
                string? line;
                while ((line = process.StandardOutput.ReadLine()) != null) LogInfo(line);

                LogInfo("Stderr>");
                while ((line = process.StandardError.ReadLine()) != null) LogInfo(line);
            }

            process.WaitForExit();
            return process.ExitCode;
        }

        #endregion
    }
}
