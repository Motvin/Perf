use perf

insert into job (JobName, JobDescription) values ('HashSet Contains Integers', 'Pre-populate HashSets with NMax integers and then call Contains in a loop N times in increments of NBy.  The HashSets are populated with the same unique random integers from int.MinValue to int.MaxValue.  The Contains is done in a loop using an array of integers that are 50% unique values contained in the HashSet and then the remaining are 25% duplicates of the 50% unique and also 25% values that aren not contained in the HashSet.')
go

-- insert into job (JobName, JobDescription) values ('HashSet Contains Integers 1,000-10,000', 'Pre-populate HashSets with 10,000 integers and then call Contains in a loop 1,000 to 10,000 times in increments of 100.  The HashSets are populated with the same unique random integers from int.MinValue to int.MaxValue.  The Contains is done in a loop using an array of integers that are 50% unique values contained in the HashSet and then the remaining are 25% duplicates of the 50% unique and also 25% values that aren not contained in the HashSet.')
-- go

-- insert into job (JobName, JobDescription) values ('HashSet Contains Integers 10,000-100,000', 'Pre-populate HashSets with 100,000 integers and then call Contains in a loop 10,000 to 100,000 times in increments of 1,000.  The HashSets are populated with the same unique random integers from int.MinValue to int.MaxValue.  The Contains is done in a loop using an array of integers that are 50% unique values contained in the HashSet and then the remaining are 25% duplicates of the 50% unique and also 25% values that aren not contained in the HashSet.')
-- go

-- insert into job (JobName, JobDescription) values ('HashSet Contains Integers 100,000-1,000,000', 'Pre-populate HashSets with 100,000 integers and then call Contains in a loop 100,000 to 1,000,000 times in increments of 10,000.  The HashSets are populated with the same unique random integers from int.MinValue to int.MaxValue.  The Contains is done in a loop using an array of integers that are 50% unique values contained in the HashSet and then the remaining are 25% duplicates of the 50% unique and also 25% values that aren not contained in the HashSet.')
-- go

-- insert into job (JobName, JobDescription) values ('HashSet Contains Integers 1,000,000-10,000,000', 'Pre-populate HashSets with 1,000,000 integers and then call Contains in a loop 1,000,000 to 10,000,000 times in increments of 100,000.  The HashSets are populated with the same unique random integers from int.MinValue to int.MaxValue.  The Contains is done in a loop using an array of integers that are 50% unique values contained in the HashSet and then the remaining are 25% duplicates of the 50% unique and also 25% values that aren not contained in the HashSet.')
-- go

-- add type (min/max int) and mix of unique/non-unique and does contain to params???



insert into benchmarkmethod (MethodName, MethodDescription, ParamsType, ExecutableFileName)
 values ('Contains_SCG', 'Run Contains using the System.Collections.Generic HashSet', 'MinMaxByN', 'E:\Proj\FastHashSet\HashSetPerf\HashSetContains\bin\Release\HashSetContains.exe')

insert into benchmarkmethod (MethodName, MethodDescription, ParamsType, ExecutableFileName)
 values ('Contains_Fast', 'Run Contains using FastHashSet', 'MinMaxByN', 'E:\Proj\FastHashSet\HashSetPerf\HashSetContainsFast\bin\Release\HashSetContainsFast.exe')

insert into benchmarkmethod (MethodName, MethodDescription, ParamsType, ExecutableFileName)
 values ('Contains_C5', 'Run Contains using the C5 HashSet', 'MinMaxByN', 'E:\Proj\FastHashSet\HashSetPerf\HashSetContainsC5\bin\Release\HashSetContainsC5.exe')

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1|100|1', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '100|1000|10', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1000|10000|100', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '10000|100000|1000', 60
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '100000|1000000|10000', 30
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1000000|10000000|100000', 10
from Job j
join BenchmarkMethod b on b.MethodName in ('Contains_SCG', 'Contains_Fast', 'Contains_C5')
where j.JobName = 'HashSet Contains Integers'

------------------

insert into job (JobName, JobDescription) values ('HashSetClassVStructAdd', 'desc.')
go

insert into benchmarkmethod (MethodName, MethodDescription, ParamsType, ExecutableFileName)
 values ('ClassVStructAddStructFast', 'Run ', 'MinMaxByN', 'E:\Proj\FastHashSet\HashSetPerf\HashSetStructFast\bin\Release\HashSetStructFast.exe')

insert into benchmarkmethod (MethodName, MethodDescription, ParamsType, ExecutableFileName)
 values ('ClassVStructAddClassFast', 'Run ', 'MinMaxByN', 'E:\Proj\FastHashSet\HashSetPerf\HashSetClassFast\bin\Release\HashSetClassFast.exe')

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1|100|1', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '100|1000|10', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1000|10000|100', 100
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '10000|100000|1000', 60
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '100000|1000000|10000', 30
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

insert into JobBenchmark (JobID, BenchmarkMethodID, Params, RunCount)
select j.JobID, b.BenchmarkMethodID, '1000000|10000000|100000', 10
from Job j
join BenchmarkMethod b on b.MethodName in ('ClassVStructAddStructFast', 'ClassVStructAddClassFast')
where j.JobName = 'HashSetClassVStructAdd'

