using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace SqlBuildManager.Console.CommandLine
{
    /// <summary>
    /// Registry that maintains mappings between System.CommandLine options and their target setters on CommandLineArgs.
    /// This replaces the deprecated NamingConventionBinder's reflection-based magic binding with explicit, type-safe registration.
    /// </summary>
    public class OptionRegistry
    {
        private readonly Dictionary<Option, Action<CommandLineArgs, object>> _setters = new();
        private readonly List<Option> _registeredOptions = new();

        /// <summary>
        /// Gets all registered options.
        /// </summary>
        public IReadOnlyList<Option> RegisteredOptions => _registeredOptions;

        /// <summary>
        /// Registers an option with its setter action.
        /// </summary>
        /// <typeparam name="T">The type of the option value.</typeparam>
        /// <param name="option">The option to register.</param>
        /// <param name="setter">The action to set the value on CommandLineArgs.</param>
        public void Register<T>(Option<T> option, Action<CommandLineArgs, T> setter)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            _registeredOptions.Add(option);
            _setters[option] = (args, value) =>
            {
                if (value is T typedValue)
                {
                    setter(args, typedValue);
                }
            };
        }

        /// <summary>
        /// Applies all registered option values from the parse result to the CommandLineArgs instance.
        /// </summary>
        /// <param name="parseResult">The parse result containing parsed option values.</param>
        /// <param name="args">The CommandLineArgs instance to populate.</param>
        public void ApplyValues(ParseResult parseResult, CommandLineArgs args)
        {
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));
            if (args == null) throw new ArgumentNullException(nameof(args));

            foreach (var option in _registeredOptions)
            {
                // Check if this option was actually specified in the parse result
                var result = parseResult.GetResult(option);
                if (result != null && _setters.TryGetValue(option, out var setter))
                {
                    var value = GetValueFromParseResult(parseResult, option);
                    if (value != null)
                    {
                        setter(args, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value from ParseResult for the given option using reflection to handle the generic method.
        /// </summary>
        private static object GetValueFromParseResult(ParseResult parseResult, Option option)
        {
            // Get the GetValue<T> method and invoke it with the option's value type
            var valueType = option.ValueType;
            var method = typeof(ParseResultExtensions).GetMethod(
                nameof(ParseResultExtensions.GetValue),
                new[] { typeof(ParseResult), typeof(Option) });

            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(valueType);
                return genericMethod.Invoke(null, new object[] { parseResult, option });
            }

            return null;
        }

        /// <summary>
        /// Checks if an option is registered.
        /// </summary>
        public bool IsRegistered(Option option) => _setters.ContainsKey(option);

        /// <summary>
        /// Gets the count of registered options.
        /// </summary>
        public int Count => _registeredOptions.Count;
    }

    /// <summary>
    /// Extension methods for ParseResult to support type-safe value retrieval.
    /// </summary>
    public static class ParseResultExtensions
    {
        /// <summary>
        /// Gets the value of an option from the parse result.
        /// </summary>
        public static T GetValue<T>(this ParseResult parseResult, Option option)
        {
            if (option is Option<T> typedOption)
            {
                return parseResult.GetValue(typedOption);
            }
            return default;
        }
    }
}
