using System.Linq.Expressions;
using System.Reflection;
using Hangfire;
using MarcusPrado.Platform.Hangfire.Attributes;
using MarcusPrado.Platform.Hangfire.Scheduler;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Hangfire.Registrar;

/// <summary>
/// Scans a set of assemblies for types decorated with <see cref="RecurringJobAttribute"/>
/// and registers them as Hangfire recurring jobs via <see cref="IRecurringJobManager"/>.
/// </summary>
public sealed partial class HangfireRecurringJobRegistrar
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireRecurringJobRegistrar> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="HangfireRecurringJobRegistrar"/>.
    /// </summary>
    /// <param name="recurringJobManager">The Hangfire recurring-job manager to register jobs against.</param>
    /// <param name="logger">Logger instance.</param>
    public HangfireRecurringJobRegistrar(
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireRecurringJobRegistrar> logger)
    {
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Scans each supplied <paramref name="assemblies"/> for concrete classes that implement
    /// <see cref="IHangfireJob"/> and are annotated with <see cref="RecurringJobAttribute"/>,
    /// then registers every match with Hangfire using <see cref="IRecurringJobManager"/>.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    public void RegisterFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        foreach (var assembly in assemblies)
        {
            RegisterFromAssembly(assembly);
        }
    }

    /// <summary>
    /// Scans a single <paramref name="assembly"/> for job types and registers them.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    public void RegisterFromAssembly(Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var jobTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IHangfireJob).IsAssignableFrom(t))
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<RecurringJobAttribute>()))
            .Where(x => x.Attr is not null)
            .ToList();

        foreach (var (jobType, attr) in jobTypes)
        {
            RegisterJob(jobType, attr!);
        }
    }

    /// <summary>
    /// Returns all types decorated with <see cref="RecurringJobAttribute"/> found in the given assemblies
    /// without registering them. Useful for introspection and testing.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>An enumerable of (type, attribute) pairs.</returns>
    public static IEnumerable<(Type JobType, RecurringJobAttribute Attribute)> FindAttributedTypes(
        params Assembly[] assemblies)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IHangfireJob).IsAssignableFrom(t))
            .Select(t => (t, Attr: t.GetCustomAttribute<RecurringJobAttribute>()))
            .Where(x => x.Attr is not null)
            .Select(x => (x.t, x.Attr!));
    }

    private void RegisterJob(Type jobType, RecurringJobAttribute attr)
    {
        var jobId = jobType.FullName ?? jobType.Name;

        LogRegisterJob(jobId, jobType.Name, attr.CronExpression, attr.Queue);

        // Locate the generic AddOrUpdate<TJob>(id, Expression<Action<TJob>>, cron) extension method
        // and invoke it at runtime so we don't need a compile-time type parameter.
        // Build: (TJob job) => job.ExecuteAsync(CancellationToken.None)
        var param = Expression.Parameter(jobType, "job");
        var ctNone = Expression.Constant(CancellationToken.None);
        var executeMethod = typeof(IHangfireJob).GetMethod(nameof(IHangfireJob.ExecuteAsync))!;
        var callExpr = Expression.Call(param, executeMethod, ctNone);
        var lambdaType = typeof(Func<,>).MakeGenericType(jobType, typeof(Task));
        var lambda = Expression.Lambda(lambdaType, callExpr, param);

        // Find RecurringJobManagerExtensions.AddOrUpdate<T>(manager, id, Expression<Func<T,Task>>, cron).
        // This overload handles async jobs.  Fall back to the Action<T> overload if absent.
        var genericAddOrUpdate = typeof(RecurringJobManagerExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == nameof(RecurringJobManagerExtensions.AddOrUpdate)
                && m.IsGenericMethodDefinition
                && m.GetParameters().Length == 4
                && m.GetParameters()[2].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
                && m.GetParameters()[2].ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<,>)
                && m.GetParameters()[3].ParameterType == typeof(string));

        if (genericAddOrUpdate is not null)
        {
            var closedMethod = genericAddOrUpdate.MakeGenericMethod(jobType);
            closedMethod.Invoke(null, [_recurringJobManager, jobId, lambda, attr.CronExpression]);
        }
        else
        {
            // Fallback: use non-generic IRecurringJobManager.AddOrUpdate with a Hangfire.Common.Job
            var hangfireJob = new global::Hangfire.Common.Job(jobType, executeMethod, CancellationToken.None);
            _recurringJobManager.AddOrUpdate(jobId, hangfireJob, attr.CronExpression, new RecurringJobOptions());
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Registering recurring job {JobId} ({JobType}) with cron {Cron} on queue {Queue}")]
    private partial void LogRegisterJob(string jobId, string jobType, string cron, string queue);
}
