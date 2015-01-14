﻿
using System;
#if __UNIFIED__
using Foundation;
#else
using MonoTouch.Foundation;
#endif
using Refractored.Xam.Settings.Abstractions;

namespace Refractored.Xam.Settings
{
  /// <summary>
  /// Main implementation for ISettings
  /// </summary>
  public class Settings : ISettings
  {

    private readonly object locker = new object();

    /// <summary>
    /// Gets the current value or the default that you specify.
    /// </summary>
    /// <typeparam name="T">Vaue of t (bool, int, float, long, string)</typeparam>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <returns>Value or default</returns>
    public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
    {
      lock (locker)
      {
        if (NSUserDefaults.StandardUserDefaults[key] == null)
          return defaultValue;

        Type typeOf = typeof(T);
        if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
          typeOf = Nullable.GetUnderlyingType(typeOf);
        }
        object value = null;
        var typeCode = Type.GetTypeCode(typeOf);
        var defaults = NSUserDefaults.StandardUserDefaults;
        switch (typeCode)
        {
          case TypeCode.Decimal:
            var savedDecimal = defaults.StringForKey(key);
            value = Convert.ToDecimal(savedDecimal, System.Globalization.CultureInfo.InvariantCulture);
            break;
          case TypeCode.Boolean:
            value = defaults.BoolForKey(key);
            break;
          case TypeCode.Int64:
            var savedInt64 = defaults.StringForKey(key);
            value = Convert.ToInt64(savedInt64, System.Globalization.CultureInfo.InvariantCulture);
            break;
          case TypeCode.Double:
            value = defaults.DoubleForKey(key);
            break;
          case TypeCode.String:
            value = defaults.StringForKey(key);
            break;
          case TypeCode.Int32:
#if __UNIFIED__
            value = (Int32)defaults.IntForKey(key);
#else
            value = defaults.IntForKey(key);
#endif
            break;
          case TypeCode.Single:
#if __UNIFIED__
            value = (float)defaults.FloatForKey(key);
#else
             value = defaults.FloatForKey(key);
#endif
           
            break;

          case TypeCode.DateTime:
            var savedTime = defaults.StringForKey(key);
            var ticks = string.IsNullOrWhiteSpace(savedTime) ? -1 : Convert.ToInt64(savedTime, System.Globalization.CultureInfo.InvariantCulture);
            if (ticks == -1)
              value = defaultValue;
            else
              value = new DateTime(ticks);
            break;
          default:

            if (defaultValue is Guid)
            {
              var outGuid = Guid.Empty;
              var savedGuid = defaults.StringForKey(key);
              if(string.IsNullOrWhiteSpace(savedGuid))
              {
                value = outGuid;
              }
              else
              {
                Guid.TryParse(savedGuid, out outGuid);
                value = outGuid;
              }
            }
            else
            {
              throw new ArgumentException(string.Format("Value of type {0} is not supported.", value.GetType().Name));
            }

            break;
        }


        return null != value ? (T)value : defaultValue;
      }
    }

    /// <summary>
    /// Adds or updates the value 
    /// </summary>
    /// <param name="key">Key for settting</param>
    /// <param name="value">Value to set</param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue<T>(string key, T value)
    {
      lock (locker)
      {
        if (Equals(default(T), value))
        {
          RemoveValue(key);
          return true;
        }
        Type typeOf = typeof(T);
        if (typeOf.IsGenericType && typeOf.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
          typeOf = Nullable.GetUnderlyingType(typeOf);
        }
        var typeCode = Type.GetTypeCode(typeOf);
        var defaults = NSUserDefaults.StandardUserDefaults;
        switch (typeCode)
        {
          case TypeCode.Decimal:
            defaults.SetString(Convert.ToString(value), key);
            break;
          case TypeCode.Boolean:
            defaults.SetBool(Convert.ToBoolean(value), key);
            break;
          case TypeCode.Int64:
            defaults.SetString(Convert.ToString(value), key);
            break;
          case TypeCode.Double:
            defaults.SetDouble(Convert.ToDouble(value), key);
            break;
          case TypeCode.String:
            defaults.SetString(Convert.ToString(value), key);
            break;
          case TypeCode.Int32:
            defaults.SetInt(Convert.ToInt32(value), key);
            break;
          case TypeCode.Single:
            defaults.SetFloat(Convert.ToSingle(value), key);
            break;
          case TypeCode.DateTime:
            defaults.SetString(Convert.ToString(((DateTime)(object)value).Ticks), key);
            break;
          default:
            if (value is Guid)
            {
              defaults.SetString(((Guid)(object)value).ToString(), key);
            }
            else
            {
              throw new ArgumentException(string.Format("Value of type {0} is not supported.", value.GetType().Name));
            }
            break;
        }
        try
        {
            defaults.Synchronize();
          
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to save: " + key, " Message: " + ex.Message);
        }
      }

     
      return true;
    }

    public void RemoveValue(string key)
    {
      lock (locker)
      {
        var defaults = NSUserDefaults.StandardUserDefaults;
        defaults.RemoveObject(key);

        try
        {
          defaults.Synchronize();

        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to save: " + key, " Message: " + ex.Message);
        }
      }
    }

    /// <summary>
    /// Saves all currents settings outs.
    /// </summary>
    [Obsolete("Save is deprecated and settings are automatically saved when AddOrUpdateValue is called.")]
    public void Save()
    {
     
    }

  }

}
