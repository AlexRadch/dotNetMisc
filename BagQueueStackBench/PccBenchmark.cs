using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

namespace BagQueueStackBench;

public enum PccCollectionType
{
    Bag,
    Queue,
    Stack
}

public enum PccBenchmarkType
{
    AddItems,
    TakeItems,
    AddWaitTakeItems,
    AddAndTakeItems,
}

public enum PccThreading
{
    Thread,
    ThreadPool,
    Task,
    ParallelInvoke,
    ParallelLoop
}

public enum PccBenchmarkParamOrder
{
    ItemsCount,
    BenchmarkType,

    Threading,
    DegreeOfParallelism,
    CollectionType,
}

public abstract class PccBenchmark<TItem>
{
    #region Params

    //[Params(1024 * 4, 1024 * 16, 1024 * 64, 1024 * 256, 1024 * 1024, Priority = (int)PccBenchmarkParamOrder.ItemsCount)]
    [Params(1024 * 1024, Priority = (int)PccBenchmarkParamOrder.ItemsCount)]
    public int ItemsCount;

    //[Params(1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, Priority = (int)PccBenchmarkParamOrder.TasksCount)]
    //[Params(1, 4, 16, Priority = (int)PccBenchmarkParamOrder.DegreeOfParallelism)]
    [Params(4, 8, Priority = (int)PccBenchmarkParamOrder.DegreeOfParallelism)]
    public int DegreeOfParallelism;

    [ParamsAllValues(Priority = (int)PccBenchmarkParamOrder.CollectionType)]
    //[Params(PccCollectionType.Bag, PccCollectionType.Queue, PccCollectionType.Stack, Priority = (int)PccBenchmarkParamOrder.CollectionType)]
    //[Params(PccCollectionType.Bag, Priority = (int)PccBenchmarkParamOrder.CollectionType)]
    public PccCollectionType CollectionType;

    //[ParamsAllValues(Priority = (int)PccBenchmarkParamOrder.BenchmarkType)]
    //[Params(PccBenchmarkType.AddItems, PccBenchmarkType.TakeItems, PccBenchmarkType.AddWaitTakeItems, PccBenchmarkType.AddAndTakeItems, Priority = (int)PccBenchmarkParamOrder.BenchmarkType)]
    [Params(PccBenchmarkType.AddAndTakeItems, Priority = (int)PccBenchmarkParamOrder.BenchmarkType)]
    public PccBenchmarkType BenchmarkType;

    [ParamsAllValues(Priority = (int)PccBenchmarkParamOrder.Threading)]
    //[Params(PccThreading.Thread, PccThreading.ThreadPool, PccThreading.Task, PccThreading.ParallelLoop, Priority = (int)PccBenchmarkParamOrder.Threading)]
    //[Params(PccThreading.ParallelLoop, Priority = (int)PccBenchmarkParamOrder.Threading)]
    public PccThreading Threading;

    #endregion

    #region Fields

    private TItem[] SourceItems = Array.Empty<TItem>();
    private TItem[] ResultItems = Array.Empty<TItem>();

    private IProducerConsumerCollection<TItem> Collection = new ConcurrentBag<TItem>();

    private Action[][] Actions = Array.Empty<Action[]>();
    private Thread[][] Threads = Array.Empty<Thread[]>();
    private Task[][] Tasks = Array.Empty<Task[]>();

    #endregion

    #region SetupCleanup

    [GlobalSetup]
    public void GlobalSetup()
    {
        //while (!System.Diagnostics.Debugger.IsAttached)
        //    Thread.Sleep(TimeSpan.FromMilliseconds(100));

        SourceItems = ItemsGenerator().Take(ItemsCount).ToArray();
        ResultItems = new TItem[ItemsCount];
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        SourceItems = Array.Empty<TItem>();
        ResultItems = Array.Empty<TItem>();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        //Console.WriteLine("IterationSetup");

        Array.Clear(ResultItems);
        Collection = Helper.CreateCollection<TItem>(CollectionType);

        var actions = Helper.CreateActions(SourceItems, Collection, ResultItems, BenchmarkType, DegreeOfParallelism, Threading);
        switch (Threading)
        {
            case PccThreading.Thread:
                Threads = Helper.CreateThreads(actions);
                break;
            case PccThreading.ThreadPool:
                Actions = actions;
                break;
            case PccThreading.Task:
                Tasks = Helper.CreateTasks(actions);
                break;
            case PccThreading.ParallelInvoke:
                Actions = actions;
                break;
            case PccThreading.ParallelLoop:
                Actions = actions;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Threading));
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        try
        {
            //Console.WriteLine("IterationCleanup");

            var source = SourceItems.ToArray();
            var dest = Collection.Count > 0 ? Collection.ToArray() : ResultItems.ToArray();

            Array.Sort(source);
            Array.Sort(dest);

            if (!source.SequenceEqual(dest))
                throw new Exception("SourceItems != ResultItems");
        }
        finally
        {
            Collection = new ConcurrentBag<TItem>();

            Actions = Array.Empty<Action[]>();
            Tasks = Array.Empty<Task[]>();
            Threads = Array.Empty<Thread[]>();
        }
    }

    #endregion

    [Benchmark]
    public void Benchmark()
    {
        switch (Threading)
        {
            case PccThreading.Thread:
                foreach (var threads in Threads)
                    Helper.StartAndWaitAll(threads);
                break;

            case PccThreading.ThreadPool:
                foreach (var actions in Actions)
                    Helper.QueueAndWaitAll(actions);
                break;

            case PccThreading.Task:
                foreach (var tasks in Tasks)
                    Helper.StartAndWaitAll(tasks);
                break;

            case PccThreading.ParallelInvoke:
                foreach (var actions in Actions)
                    Parallel.Invoke(actions);
                break;

            case PccThreading.ParallelLoop:
                foreach (var actions in Actions)
                    Parallel.Invoke(actions);
                break;
        }
    }

    protected virtual IEnumerable<TItem> ItemsGenerator() => Helper.ItemsGenerator<TItem>();
}

public class PccBenchmark_Int : PccBenchmark<int>
{
    protected override IEnumerable<int> ItemsGenerator() => Helper.IntGenerator();
}

public class PccBenchmark_String : PccBenchmark<string>
{
    protected override IEnumerable<string> ItemsGenerator() => Helper.StringGenerator();
}
