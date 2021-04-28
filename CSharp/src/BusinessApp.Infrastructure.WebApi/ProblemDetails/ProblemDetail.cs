using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;

namespace BusinessApp.Infrastructure.WebApi.ProblemDetails
{
#pragma warning disable CA1710
    /// <summary>
    /// Data structure for returning an exception to the client
    /// ref: https://tools.ietf.org/html/rfc7807#section-4.1
    /// </summary>
    [DataContract]
    public class ProblemDetail : IDictionary<string, object>
#pragma warning disable CA1710
    {
        private readonly IDictionary<string, object> props = new Dictionary<string, object>();
        private string? detail;
        private Uri? instance;

        public ProblemDetail(int status, Uri? type = null)
        {
            Type = type ?? new Uri("about:blank");

            props.Add(nameof(StatusCode), StatusCode = status);

            Title = Enum.IsDefined(typeof(HttpStatusCode), status)
                ? ((HttpStatusCode)status).ToString()
                : $"Unknown status: {status}";

            props.Add(nameof(Title), Title);
            props[nameof(Type)] = Type;
        }

        public object this[string key]
        {
            get => props[key];
            set => props[key] = value;
        }

        public int StatusCode { get; }
        public string Title { get; }
        public string? Detail
        {
            set
            {
                if (value == null)
                {
                    _ = props.Remove(nameof(Detail));
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
        public Uri? Instance
        {
            set
            {
                if (value == null)
                {
                    _ = props.Remove(nameof(Instance));
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

        public void Add(string key, object value) => props.Add(key, value);

        public void Add(KeyValuePair<string, object> item) => props.Add(item);

        public void Clear() => props.Clear();

        public bool Contains(KeyValuePair<string, object> item) => props.Contains(item);

        public bool ContainsKey(string key) => props.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => props.CopyTo(array, arrayIndex);

        public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            => props.GetEnumerator();

        public bool Remove(string key) => props.Remove(key);

        public bool Remove(KeyValuePair<string, object> item) => props.Remove(item);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
            => props.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)props).GetEnumerator();
        #endregion
    }
}
