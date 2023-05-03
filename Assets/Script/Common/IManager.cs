namespace Assets.Script.Manager
{
    interface IManager
    {
        public abstract void Load();
        public abstract void Init(GameManager gameManager);
        public abstract void Update();
    }
}
