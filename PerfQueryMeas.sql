use perf

--select
--b.MethodName, m.N, m.NanoSeconds, m.Iterations, m.StartTime
--from Measurement m
--inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
--where m.RunID = 1 and MethodName = 'Contains_SCG' -- Contains_Fast Contains_C5
--and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.03)
--order by b.MethodName, m.N, m.NanoSeconds


select
m.N, b.MethodName, Avg(m.NanoSeconds), Count(*)
from Measurement m
inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
where m.RunID = 1 and MethodName = 'Contains_C5' -- Contains_SCG Contains_Fast Contains_C5
and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.03)
group by b.MethodName, m.N
order by b.MethodName, m.N--, m.NanoSeconds

select
m.N, b.MethodName, Avg(m.NanoSeconds / iterations), Count(*), min(Iterations)
from Measurement m
inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
where m.RunID = 1 and MethodName = 'Contains_SCG' -- Contains_SCG Contains_Fast Contains_C5
--and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.08)
group by b.MethodName, m.N
order by b.MethodName, m.N--, m.NanoSeconds

use perf

select
m.N, b.MethodName, Avg(m.NanoSeconds), Count(*)
from Measurement m
inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
where m.RunID = 1011 and MethodName = 'ClassVStructAddClassFast' -- Contains_SCG Contains_Fast Contains_C5
and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.03)
group by b.MethodName, m.N
order by b.MethodName, m.N--, m.NanoSeconds

select
m.N, b.MethodName, Avg(m.NanoSeconds / iterations), Count(*), min(Iterations)
from Measurement m
inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
where m.RunID = 1011 and MethodName = 'ClassVStructAddClassFast' -- Contains_SCG Contains_Fast Contains_C5
and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.03)
group by b.MethodName, m.N
order by b.MethodName, m.N--, m.NanoSeconds


select
m.N, b.MethodName, Avg(m.NanoSeconds)
from Measurement m
inner join BenchmarkMethod b on b.BenchmarkMethodID = m.BenchmarkMethodID
where m.RunID = 1011 and MethodName = 'ClassVStructAddClassFast' -- Contains_SCG Contains_Fast Contains_C5
and NanoSeconds <= ((select min(NanoSeconds) from Measurement m2 where m2.BenchmarkMethodID = m.BenchmarkMethodID and m2.N = m.N) * 1.03)
group by b.MethodName, m.N
order by b.MethodName, m.N--, m.NanoSeconds