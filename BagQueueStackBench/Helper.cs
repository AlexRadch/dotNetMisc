using BagQueueStackBench;
using System.Collections.Concurrent;

static class Helper
{
    public static IEnumerable<TItem> ItemsGenerator<TItem>(int seed = int.MinValue)
    {
        var item = default(TItem);
        return item switch
            {
                int _ => (IEnumerable<TItem>)IntGenerator(seed),
                string _ => (IEnumerable<TItem>)StringGenerator(seed),
                _ => throw new InvalidOperationException(),
            };
    }

    public static IEnumerable<int> IntGenerator(int seed = int.MinValue)
    {
        var random = seed == int.MinValue ? new Random() : new Random(seed);

        while (true)
            yield return random.Next();
    }

    public static IEnumerable<string> StringGenerator(int seed = int.MinValue) => IntGenerator(seed).Select(i => i.ToString());

    public static IProducerConsumerCollection<TItem> CreateCollection<TItem>(PccCollectionType collectionType)
            => collectionType switch
        {
            PccCollectionType.Bag => new ConcurrentBag<TItem>(),
            PccCollectionType.Queue => new ConcurrentQueue<TItem>(),
            PccCollectionType.Stack => new ConcurrentStack<TItem>(),
            _ => throw new ArgumentOutOfRangeException(nameof(collectionType)),
        };

    public static Action[][] CreateActions<TItem>(TItem[] sourceItems, IProducerConsumerCollection<TItem> collection, TItem[] resultItems,
        PccBenchmarkType benchmarkType, int degreeOfParallelism, PccThreading threading)
    {
        Action[] AddActions() => CreateActions(collection, sourceItems, degreeOfParallelism, AddItems<TItem>);
        Action[] TakeActions() => CreateActions(collection, resultItems, degreeOfParallelism, TakeItems<TItem>);

        void AddItemsLoop() => AddItemsParallelLoop(collection, sourceItems, degreeOfParallelism);
        void TakeItemsLoop() => TakeItemsParallelLoop(collection, resultItems, degreeOfParallelism);

        switch (benchmarkType)
        {
            case PccBenchmarkType.AddItems:
                return threading != PccThreading.ParallelLoop ? 
                    new Action[][] { AddActions() } : 
                    new Action[][] { new Action[] { AddItemsLoop } };

            case PccBenchmarkType.TakeItems:
                {
                    // Add sourceItems to collection with the same threading
                    switch (threading)
                    {
                        case PccThreading.Thread:
                            StartAndWaitAll(CreateThreads(AddActions()));
                            break;
                        case PccThreading.ThreadPool:
                            QueueAndWaitAll(AddActions());
                            break;
                        case PccThreading.Task:
                            StartAndWaitAll(CreateTasks(AddActions()));
                            break;
                        case PccThreading.ParallelInvoke:
                            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = degreeOfParallelism}, AddActions());
                            break;
                        case PccThreading.ParallelLoop:
                            AddItemsLoop();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(threading));
                    }

                    return threading != PccThreading.ParallelLoop ? 
                        new Action[][] { TakeActions() } : 
                        new Action[][] { new Action[] { TakeItemsLoop } }; 
                }

            case PccBenchmarkType.AddWaitTakeItems:
                return threading != PccThreading.ParallelLoop ? 
                    new Action[][] { AddActions(), TakeActions() } : 
                    new Action[][] { new Action[] { AddItemsLoop }, new Action[] { TakeItemsLoop } };

            case PccBenchmarkType.AddAndTakeItems:
                return threading != PccThreading.ParallelLoop ? 
                    new Action[][] { AddActions().Concat(TakeActions()).ToArray() } :
                    new Action[][] { new Action[] { AddItemsLoop, TakeItemsLoop } };


            default:
                throw new ArgumentOutOfRangeException(nameof(benchmarkType));
        }
    }

    public delegate void WorkDelegate<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int offset, int count);

    public static Action[] CreateActions<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int actionsCount, WorkDelegate<TItem> work)
    {
        var actions = new Action[actionsCount];

        for (var index = 0; index < actionsCount; index++)
        {
            var offset = items.Length * index / actionsCount;
            var count = items.Length * (index + 1) / actionsCount - offset;

            //Console.WriteLine($"Offset = {offset}, Count = {count}");
            actions[index] = () => work(collection, items, offset, count);
        }

        return actions;
    }

    public static void AddItems<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int offset, int count)
    {
        offset %= items.Length;
        while (count > 0)
        {
            if (!collection.TryAdd(items[offset]))
            {
                // If we failed, go to the slow path and loop around until we succeed.
                SpinWait spin = default;
                do
                {
                    Console.WriteLine($"!collection.TryAdd offset = {offset} spin.Count = {spin.Count}");
                    spin.SpinOnce();
                }
                while (!collection.TryAdd(items[offset]));
            }

            offset++;
            if (offset >= items.Length)
                offset -= items.Length;
            count--;
        }
    }
    public static void TakeItems<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int offset, int count)
    {
        offset %= items.Length;
        while (count > 0)
        {
            if (!collection.TryTake(out var item))
            {
                // If we failed, go to the slow path and loop around until we succeed.
                SpinWait spin = default;
                do
                {
                    Console.WriteLine($"!collection.TryTake offset = {offset} collection.Count = {collection.Count} spin.Count = {spin.Count}");
                    spin.SpinOnce();
                }
                while (!collection.TryTake(out item));
            }
            items[offset] = item;

            offset++;
            if (offset >= items.Length)
                offset -= items.Length;
            count--;
        }
    }
    public static void AddItemsParallelLoop<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int degreeOfParallelism)
    {
        Parallel.For(0, items.Length, new ParallelOptions() { MaxDegreeOfParallelism = degreeOfParallelism }, offset => 
        {
            if (!collection.TryAdd(items[offset]))
            {
                // If we failed, go to the slow path and loop around until we succeed.
                SpinWait spin = default;
                do
                {
                    Console.WriteLine($"!collection.TryAdd(items[{offset}]) spin.Count = {spin.Count}");
                    spin.SpinOnce();
                }
                while (!collection.TryAdd(items[offset]));
            }
        });
    }
    public static void TakeItemsParallelLoop<TItem>(IProducerConsumerCollection<TItem> collection, TItem[] items, int degreeOfParallelism)
    {
        Parallel.For(0, items.Length, new ParallelOptions() { MaxDegreeOfParallelism = degreeOfParallelism }, offset =>
        {
            if (!collection.TryTake(out var item))
            {
                // If we failed, go to the slow path and loop around until we succeed.
                SpinWait spin = default;
                do
                {
                    Console.WriteLine($"!collection.TryTake({offset}) collection.Count = {collection.Count} spin.Count = {spin.Count}");
                    spin.SpinOnce();
                }
                while (!collection.TryTake(out item));
            }
            items[offset] = item;
        });
    }

    public static Thread[][] CreateThreads(Action[][] actions)
    {
        var threads = new Thread[actions.Length][];

        for (var index = 0; index < actions.Length; index++)
            threads[index] = CreateThreads(actions[index]);

        return threads;
    }
    public static Thread[] CreateThreads(Action[] actions)
    {
        var threads = new Thread[actions.Length];

        for (var index = 0; index < actions.Length; index++)
            threads[index] = new Thread(new ThreadStart(actions[index]));

        return threads;
    }

    public static Task[][] CreateTasks(Action[][] actions)
    {
        var tasks = new Task[actions.Length][];

        for (var index = 0; index < actions.Length; index++)
            tasks[index] = CreateTasks(actions[index]);

        return tasks;
    }
    public static Task[] CreateTasks(Action[] actions)
    {
        var tasks = new Task[actions.Length];

        for (var index = 0; index < actions.Length; index++)
                tasks[index] = new Task(actions[index]);

        return tasks;
    }

    public static void StartAll(Thread[] threads)
    {
        foreach (var thread in threads)
            thread.Start();
    }
    public static void StartAll(Task[] tasks)
    {
        foreach (var task in tasks)
        {
            //Console.WriteLine(task.Status);
            task.Start(TaskScheduler.Default);
        }
    }
    public static void WaitAll(Thread[] threads)
    {
        foreach (var thread in threads)
        {
            thread.Join();
            //while (thread.IsAlive)
            //    Thread.Yield();
        }
    }

    public static void QueueAndWaitAll(Action[] actions)
    {
        using var countdownEvent = new CountdownEvent(actions.Length);

        foreach (var action in actions)
        {
            void callBack(object? state)
            {
                try
                {
                    action();
                }
                finally
                {
                    countdownEvent.Signal();
                }
            }

            if (!ThreadPool.QueueUserWorkItem(callBack))
            {
                // If we failed, go to the slow path and loop around until we succeed.
                SpinWait spin = default;
                do
                {
                    Console.WriteLine($"!ThreadPool.QueueUserWorkItem(callBack) spin.Count = {spin.Count}");
                    spin.SpinOnce();
                }
                while (!ThreadPool.QueueUserWorkItem(callBack));
            }
        }

        countdownEvent.Wait();
    }
    public static void StartAndWaitAll(Thread[] threads)
    {
        StartAll(threads);
        WaitAll(threads);
    }
    public static void StartAndWaitAll(Task[] tasks)
    {
        StartAll(tasks);
        Task.WaitAll(tasks);
    }
}
