using DynamicData.Kernel;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.Windows;
using NexusMods.MnemonicDB.Abstractions.ElementComparers;

namespace NexusMods.App.UI.WorkspaceSystem;

/// <summary>
/// Represents a factory for creating pages.
/// </summary>
public interface IPageFactory
{
    /// <summary>
    /// Gets the unique identifier of this factory.
    /// </summary>
    public PageFactoryId Id { get; }

    /// <summary>
    /// Creates a new page using the provided context.
    /// </summary>
    public Page Create(PageData context);

    /// <summary>
    /// Returns details about every page that can be created with this factory in the given <see cref="IWorkspaceContext"/>.
    /// </summary>
    public IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext);

    public Optional<OpenPageBehaviorType> DefaultOpenPageBehavior { get; }
}

/// <summary>
/// Generic implementation of <see cref="IPageFactory"/>
/// </summary>
/// <typeparam name="TViewModel"></typeparam>
/// <typeparam name="TContext"></typeparam>
public interface IPageFactory<out TViewModel, in TContext> : IPageFactory
    where TViewModel : class, IPageViewModelInterface
{
    Page IPageFactory.Create(PageData pageData)
    {
        if (pageData.Context is not TContext actualContext)
            throw new ArgumentException($"Unsupported type: {pageData.Context.GetType()}");

        var vm = CreateViewModel(actualContext);
        return new Page
        {
            ViewModel = vm,
            PageData = pageData with { FactoryId = Id },
        };
    }

    /// <summary>
    /// Creates a new view model using the provided context.
    /// </summary>
    public TViewModel CreateViewModel(TContext context);
}

/// <summary>
/// Abstract class to easily implement <see cref="IPageFactory"/>.
/// </summary>
[PublicAPI]
public abstract class APageFactory<TViewModel> : IPageFactory<TViewModel, Null>
    where TViewModel : class, IPageViewModelInterface 
{
    /// <inheritdoc/>
    public abstract PageFactoryId Id { get; }

    protected readonly IServiceProvider ServiceProvider;
    protected readonly IWindowManager WindowManager;

    protected APageFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        WindowManager = serviceProvider.GetRequiredService<IWindowManager>();
    }

    /// <inheritdoc/>
    public TViewModel CreateViewModel(Null context)
    {
        return CreateViewModel();
    }
    
    /// <summary>
    /// Creates a new view model.
    /// </summary>
    protected abstract TViewModel CreateViewModel();

    /// <summary>
    /// Creates a new <see cref="PageData"/> object with the provided context.
    /// </summary>
    /// <param name="arg1"></param>
    /// <returns></returns>
    protected PageData CreatePageData() => new()
    {
        FactoryId = Id,
        Context = Null.Instance,
    };

    /// <inheritdoc/>
    public virtual IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext) => Array.Empty<PageDiscoveryDetails?>();

    /// <inheritdoc/>
    public virtual Optional<OpenPageBehaviorType> DefaultOpenPageBehavior { get; } = Optional<OpenPageBehaviorType>.None;
}



/// <summary>
/// Abstract class to easily implement <see cref="IPageFactory"/>.
/// </summary>
[PublicAPI]
public abstract class APageFactory<TViewModel, TArg1> : IPageFactory<TViewModel, TArg1>
    where TViewModel : class, IPageViewModelInterface 
    where TArg1 : notnull
{
    /// <inheritdoc/>
    public abstract PageFactoryId Id { get; }

    protected readonly IServiceProvider ServiceProvider;
    protected readonly IWindowManager WindowManager;

    protected APageFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        WindowManager = serviceProvider.GetRequiredService<IWindowManager>();
    }

    /// <inheritdoc/>
    public abstract TViewModel CreateViewModel(TArg1 context);

    /// <summary>
    /// Creates a new <see cref="PageData"/> object with the provided context.
    /// </summary>
    /// <param name="arg1"></param>
    /// <returns></returns>
    protected PageData CreatePageData(TArg1 arg1) => new()
    {
        FactoryId = Id,
        Context = arg1,
    };

    /// <inheritdoc/>
    public virtual IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext) => Array.Empty<PageDiscoveryDetails?>();

    /// <inheritdoc/>
    public virtual Optional<OpenPageBehaviorType> DefaultOpenPageBehavior { get; } = Optional<OpenPageBehaviorType>.None;
}
