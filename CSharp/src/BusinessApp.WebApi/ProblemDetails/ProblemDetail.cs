namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data structure for returning an exception to the client
    /// ref: https://tools.ietf.org/html/rfc7807#section-4.1
    /// </summary>
    [DataContract]
    public class ProblemDetail : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> props = new Dictionary<string, object>();
        private string detail;
        private Uri instance;

        public ProblemDetail(int status, Uri type = null)
        {
            Type = type ?? new Uri("about:blank");

            props.Add(nameof(StatusCode), StatusCode = status);

            if (Enum.IsDefined(typeof(HttpStatusCode), status))
            {
                Title = ((HttpStatusCode)status).ToString();
            }
            else
            {
                Title = $"Unknown status: {status}";
            }

            props.Add(nameof(Title), Title);
            props[nameof(Type)] = Type;
        }

        public object this[string key]
        {
            get => props[key];
            set => props[key] = value;
        }

        public int? StatusCode { get; }
        public string Title { get; }
        public string Detail
        {
            set
            {
                if (value == null)
                {
                    props.Remove(nameof(Detail));
                    detail = null;
                }
                else
                {
                    props[nameof(Detail)] = detail = value;
                }
            }

            get => detail;
        }
        public Uri Type { get; }
        public Uri Instance
        {
            set
            {
                if (value == null)
                {
                    props.Remove(nameof(Instance));
                    instance = null;
                }
                else
                {
                    props[nameof(Instance)] = instance = value;
                }
            }

            get => instance;
        }

#region IDictionary implementation
        public ICollection<string> Keys => props.Keys;

        public ICollection<object> Values => props.Values;

        public int Count => props.Count;

        public bool IsReadOnly => props.IsReadOnly;

        public void Add(string key, object value)
        {
            props.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            props.Add(item);
        }

        public void Clear()
        {
            props.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return props.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return props.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            props.CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return props.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return props.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return props.Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return props.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)props).GetEnumerator();
        }
#endregion
    }
}
