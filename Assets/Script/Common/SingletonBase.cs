using System;

/// <summary>
/// A base class for the singleton design pattern.
/// </summary>
/// <typeparam name="T">Class type of the singleton</typeparam>
public abstract class SingletonBase<T> where T : class
{
    /// <summary>
    /// Static instance. Needs to use lambda expression
    /// to construct an instance (since constructor is private).
    /// </summary>
    private static readonly Lazy<T> instance = new Lazy<T>(() => CreateInstance());


    /// <summary>
    /// Gets the instance of this singleton.
    /// </summary>
    public static T Instance { get { return instance.Value; } }

    /// <summary>
    /// Creates an instance of T via reflection since T's constructor is expected to be private.
    /// </summary>
    /// <returns></returns>
    private static T CreateInstance()
    {
        return Activator.CreateInstance(typeof(T), true) as T;
    }
}
