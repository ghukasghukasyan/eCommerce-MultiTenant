namespace ClientLibrary.States
{
    public class CartState
    {
        public int Count { get; private set; }

        public event Action OnChange;

        public void SetCount(int count)
        {
            Count = count;
            NotifyStateChanged();
        }
        public void Increment(int amount = 1)
        {
            Count += amount;
            NotifyStateChanged();
        }
        public void Decrement(int amount = 1)
        {
            Count = Math.Max(0, Count - amount);
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
