using System;
using System.Collections.Generic;

namespace DependencyInjection
{
    public static partial class DI
    {
        private readonly static Dictionary<Type, object> Singletons = new Dictionary<Type, object>();
        private readonly static Dictionary<Type, Type> SingletonTypes = new Dictionary<Type, Type>();
        private readonly static Dictionary<Type, Type> Transients = new Dictionary<Type, Type>();
        public static void AddSingleton<T1, T2>() where T2 : T1
        {
            SingletonTypes[typeof(T1)] = typeof(T2);
            Singletons[typeof(T1)] = null;
        }
        public static void AddTransient<T1, T2>() where T2 : T1
        {
            Transients[typeof(T1)] = typeof(T2);
        }
        private static object Create(Type t, object[] additonalParametrs = null)
        {
            List<object> parameters = new List<object>();
            int i = 0;
            foreach (var constructor in t.GetConstructors())
            {
                parameters.Clear();
                bool parametersSatisfied = true;
                foreach (var parameter in constructor.GetParameters())
                {
                    if (additonalParametrs != null && i < additonalParametrs.Length && additonalParametrs[i].GetType() == parameter.ParameterType)
                    {
                        parameters.Add(additonalParametrs[i]);
                        i++;
                        continue;
                    }
                    object parObject = New(parameter.ParameterType);
                    if (parObject == null)
                    {
                        parametersSatisfied = false;
                        break;
                    }
                    parameters.Add(parObject);
                }
                if (parametersSatisfied)
                {
                    return constructor.Invoke(parameters.ToArray());
                }
            }
            return null;
        }
        private static object New(Type t, object[] parameters = null)
        {
            if (Singletons.TryGetValue(t, out object o))
            {
                if(o == null)
                {
                    object res;
                    if (SingletonTypes[t].IsInterface || SingletonTypes[t].IsAbstract)
                    {
                        res = New(SingletonTypes[t], parameters);
                    }
                    else
                    {
                        res = Create(SingletonTypes[t], parameters);
                    }
                    Singletons[t] = res;
                    return res;
                }
                if (parameters != null && parameters.Length > 0)
                {
                    throw new InvalidOperationException($"Singleton object {t.FullName} already initialized");
                }
                return o;
            }
            if (Transients.TryGetValue(t, out Type resType))
            {
                if (resType.IsInterface || resType.IsAbstract)
                {
                    return New(resType, parameters);
                }
                else
                {
                    return Create(resType, parameters);
                }
            }
            if (t.IsInterface || t.IsAbstract)
            {
                return null;
            }
            return Create(t, parameters);
        }
        public static T Get<T>(params object[] parameters)
        {
            return (T)New(typeof(T), parameters);
        }
    }
}
