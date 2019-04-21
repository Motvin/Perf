using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using Perf;

namespace PerfRunner
{
	class Program
	{
		public struct OrderAndIndex
		{
			public int order;
			public int index;
			public int n;

			public OrderAndIndex(int order, int index, int n)
			{
				this.order = order;
				this.index = index;
				this.n = n;
			}
		}

		public class CompareOrderAndIndex : IComparer<OrderAndIndex>
		{
			public int Compare(OrderAndIndex x, OrderAndIndex y)
			{
				int res = x.order.CompareTo(y.order);
				//if (res == 0)
				//{
				//	res = x.index.CompareTo(y.index);
				//}
				return res;
			}
		}

		static void Main(string[] args)
		{
			try
			{
				DateTime startTime = DateTime.Now;

				// first arg is job name, 2nd is params
				string jobName = "";
				string paramsStr = "";
				if (args.Length > 0)
				{
					jobName = args[0].Trim();
					paramsStr = args[1].Trim();
				}
				else
				{
					//??? output error and exit
				}

				// 3rd arg is connection string override
				string dbConnStr;
				if (args.Length > 2)
				{
					dbConnStr = args[2].Trim();
				}
				else
				{
					dbConnStr = PerfRunner.Properties.Settings.Default.ConnStr.Trim();
				}

				JobWithBenchmarks j = PerfDb.ReadJobBenchmarkFromDb(dbConnStr, jobName, paramsStr);

				if (j.BenchmarkList != null && j.BenchmarkList.Count > 0)
				{
					int totalRunCount = 0;
					for (int i = 0; i < j.BenchmarkList.Count; i++)
					{
						int cnt = j.RunCount * j.BenchmarkList[i].CountN;
						totalRunCount += cnt;
					}

					OrderAndIndex[] runOrderArray = new OrderAndIndex[totalRunCount];

					Random rand = new Random(89);

					int n = 0;
					for (int i = 0; i < j.BenchmarkList.Count; i++)
					{
						int cnt = j.RunCount;
						int minN = j.BenchmarkList[i].MinN;
						int maxN = j.BenchmarkList[i].MaxN;
						int byN = j.BenchmarkList[i].ByN;

						for (int k = 0; k < cnt; k++)
						{
							for (int N = minN; N <= maxN; N += byN)
							{
								runOrderArray[n++] = new OrderAndIndex(rand.Next(int.MinValue, int.MaxValue), i, N);
							}
						}
					}
					Array.Sort(runOrderArray, new CompareOrderAndIndex());

					EnvironmentInfo environInfo = PerfUtil.GetEnvironmentInfo();
					int runID = PerfDb.InsertRun(dbConnStr, j.JobID, j.Params, j.JobName, j.JobDescription, "", environInfo);
					int everyTenth = runOrderArray.Length / 10;
					int nextTenth = everyTenth;
					int nextPct = 10;
					DateTime runStartTime = DateTime.Now;
					Console.WriteLine("Starting Job: " + jobName + ", with Params: " + paramsStr + ", at: " + runStartTime.ToString("h:mm.s tt"));
					for (int i = 0; i < runOrderArray.Length; i++)
					{
						BenchmarkMethod b = j.BenchmarkList[runOrderArray[i].index];
						PerfUtil.StartProgramWithArguments_DbNAndMaxN(b.ExecutableFileName, dbConnStr, runID, b.BenchmarkMethodID, runOrderArray[i].n, b.MaxN);
						if (i == nextTenth)
						{
							DateTime timeNow = DateTime.Now;
							TimeSpan ts = timeNow - runStartTime;
							Console.WriteLine("Percent Done: " + nextPct.ToString("N0") + "%, Total Minutes So Far = " + ts.TotalMinutes.ToString("N2"));
							nextTenth += everyTenth;
							nextPct += 10;
						}
					}
					DateTime timeNowDone = DateTime.Now;
					TimeSpan tsDone = timeNowDone - runStartTime;
					Console.WriteLine("Finished: Total Minutes = " + tsDone.TotalMinutes.ToString("N2"));
				}
			}
			catch (Exception ex)
			{
				PerfUtil.OutputError(ex);
				Console.ReadKey();
			}
		}
	}
}
