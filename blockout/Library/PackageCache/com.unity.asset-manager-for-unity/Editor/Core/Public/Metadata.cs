using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using UnityEngine;

namespace Unity.AssetManager.Editor
{
    /// <summary>
    /// A base for metadata classes.
    /// </summary>
    [Serializable]
    public abstract class Metadata
    {
        /// <summary>
        /// The key for the metadata.
        /// </summary>
        /// <remarks>
        /// Can be converted from a display name to a key using the <see cref="AssetManagerClient.GetMetadataKeyFromDisplayNameAsync"/> class.
        /// </remarks>
        [SerializeField, ReadOnly]
        string m_Key;

        internal string Key
        {
            get => m_Key;
            set => m_Key = value;
        }

        private protected Metadata(string key)
        {
            m_Key = key;
        }

        internal virtual void Validate()
        {
            if (string.IsNullOrEmpty(m_Key))
            {
                throw new InvalidOperationException("Key is null or empty.");
            }
        }

        internal bool CastToBool()
        {
            switch (this)
            {
                case BooleanMetadata booleanMetadata:
                    return booleanMetadata.Value;
                case StringMetadata stringMetadata:
                    if (bool.TryParse(stringMetadata.Value, out var boolValue))
                    {
                        return boolValue;
                    }
                    throw new ArgumentException($"Cannot convert string metadata value '{stringMetadata.Value}' to boolean for key: {Key}");
                case NumberMetadata numberMetadata:
                    return numberMetadata.Value != 0;
                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to boolean for key: {Key}");

            }
        }

        internal double CastToNumber()
        {
            switch (this)
            {
                case NumberMetadata numberMetadata:
                    return numberMetadata.Value;
                case StringMetadata stringMetadata:
                    if (double.TryParse(stringMetadata.Value, out var doubleValue))
                    {
                        return doubleValue;
                    }
                    throw new ArgumentException($"Cannot convert string metadata value '{stringMetadata.Value}' to double for key: {Key}");
                case BooleanMetadata booleanMetadata:
                    return booleanMetadata.Value ? 1 : 0;
                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to double for key: {Key}");

            }
        }

        internal string CastToString()
        {
            switch (this)
            {
                case StringMetadata stringMetadata:
                    return stringMetadata.Value;
                case NumberMetadata numberMetadata:
                    return numberMetadata.Value.ToString();
                case BooleanMetadata booleanMetadata:
                    return booleanMetadata.Value.ToString().ToLower();
                case DateTimeMetadata dateTimeMetadata:
                    return dateTimeMetadata.DateTime.ToString();
                case DateTimeRangeMetadata dateTimeRangeMetadata:
                    return $"{dateTimeRangeMetadata.StartDateTime} - {dateTimeRangeMetadata.EndDateTime}";
                case MultiValueMetadata multiValueMetadata:
                    return string.Join(", ", multiValueMetadata.Values);
                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to string for key: {Key}");

            }
        }

        internal List<string> CastToMultiSelection()
        {
            switch (this)
            {
                case MultiValueMetadata multiValueMetadata:
                    return multiValueMetadata.Values.ToList();
                case StringMetadata stringMetadata:
                    return new List<string> { stringMetadata.Value };
                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to string list for key: {Key}");
            }
        }

        internal DateTimeEntry CastToDateTimeEntry()
        {
            switch (this)
            {
                case DateTimeMetadata dateTimeMetadata:
                    return new DateTimeEntry(dateTimeMetadata.DateTime);
                case StringMetadata stringMetadata:
                    if (DateTime.TryParse(stringMetadata.Value, out var dateTimeValue))
                    {
                        return new DateTimeEntry(dateTimeValue);
                    }
                    throw new ArgumentException($"Cannot convert string metadata value '{stringMetadata.Value}' to DateTime for key: {Key}");
                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to DateTime for key: {Key}");
            }
        }

        internal UriEntry CastToUriEntry()
        {
            switch (this)
            {
                case StringMetadata stringMetadata:
                    var value = stringMetadata.Value;
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException($"Cannot convert empty string metadata value to Uri for key: {Key}");
                    }

                    // Check for markdown link format [label](url)
                    string url = value; // default as plain url
                    string label = null;

                    if (value.StartsWith("[") && value.Contains("](") && value.EndsWith(")"))
                    {
                        var endOfLabelIndex = value.IndexOf("](", StringComparison.Ordinal);
                        label = value.Substring(1, endOfLabelIndex - 1);
                        url = value.Substring(endOfLabelIndex + 2, value.Length - endOfLabelIndex - 3);
                    }

                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        return new UriEntry(uri, label);
                    }
                    throw new ArgumentException($"Cannot convert string metadata value '{stringMetadata.Value}' to Uri for key: {Key}");

                default:
                    throw new ArgumentException($"Cannot convert metadata of type {GetType()} to Uri for key: {Key}");
            }
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadata by string value.
    /// </summary>
    [Serializable]
    public sealed class StringMetadata : Metadata
    {
        [SerializeField]
        string m_Value;

        /// <summary>
        /// The searchable value of a text metadata. This must match the entirety of the text value.
        /// </summary>
        /// <example>
        /// Given a metadata value of "Hello World", the value of the search string must be "Hello World".<br/>
        /// Given a url metadata with a label "Unity" and a url of "https://unity.com", the value of the search string must be "[Unity](https://unity.com)".<br/>
        /// Given a url metadata with no label and a url of "https://unity.com", the value of the search string must be "https://unity.com".<br/>
        /// Given a boolean metadata, valid search terms are "true" and "false". Alternatively, search with a <see cref="BooleanMetadata"/>. <br/>
        /// Given a number metadata, the string representation of the value can be used. Alternatively, search with a <see cref="NumberMetadata"/>. <br/>
        /// </example>
        public string Value => m_Value;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(m_Value))
            {
                throw new InvalidOperationException($"Value for metadata key {Key} is null or empty.");
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringMetadata"/> class.
        /// </summary>
        /// <param name="value">The searchable value of a text metadata. This must match the entirety of the text value.</param>
        /// <example>
        /// Given a metadata value of "Hello World", the value of the search string must be "Hello World".<br/>
        /// Given a url metadata with a label "Unity" and a url of "https://unity.com", the value of the search string must be "[Unity](https://unity.com)".<br/>
        /// Given a url metadata with no label and a url of "https://unity.com", the value of the search string must be "https://unity.com".<br/>
        /// </example>
        public StringMetadata(string value)
            : base(null)
        {
            m_Value = value;
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadata by number value.
    /// </summary>
    [Serializable]
    public sealed class NumberMetadata : Metadata
    {
        [SerializeField]
        double m_Value;

        /// <summary>
        /// The searchable value of a number metadata. This must exactly match the number value.
        /// </summary>
        /// <example>
        /// Given a number value of 123, the number value must be 123.<br/>
        /// Given a number value of 123.45, the number value must be 123.45.
        /// </example>
        public double Value => m_Value;

        /// <summary>
        /// Creates a new instance of the <see cref="NumberMetadata"/> class.
        /// </summary>
        /// <param name="value">The searchable value of a number metadata. This must exactly match the number value. </param>
        /// <example>
        /// Given a number value of 123, the number value must be 123.<br/>
        /// Given a number value of 123.45, the number value must be 123.45.
        /// </example>
        public NumberMetadata(double value)
            : base(null)
        {
            m_Value = value;
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadata by boolean value.
    /// </summary>
    [Serializable]
    public sealed class BooleanMetadata : Metadata
    {
        [SerializeField]
        bool m_Value;

        /// <summary>
        /// The searchable value of a boolean metadata. This must exactly match the boolean value.
        /// </summary>
        public bool Value => m_Value;

        /// <summary>
        /// Creates a new instance of the <see cref="BooleanMetadata"/> class.
        /// </summary>
        /// <param name="value">The searchable value of a boolean metadata. This must exactly match the boolean value. </param>
        public BooleanMetadata(bool value)
            : base(null)
        {
            m_Value = value;
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadata by date time value.
    /// </summary>
    [Serializable]
    public sealed class DateTimeMetadata : Metadata
    {
        /// <summary>
        /// The searchable value of a timestamp metadata. This must exactly match the timestamp value.
        /// </summary>
        /// <remarks>
        /// Given a <see cref="DateTime"/>, use <see cref="DateTime.Ticks"/> to represent the timestamp.
        /// </remarks>
        [SerializeField]
        long Ticks;

        /// <summary>
        /// The format of the timestamp metadata. This is used to determine the kind of the timestamp.
        /// </summary>
        /// <remarks>
        /// Given a <see cref="DateTime"/>, use <see cref="DateTime.Kind"/> to specify the kind of the timestamp.
        /// </remarks>
        [SerializeField]
        DateTimeKind Kind;

        /// <summary>
        /// The searchable value of a timestamp metadata. This must exactly match the timestamp value.
        /// </summary>
        public DateTime DateTime => new(Ticks, Kind);

        /// <summary>
        /// Creates a new instance of the <see cref="DateTimeMetadata"/> class.
        /// </summary>
        /// <param name="dateTime">The searchable value of a timestamp metadata. This must exactly match the timestamp value.</param>
        public DateTimeMetadata(DateTime dateTime)
            : base(null)
        {
            Ticks = dateTime.Ticks;
            Kind = dateTime.Kind;
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadata by date time value.
    /// </summary>
    [Serializable]
    public sealed class DateTimeRangeMetadata : Metadata
    {
        /// <summary>
        /// The minimum searchable value of a timestamp metadata. This must less than or equal to the expected timestamp.
        /// </summary>
        /// <remarks>
        /// Given a <see cref="DateTime"/>, use <see cref="DateTime.Ticks"/> to represent the timestamp.
        /// </remarks>
        [SerializeField]
        long StartTicks;

        /// <summary>
        /// The maximum searchable value of a timestamp metadata. This must greater than the expected timestamp.
        /// </summary>
        /// <remarks>
        /// Given a <see cref="DateTime"/>, use <see cref="DateTime.Ticks"/> to represent the timestamp.
        /// </remarks>
        [SerializeField]
        long EndTicks;

        /// <summary>
        /// The format of the timestamp metadata. This is used to determine the kind of the timestamp.
        /// </summary>
        /// <remarks>
        /// Given a <see cref="DateTime"/>, use <see cref="DateTime.Kind"/> to specify the kind of the timestamp.
        /// </remarks>
        [SerializeField]
        DateTimeKind Kind;

        /// <summary>
        /// The searchable value of a timestamp metadata. This must be less than or equal to the expected timestamp.
        /// </summary>
        public DateTime StartDateTime => new(StartTicks, Kind);

        /// <summary>
        /// The searchable value of a timestamp metadata. This must be greater than the expected timestamp.
        /// </summary>
        public DateTime EndDateTime => new(EndTicks, Kind);

        /// <summary>
        /// Creates a new instance of the <see cref="DateTimeMetadata"/> class.
        /// </summary>
        /// <param name="startDateTime">This must be less than or equal to the expected timestamp.</param>
        /// <param name="endDateTime">This must be greater than the expected timestamp.</param>
        public DateTimeRangeMetadata(DateTime startDateTime, DateTime endDateTime)
            : base(null)
        {
            if (startDateTime.Kind != endDateTime.Kind)
            {
                throw new ArgumentException($"The min and max date time must have the same kind. Min: {startDateTime.Kind}, Max: {endDateTime.Kind}");
            }

            Kind = startDateTime.Kind;
            StartTicks = startDateTime.Ticks;
            EndTicks = endDateTime.Ticks;
        }
    }

    /// <summary>
    /// A class which can be used as a filter for searching for metadatas with multiple string values.
    /// </summary>
    [Serializable]
    public sealed class MultiValueMetadata : Metadata
    {
        [SerializeField]
        string[] m_Values;

        /// <summary>
        /// For metadata that can have multiple values, this is the list of values to search for. This search values must match all values in the metadata.
        /// </summary>
        /// <example>
        /// Given a metadata with values of "selection-A", "selection-B", and "selection-C", the array for search must contain "selection-A", "selection-B", and "selection-C".<br/>
        /// </example>
        public IEnumerable<string> Values => m_Values;

        internal override void Validate()
        {
            base.Validate();

            if (m_Values == null || m_Values.Length == 0)
            {
                throw new InvalidOperationException($"Values for key {Key} is null or empty.");
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MultiValueMetadata"/> class.
        /// </summary>
        /// <param name="values">For metadata that can have multiple values, this is the list of values to search for. This search values must match all values in the metadata.</param>
        public MultiValueMetadata(IEnumerable<string> values)
            : base(null)
        {
            m_Values = values?.ToArray() ?? Array.Empty<string>();
        }
    }


}
