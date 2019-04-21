using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;

namespace Perf
{
	//public class PerfGroup
	//{
	//	public class PerfGroup(int iterationCount, )
	//	{
	//	}
	//}

	//  public class Timings
	//  {
	//private long[] timingsArray;
	//private int timingsIdx;

	//[MethodImpl(MethodImplOptions.AggressiveInlining)] //??? not sure this should be used
	//public Timings(int timingsCount)
	//{
	//	timingsArray = new long[timingsCount];
	//}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//public void Start(Stopwatch sw)
	//{
	//	sw.Restart();
	//}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//public void Stop(Stopwatch sw, bool getElapsedTime = true)
	//{
	//	sw.Stop();
	//	if (getElapsedTime)
	//	{
	//		timingsArray[timingsIdx++] = sw.ElapsedTicks;
	//	}
	//}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//public void StopAndRestart(Stopwatch sw, bool getElapsedTime = true)
	//{
	//	if (getElapsedTime)
	//	{
	//		sw.Stop();
	//		timingsArray[timingsIdx++] = sw.ElapsedTicks;
	//	}
	//	sw.Restart();
	//}

	//public void Clear()
	//{
	//	//???
	//}
	//  }
	public class BenchmarkSummaries
	{
		List<List<NSummary>> summariesList = new List<List<NSummary>>();
		List<string> benchmarkNamesList = new List<string>();

		public void AddNSummaryList(List<NSummary> lst, string benchmarkName)
		{
			summariesList.Add(lst);
			benchmarkNamesList.Add(benchmarkName);
		}

		public void OutputSummariesToFile(string fileName, string baseLineBenchmarkName = null)
		{
			List<string> lines = new List<string>();

			bool outputRatioCols = (baseLineBenchmarkName != null);

			const char sep = '\t';

			// write header line
			StringBuilder sb = new StringBuilder("N", 256);

			for (int i = 0; i < summariesList.Count; i++)
			{
				string benchmarkName = benchmarkNamesList[i];
				sb.Append(sep);
				sb.Append(benchmarkName);
				sb.Append(" Mean"); //??? need units other than nanoseconds
			}

			if (outputRatioCols)
			{
				for (int i = 0; i < summariesList.Count; i++)
				{
					string benchmarkName = benchmarkNamesList[i];
					sb.Append(sep);
					sb.Append(benchmarkName);
					sb.Append(" Ratio");
				}
			}
			lines.Add(sb.ToString());
			sb.Clear();

			// for now each summary list must have the same n values
			//int[] idxForSummary = new int[summariesList.Count];
			//while (true)
			//{

			//}

			int maxIdx = summariesList[0].Count;
			for (int i = 0; i < maxIdx; i++)
			{
				double baseLineMean = 0;
				for (int j = 0; j < summariesList.Count; j++)
				{
					List<NSummary> lst = summariesList[j];

					if (j == 0)
					{
						sb.Append(lst[i].n.ToString("N0"));
					}
					sb.Append(sep);
					sb.Append(lst[i].meanNanoSeconds.ToString("N0")); // ??? might want to show some decimal places if this is really small

					if (outputRatioCols && baseLineMean == 0 && string.Equals(benchmarkNamesList[j], baseLineBenchmarkName, StringComparison.Ordinal))
					{
						baseLineMean = lst[i].meanNanoSeconds;
					}
				}

				if (outputRatioCols)
				{
					for (int j = 0; j < summariesList.Count; j++)
					{
						List<NSummary> lst = summariesList[j];
						sb.Append(sep);
						sb.Append((lst[i].meanNanoSeconds / baseLineMean).ToString("N2"));
					}
				}

				lines.Add(sb.ToString());
				sb.Clear();
			}

			File.WriteAllLines(fileName, lines.ToArray());
		}
	}

	public class NSummary
	{
		public int n; //??? maybe this should be a long
		public int tickSamplesPerN; // this is loop unroll count if any * iterationCount
		public long totalNanoSeconds;
		public double meanNanoSeconds;

		public NSummary(double overheadTicks, int[] nArray, int nIndex, int tickSamplesPerN, long[] ticksArray)
		{
			n = nArray[nIndex];
			this.tickSamplesPerN = tickSamplesPerN;

			long ticksPerSec = Stopwatch.Frequency;
			long nanoSecPerTick = 1_000_000_000 / ticksPerSec;
			long overheadNanoSeconds = (long)Math.Round((double)nanoSecPerTick * overheadTicks);

			int tickStartIdx = nIndex * tickSamplesPerN;
			int tickEndIdx = tickStartIdx + tickSamplesPerN - 1;
			for (int i = tickStartIdx; i < tickEndIdx; i++)
			{
				totalNanoSeconds += (nanoSecPerTick * ticksArray[i]) - overheadNanoSeconds;
			}

			//??? indicate any negative ticks as an error somehow - probably in some other code that checks all ticks and the overheadTicks

			meanNanoSeconds = (double)totalNanoSeconds / (double)tickSamplesPerN;
		}

		public static List<NSummary> CreateNSummaryListForBenchmark(double overheadTicks, int[] nArray, int tickSamplesPerN, long[] ticksArray)
		{
			List<NSummary> lst = new List<NSummary>();
			for (int i = 0; i < nArray.Length; i++)
			{
				lst.Add(new NSummary(overheadTicks, nArray, i, tickSamplesPerN, ticksArray));
				// ??? try to determine in this loop what units these should be in - might not want mean in nanoSeconds
			}
			return lst;
		}
	}

	public class EnvironmentInfo
	{
		public string MachineName { get; set; }
		public string CPUDescription { get; set; }
		public string OSVersion { get; set; }
		public bool IsOS64Bit { get; set; }

		public string BenchmarkingProgram { get; set; }
		public string BenchmarkingProgramVersion { get; set; }
		public string ClrType { get; set; }
		public string ClrVersion { get; set; }

		public bool ArePrograms64Bit { get; set; }
	}

	public static class PerfUtil
    {
		//private long[] timingsArray;
		//private Stopwatch sw;
		//private int timingsIdx;

		//[MethodImpl(MethodImplOptions.AggressiveInlining)] //??? not sure this should be used
		//public Perf(int timingsCount)
		//{
		//	timingsArray = new long[timingsCount];
		//	sw = new Stopwatch();
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Start()
		//{
		//	sw.Restart();
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void Stop(bool getElapsedTime = true)
		//{
		//	sw.Stop();
		//	if (getElapsedTime)
		//	{
		//		timingsArray[timingsIdx++] = sw.ElapsedTicks;
		//	}
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void StopAndRestart(bool getElapsedTime = true)
		//{
		//	if (getElapsedTime)
		//	{
		//		sw.Stop();
		//		timingsArray[timingsIdx++] = sw.ElapsedTicks;
		//	}
		//	sw.Restart();
		//}


		public static double GetNanoSecondsFromTicks(double ticks, long ticksPerSec)
		{
			double nanoSecPerTick = (1_000_000_000.0) / ticksPerSec;
			return nanoSecPerTick * ticks;
		}

		public static void OutputError(Exception ex)
		{
			Console.Write(ex.ToString());
		}

		public static void StartProgramWithArguments_DbNAndMaxN(string exeFileName, string dbConnStr, int runID, int benchmarkMethodID, int n, int maxN)
		{
			using (Process proc = new Process())
			{
				proc.StartInfo.UseShellExecute = false;
				//proc.StartInfo.CreateNoWindow = true;
				//proc.ProcessorAffinity;
				//proc.PriorityClass = ProcessPriorityClass.High; // or realtime or normal
				proc.StartInfo.FileName = exeFileName;
				proc.StartInfo.Arguments = '"' + dbConnStr + '"' + ' ' + runID.ToString("F0") + ' ' + benchmarkMethodID.ToString("F0") + ' ' + n.ToString("F0") + ' ' + maxN.ToString("F0");
				//proc.StartInfo.ErrorDialog = true;
				//proc.StartInfo.CreateNoWindow = true;
				//proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
				proc.Start();
				proc.WaitForExit();
				TimeSpan ts = proc.TotalProcessorTime;
			}
		}

		//??? maybe return a list of error messages instead of just one
		public static string GetCmdLineParams_DbNAndMaxN(string[] args, out string dbConnStr, out int runID, out int benchmarkMethodID, out int n, out int maxN)
		{
			string errMsg = null;
			dbConnStr = "";
			runID = 0;
			benchmarkMethodID = 0;
			n = 0;
			maxN = 0;

			if (args.Length < 5)
			{
				errMsg = "Too few cmd line params.  There are " + args.Length.ToString("N0") + " params and there needs to be at least 5.  These should be DbConnStr, RunID, BenchmarkMethodID, N, MaxN";
			}
			else
			{
				dbConnStr = args[0].Trim();

				if (!int.TryParse(args[1].Trim(), out runID))
				{
					errMsg = "2nd cmd line param (RunID) needs to be an int.";
				}

				if (!int.TryParse(args[2].Trim(), out benchmarkMethodID))
				{
					errMsg = "3rd cmd line param (BenchmarkMethodID) needs to be an int.";
				}

				if (!int.TryParse(args[3].Trim(), out n))
				{
					errMsg = "4th cmd line param (N) needs to be an int, it is:" + args[3].Trim();
				}

				if (!int.TryParse(args[4].Trim(), out maxN))
				{
					errMsg = "5th cmd line param (MaxN) needs to be an int, it is:" + args[4].Trim();
				}

				if (errMsg == null)
				{
					if (n == 0)
					{
						errMsg = "4th cmd line param (N) cannot be 0.";
					}

					if (maxN == 0)
					{
						errMsg = "5th cmd line param (MaxN) cannot be 0.";
					}
				}
			}

			return errMsg;
		}

		public static string GetCmdLineParams_OutputFileAndMinMaxIncN(string[] args, out int minN, out int maxN, out int incrementNBy, out string outputFileName)
		{
			string errMsg = null;
			outputFileName = "";
			minN = 0;
			maxN = 0;
			incrementNBy = 0;

			if (args.Length < 4)
			{
				errMsg = "Too few cmd line params.  There are " + args.Length.ToString("N0") + " params and there needs to be 4.  These should be minN, maxN, incrementNBy, outputFileName";
			}
			else
			{
				if (!int.TryParse(args[0], out minN))
				{
					errMsg = "1st cmd line param (minN) needs to be an int.";
				}

				if (!int.TryParse(args[1], out maxN))
				{
					errMsg = "2nd cmd line param (maxN) needs to be an int.";
				}

				if (!int.TryParse(args[2], out incrementNBy))
				{
					errMsg = "3rd cmd line param (incrementNBy) needs to be an int.";
				}

				outputFileName = args[3];

				if (errMsg != null)
				{
					if (incrementNBy == 0)
					{
						errMsg = "3rd cmd line param (incrementNBy) cannot be 0.";
					}
					else if ((maxN > minN) != (incrementNBy > 0))
					{
						errMsg = "minN cmd line param must be less than maxN, unless incrementNBy is negative.";
					}
				}
			}

			return errMsg;
		}

		public static string GetCmdLineParams_OutputFileAndMinMaxIncN(string[] args, out int minN, out int maxN, out int incrementNBy)
		{
			string errMsg = null;
			minN = 0;
			maxN = 0;
			incrementNBy = 0;

			if (args.Length < 3)
			{
				errMsg = "Too few cmd line params.  There are " + args.Length.ToString("N0") + " params and there needs to be 3.  These should be minN, maxN, incrementNBy";
			}
			else
			{
				if (!int.TryParse(args[0], out minN))
				{
					errMsg = "1st cmd line param (minN) needs to be an int.";
				}

				if (!int.TryParse(args[1], out maxN))
				{
					errMsg = "2nd cmd line param (maxN) needs to be an int.";
				}

				if (!int.TryParse(args[2], out incrementNBy))
				{
					errMsg = "3rd cmd line param (incrementNBy) needs to be an int.";
				}

				if (errMsg != null)
				{
					if (incrementNBy == 0)
					{
						errMsg = "3rd cmd line param (incrementNBy) cannot be 0.";
					}
					else if ((maxN > minN) != (incrementNBy > 0))
					{
						errMsg = "minN cmd line param must be less than maxN, unless incrementNBy is negative.";
					}
				}
			}

			return errMsg;
		}

		public static void DoGCCollect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		public static double GetTimestampOverheadInNanoSeconds()
		{
			// there is some overhead that should be removed - it is returning from GetTimestamp and setting startTicks and afterwards calling GetTimestamp until the point where the return value is obtained
			// we should determine this overhead by calling it 16 times in a row

			Stopwatch.GetTimestamp(); // make sure this is jitted - not sure this even gets jitted???

			long startTicks = Stopwatch.GetTimestamp();

			Stopwatch.GetTimestamp(); // 1
			Stopwatch.GetTimestamp(); // 2
			Stopwatch.GetTimestamp(); // 3
			Stopwatch.GetTimestamp(); // 4
			Stopwatch.GetTimestamp(); // 5
			Stopwatch.GetTimestamp(); // 6
			Stopwatch.GetTimestamp(); // 7
			Stopwatch.GetTimestamp(); // 8
			Stopwatch.GetTimestamp(); // 9
			Stopwatch.GetTimestamp(); // 10
			Stopwatch.GetTimestamp(); // 11
			Stopwatch.GetTimestamp(); // 12
			Stopwatch.GetTimestamp(); // 13
			Stopwatch.GetTimestamp(); // 14
			Stopwatch.GetTimestamp(); // 15
			Stopwatch.GetTimestamp(); // 16

			Stopwatch.GetTimestamp(); // 1
			Stopwatch.GetTimestamp(); // 2
			Stopwatch.GetTimestamp(); // 3
			Stopwatch.GetTimestamp(); // 4
			Stopwatch.GetTimestamp(); // 5
			Stopwatch.GetTimestamp(); // 6
			Stopwatch.GetTimestamp(); // 7
			Stopwatch.GetTimestamp(); // 8
			Stopwatch.GetTimestamp(); // 9
			Stopwatch.GetTimestamp(); // 10
			Stopwatch.GetTimestamp(); // 11
			Stopwatch.GetTimestamp(); // 12
			Stopwatch.GetTimestamp(); // 13
			Stopwatch.GetTimestamp(); // 14
			Stopwatch.GetTimestamp(); // 15
			Stopwatch.GetTimestamp(); // 16

			Stopwatch.GetTimestamp(); // 1
			Stopwatch.GetTimestamp(); // 2
			Stopwatch.GetTimestamp(); // 3
			Stopwatch.GetTimestamp(); // 4
			Stopwatch.GetTimestamp(); // 5
			Stopwatch.GetTimestamp(); // 6
			Stopwatch.GetTimestamp(); // 7
			Stopwatch.GetTimestamp(); // 8
			Stopwatch.GetTimestamp(); // 9
			Stopwatch.GetTimestamp(); // 10
			Stopwatch.GetTimestamp(); // 11
			Stopwatch.GetTimestamp(); // 12
			Stopwatch.GetTimestamp(); // 13
			Stopwatch.GetTimestamp(); // 14
			Stopwatch.GetTimestamp(); // 15
			Stopwatch.GetTimestamp(); // 16

			Stopwatch.GetTimestamp(); // 1
			Stopwatch.GetTimestamp(); // 2
			Stopwatch.GetTimestamp(); // 3
			Stopwatch.GetTimestamp(); // 4
			Stopwatch.GetTimestamp(); // 5
			Stopwatch.GetTimestamp(); // 6
			Stopwatch.GetTimestamp(); // 7
			Stopwatch.GetTimestamp(); // 8
			Stopwatch.GetTimestamp(); // 9
			Stopwatch.GetTimestamp(); // 10
			Stopwatch.GetTimestamp(); // 11
			Stopwatch.GetTimestamp(); // 12
			Stopwatch.GetTimestamp(); // 13
			Stopwatch.GetTimestamp(); // 14
			Stopwatch.GetTimestamp(); // 15

			long endTicks = Stopwatch.GetTimestamp(); // 16
			long overHeadTicks = endTicks - startTicks;

			// ??? indicate negative is an error somehow
			//if (overHeadTicks < 0)
			//{

			//}

			double overHeadTicksDbl = (double)overHeadTicks / 64.0;
			double nanoSecPerTick = 1_000_000_000.0 / (double)Stopwatch.Frequency;
			return nanoSecPerTick * overHeadTicksDbl;
		}

		public static EnvironmentInfo GetEnvironmentInfo()
		{
			EnvironmentInfo n = new EnvironmentInfo();

			n.MachineName = Environment.MachineName;

			int procCnt = Environment.ProcessorCount;
			n.CPUDescription = procCnt.ToString("N0");

			n.OSVersion = Environment.OSVersion.ToString();
			n.IsOS64Bit = Environment.Is64BitOperatingSystem;

			n.BenchmarkingProgram = System.AppDomain.CurrentDomain.FriendlyName;

			//Assembly assembly = Assembly.GetExecutingAssembly();
			//FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			//n.BenchmarkingProgramVersion = fileVersionInfo.ProductVersion;

			n.BenchmarkingProgramVersion = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion).ToString();

			n.ClrType = "";
			n.ClrVersion = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName; // doesn't work for .net core???
																						 //n.ClrVersion = Environment.Version.ToString(); // doesn't work after .net 4???

			return n;
		}
	}

	public class Job
	{
		public int JobID { get; set; }
		public string JobName { get; set; }
		public string JobDescription { get; set; }
	}

	public class BenchmarkMethod
	{
		public int BenchmarkMethodID { get; set; }
		public string MethodName { get; set; }
		public string MethodDescription { get; set; }
		public string ParamsType { get; set; }
		public string ExecutableFileName { get; set; }

		// if ParamsType == MinMaxByN, then set the variables below
		public int CountN { get; set; }
		public int MinN { get; set; }
		public int MaxN { get; set; }
		public int ByN { get; set; }

		//??? add other prop if needed

		public const string ParamsType_MinMaxByN = "MinMaxByN";
	}

	public class JobWithBenchmarks
	{
		public int JobID { get; set; }
		public string JobName { get; set; }
		public string JobDescription { get; set; }

		public string Params { get; set; }
		public int RunCount { get; set; }

		public List<BenchmarkMethod> BenchmarkList { get; set; } = new List<BenchmarkMethod>();
	}

	public static class PerfDb
	{
		public static DbConnection GetOpenConnection(string dbConnStr)
		{
			DbConnection conn = new SqlConnection(dbConnStr);
			conn.Open();

			return conn;
		}

		public static void CloseDbObjects(DbDataReader rdr, DbCommand cmd, DbConnection conn = null)
		{
			if (rdr != null)
			{
				rdr.Close();
			}

			if (cmd != null)
			{
				cmd.Dispose();
			}

			if (conn != null)
			{
				conn.Close();
			}
		}

		public static JobWithBenchmarks ReadJobBenchmarkFromDb(string dbConnStr, string jobName, string paramsStr)
		{
			DbConnection conn = null;
			DbCommand cmd = null;
			DbDataReader rdr = null;

			JobWithBenchmarks j = new JobWithBenchmarks();

			try
			{
				conn = GetOpenConnection(dbConnStr);

				string sql =
				$@"
select
j.JobID
, j.JobName
, j.JobDescription
, jb.RunCount
, b.BenchmarkMethodID
, b.MethodName
, b.MethodDescription
, b.ExecutableFileName
, b.ParamsType
from job j
inner join JobBenchmark jb on jb.JobID = j.JobID
inner join BenchmarkMethod b on b.BenchmarkMethodID = jb.BenchmarkMethodID
where j.JobName = '{jobName}' and jb.Params = '{paramsStr}' and jb.Active = 1
";

				cmd = conn.CreateCommand();
				cmd.CommandText = sql;

				rdr = cmd.ExecuteReader();

				while (rdr.Read())
				{
					int i = 0;
					j.JobID = rdr.GetInt32(i++);
					j.JobName = rdr.GetString(i++);
					j.JobDescription = rdr.GetString(i++);
					j.Params = paramsStr;
					j.RunCount = rdr.GetInt32(i++);

					BenchmarkMethod b = new BenchmarkMethod();

					b.BenchmarkMethodID = rdr.GetInt32(i++);
					b.MethodName = rdr.GetString(i++);
					b.MethodDescription = rdr.GetString(i++);
					b.ExecutableFileName = rdr.GetString(i++);
					b.ParamsType = rdr.GetString(i++);

					if (string.Equals(b.ParamsType, BenchmarkMethod.ParamsType_MinMaxByN, StringComparison.OrdinalIgnoreCase))
					{
						// parse the first 3 arguments in Params
						string[] args = j.Params.Split('|');

						if (args.Length >= 3)
						{
							if (int.TryParse(args[0].Trim(), out int minN))
							{
								b.MinN = minN;
							}
							else
							{
								//??? error
							}

							if (int.TryParse(args[1].Trim(), out int maxN))
							{
								b.MaxN = maxN;
							}
							else
							{
								//??? error
							}

							if (int.TryParse(args[2].Trim(), out int byN))
							{
								b.ByN = byN;
							}
							else
							{
								//??? error
							}

							if (byN > 0)
							{
								b.CountN = ((maxN - minN) / byN) + 1;
							}
						}
						else
						{
							//??? error
						}
					}

					j.BenchmarkList.Add(b);
				}
			}
			finally
			{
				CloseDbObjects(rdr, cmd, conn);
			}

			return j;
		}

		public static void InsertRunError(string dbConnStr, int runID, int benchmarkMethodID, Exception ex)
		{
			InsertRunError(dbConnStr, runID, benchmarkMethodID, ex.ToString());
		}

		public static void InsertRunError(string dbConnStr, int runID, int benchmarkMethodID, string errStr)
		{
			DbConnection conn = null;
			DbCommand cmd = null;

			try
			{
				conn = GetOpenConnection(dbConnStr);

				string sql = $@"
insert Error
(RunID, BenchmarkMethodID, ErrorString) values
(@RunID, @BenchmarkMethodID, @ErrorString) 
";

				cmd = conn.CreateCommand();
				cmd.Parameters.Add(new SqlParameter("@RunID", runID));
				cmd.Parameters.Add(new SqlParameter("@BenchmarkMethodID", benchmarkMethodID));
				cmd.Parameters.Add(new SqlParameter("@ErrorString", errStr));

				cmd.CommandText = sql;

				int cnt = cmd.ExecuteNonQuery();
			}
			finally
			{
				CloseDbObjects(null, cmd, conn);
			}
		}

		public static void InsertMeasurement(string dbConnStr, int runID, int benchmarkMethodID, int n, int iterations, double nanoSecs, DateTime startTime, DateTime endTime)
		{
			DbConnection conn = null;
			DbCommand cmd = null;

			try
			{
				conn = GetOpenConnection(dbConnStr);

				string sql = $@"
insert Measurement
(RunID, BenchmarkMethodID, N, Iterations, NanoSeconds, StartTime, EndTime) values
(@RunID, @BenchmarkMethodID, @N, @Iterations, @NanoSeconds, @StartTime, @EndTime) 
";

				cmd = conn.CreateCommand();
				cmd.Parameters.Add(new SqlParameter("@RunID", runID));
				cmd.Parameters.Add(new SqlParameter("@BenchmarkMethodID", benchmarkMethodID));
				cmd.Parameters.Add(new SqlParameter("@N", n));
				cmd.Parameters.Add(new SqlParameter("@Iterations", iterations));
				cmd.Parameters.Add(new SqlParameter("@NanoSeconds", nanoSecs));
				cmd.Parameters.Add(new SqlParameter("@StartTime", startTime));
				cmd.Parameters.Add(new SqlParameter("@EndTime", endTime));

				cmd.CommandText = sql;

				int cnt = cmd.ExecuteNonQuery();
			}
			finally
			{
				CloseDbObjects(null, cmd, conn);
			}
		}

		public static int InsertRun(string dbConnStr, int jobID, string paramsStr, string runName, string runDescription, string runDescriptionExtra, EnvironmentInfo n)
		{
			DbConnection conn = null;
			DbCommand cmd = null;
			int runID = 0;

			try
			{
				conn = GetOpenConnection(dbConnStr);

				string sql = $@"
insert Run
(JobID, Params, RunName, RunDescription, RunDescriptionExtra, MachineName, CPUDescription, OSVersion, IsOS64Bit, BenchmarkingProgram, BenchmarkingProgramVersion, ClrType, ClrVersion, ArePrograms64Bit, StartTime) values
(@JobID, @Params, @RunName, @RunDescription, @RunDescriptionExtra, @MachineName, @CPUDescription, @OSVersion, @IsOS64Bit, @BenchmarkingProgram, @BenchmarkingProgramVersion, @ClrType, @ClrVersion, @ArePrograms64Bit, @StartTime);
SELECT CAST(SCOPE_IDENTITY() AS INT) AS IntIdentity
";

				cmd = conn.CreateCommand();
				cmd.Parameters.Add(new SqlParameter("@JobID", jobID));
				cmd.Parameters.Add(new SqlParameter("@Params", paramsStr));
				cmd.Parameters.Add(new SqlParameter("@RunName", runName));
				cmd.Parameters.Add(new SqlParameter("@RunDescription", runDescription));
				cmd.Parameters.Add(new SqlParameter("@RunDescriptionExtra", runDescriptionExtra));
				cmd.Parameters.Add(new SqlParameter("@MachineName", n.MachineName));
				cmd.Parameters.Add(new SqlParameter("@CPUDescription", n.CPUDescription));
				cmd.Parameters.Add(new SqlParameter("@OSVersion", n.OSVersion));
				cmd.Parameters.Add(new SqlParameter("@IsOS64Bit", n.IsOS64Bit));
				cmd.Parameters.Add(new SqlParameter("@BenchmarkingProgram", n.BenchmarkingProgram));
				cmd.Parameters.Add(new SqlParameter("@BenchmarkingProgramVersion", n.BenchmarkingProgramVersion));
				cmd.Parameters.Add(new SqlParameter("@ClrType", n.ClrType));
				cmd.Parameters.Add(new SqlParameter("@ClrVersion", n.ClrVersion));
				cmd.Parameters.Add(new SqlParameter("@ArePrograms64Bit", n.ArePrograms64Bit));
				cmd.Parameters.Add(new SqlParameter("@StartTime", DateTime.Now));

				cmd.CommandText = sql;

				runID = (int)cmd.ExecuteScalar();
			}
			finally
			{
				CloseDbObjects(null, cmd, conn);
			}

			return runID;
		}
	}
}
