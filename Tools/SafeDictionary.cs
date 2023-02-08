namespace Chomp.Tools {
    public class SafeDictionary<K, V> : Dictionary<K, V> where K : notnull {
        public new virtual V? this[K key] {
            get => this.TryGetValue(key, out V value) ? value : default;
            set => base[key] = value;
        }
        public SafeDictionary(IEnumerable<KeyValuePair<K, V>> emum) : base(emum) { }
    }
}
