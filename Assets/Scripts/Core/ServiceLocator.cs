using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        Type type = typeof(T);
        services[type] = service;
    }

    public static void Unregister<T>() where T : class
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            services.Remove(type);
        }
    }

    public static T Get<T>() where T : class
    {
        Type type = typeof(T);
        if (services.TryGetValue(type, out object service))
        {
            return (T)service;
        }
        return null;
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        service = Get<T>();
        return service != null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        services.Clear();
    }
}
